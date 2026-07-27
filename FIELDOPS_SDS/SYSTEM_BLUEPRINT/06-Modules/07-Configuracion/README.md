# Módulo: Configuración

> Parametrización multiempresa, roles y permisos, feature flags y configuración general del sistema.

**Estado:** `Draft v0.1`
**Depende de:** `03-Domain-Model/README.md`
**De este documento dependen:** **todos los demás módulos** (toda entidad de negocio lleva `company_id` y pasa por RBAC definido aquí)

---

## Purpose

Especificar el bounded context que sostiene el aislamiento multiempresa y
el control de acceso de todo el sistema — ningún otro módulo puede
funcionar sin que `Company`, `User`, `Role` y `Permission` existan primero.

## Vision

Que dar de alta una empresa nueva (onboarding) y su primer usuario
administrador sea un flujo de minutos, y que asignar o quitar acceso a
alguien sea inmediato y auditable.

## Scope

**Dentro:** `Company` (tenant), `Partner` (reventa, confirmado en el MVP),
`User`, `Membership`, `Role`, `Permission`, `FeatureFlag`.
**Fuera:** lógica de negocio de cada módulo (cada uno documenta sus propios
permisos específicos en su tabla de Permissions, este módulo define el
mecanismo, no cada regla puntual).

## Bounded Context

**Identity & Tenancy** — quién es la empresa, quién es el usuario, qué
puede hacer. **Decisión (resuelve `03-Domain-Model/` Open Question #2):
se documenta como bounded context de negocio propio**, no como
infraestructura transversal invisible — `Company`, `User` y `Role` son
entidades con ciclo de vida y reglas de negocio reales (alta, invitación,
revocación), no solo configuración técnica.

## Entities

| Entidad | Notas |
|---|---|
| `Company` | el tenant — límite de aislamiento de todo el sistema, ver Business Rules |
| `Partner` | **nuevo** — administra una o más `Company` bajo un mismo contrato (modelo de reventa, confirmado MVP) |
| `User` | puede tener acceso a más de una `Company` vía `Membership` (ver Business Rules) |
| `Membership` | **nuevo** — vincula un `User` a una `Company` con un `Role` específico para esa empresa |
| `Role` | agrupa `Permission` |
| `Permission` | acción concreta sobre un recurso/módulo |
| `FeatureFlag` | habilita/deshabilita capacidades por `Company`, reflejando su plan (`02-Business/`) |

## Relationships

- `Partner` 1—* `Company` (**opcional** — una `Company` puede no tener
  `Partner` y ser autónoma, o pertenecer a uno).
- `User` 1—* `Membership`; `Membership` *—1 `Company`; `Membership` *—1
  `Role` (el rol es **por membership**, no global al usuario — un mismo
  `User` puede ser Admin en una empresa y Supervisor en otra).
- `Role` *—* `Permission`.
- Todo `FeatureFlag` de una `Company` refleja los límites de su
  `Subscription` (externo, `06-Modules/05-Finanzas`/`02-Business/`).

## Commands

`CreateCompany` (onboarding), `UpdateCompany`, `CreatePartner`,
`LinkCompanyToPartner`, `UnlinkCompanyFromPartner`, `InviteUser`,
`CreateMembership` (otorga acceso de un `User` a una `Company` con un
`Role`), `RevokeMembership`, `ActivateUser`, `DeactivateUser`, `CreateRole`,
`AssignRole`, `RevokeRole`, `ToggleFeatureFlag` (uso restringido, ver
Permissions).

## Queries

`GetCompanyById`, `ListCompaniesByPartner`, `ListMembershipsByUser`
(empresas a las que un usuario tiene acceso), `ListUsersByCompany`,
`GetUserPermissions` (scoped a la `Company` activa de la sesión),
`ListFeatureFlagsByCompany`, `ListRoles`.

## Events

**Emitidos:** `CompanyCreated`, `PartnerCreated`, `CompanyLinkedToPartner`,
`UserInvited`, `MembershipCreated`, `MembershipRevoked`, `UserActivated`,
`UserDeactivated`, `RoleAssigned`, `FeatureFlagToggled`.
**Consumidos:** `SubscriptionUpgraded`/`SubscriptionDowngraded` (externo,
`02-Business/`/`06-Modules/05-Finanzas`, dispara recálculo automático de
`FeatureFlag` según el nuevo plan — la suscripción se factura por
`Company` individual o por `Partner` consolidado, ver Open Questions).

## Business Rules

1. **Un `User` puede tener acceso a más de una `Company`** a través de
   `Membership` (decisión confirmada — reemplaza la regla original de 1:1
   estricto). El aislamiento de datos sigue siendo por **`Company` activa
   en la sesión**, no por usuario: al autenticarse, un `User` con más de un
   `Membership` selecciona su empresa activa, y toda acción posterior
   queda scoped a ese `company_id` exactamente igual que si tuviera una
   sola empresa (el middleware de `04-Architecture/README.md` regla #4 no
   cambia su lógica, solo la fuente del valor).
2. Un `Partner` puede administrar varias `Company`, pero cada `Company`
   sigue siendo el límite de aislamiento de datos — un `Partner` **no ve
   datos cruzados entre sus empresas** en una sola vista salvo un reporte
   consolidado explícito (relacionado con `06-Modules/06-BI/` Open
   Question #2).
3. `FeatureFlag` se recalcula automáticamente al cambiar de plan
   (`SubscriptionUpgraded`/`Downgraded`) — nunca se edita a mano salvo por
   soporte de plataforma en un caso excepcional auditado.
4. `Permission` se agrupa siempre en `Role`, y el `Role` es específico de
   cada `Membership` — no se asignan permisos individuales sueltos a un
   `User` salvo excepción auditada explícita.
5. `DeactivateUser` no borra al usuario ni su historial de auditoría en
   otros módulos (soft delete, regla heredada de `MASTER.md §3`) — solo
   revoca acceso. `RevokeMembership` es más granular: quita acceso a **una**
   `Company` sin afectar memberships del mismo usuario en otras.

## Permissions

| Acción | Admin de empresa | Admin de Partner | Soporte de plataforma | Usuario regular |
|---|---|---|---|---|
| Gestionar usuarios/roles de su propia empresa | ✅ | ✅ (de sus empresas) | ✅ (excepcional, auditado) | ❌ |
| Crear/vincular Company bajo su Partner | ❌ | ✅ | ✅ (excepcional) | ❌ |
| Ver datos consolidados cross-Company de su Partner | ❌ | Solo agregados (regla #2) | ✅ (excepcional, auditado) | ❌ |
| Editar `FeatureFlag` manualmente | ❌ | ❌ | ✅ (excepcional, auditado) | ❌ |
| Ver su propio perfil/memberships | ✅ | ✅ | ✅ | ✅ |

## Screens

- **Configuración de Empresa**: datos generales, moneda/país por defecto
  (ver `05-Database/` §Multimoneda), plan vigente (solo lectura, gestión de
  plan vive en `02-Business/`).
- **Selector de Empresa Activa**: visible en el header cuando un `User`
  tiene más de un `Membership` — cambia el contexto de toda la sesión.
- **Panel de Partner**: listado de `Company` administradas, alta de nueva
  empresa bajo el mismo `Partner`.
- **Gestión de Usuarios y Roles**: invitar, activar/desactivar, asignar rol
  por `Membership` (no global).
- **Onboarding Wizard**: alta de empresa nueva (independiente o bajo un
  `Partner` existente) y su primer administrador.

## Wireframes

Ver `14-UX/`. El Selector de Empresa Activa debe ser inmediato y visible —
un usuario de Partner nunca debe confundir en qué empresa está actuando
mientras ejecuta una acción.

## Forms

- **Invitar usuario:** email, empresa a la que se le da acceso
  (`Company`), rol inicial para esa empresa.
- **Crear rol personalizado:** nombre, selección de permisos agrupados por
  módulo.
- **Vincular empresa a Partner:** selección de `Company` existente o alta
  de una nueva bajo el `Partner`.

## Filters

Por rol, estado de usuario (activo/invitado/inactivo), por `Company` (para
vistas de Partner).

## Reports

Usuarios activos por empresa, empresas por Partner, uso de feature flags
por plan (insumo para `02-Business/` al evaluar límites de plan).

## KPIs

`# empresas activas`, `# usuarios activos promedio por empresa`, `# Partners
activos`, `# empresas promedio por Partner`.

## Notifications

Invitación de usuario (email, especifica a qué empresa), cambio de rol
(in-app), feature flag activado/desactivado por cambio de plan (in-app, a
Admin de la empresa), nueva empresa vinculada a un Partner (a Admin de
Partner).

## Automations

Recalculo automático de `FeatureFlag` al cambiar de plan (regla #3).

## AI

**N/A** — no se identifican casos de uso de IA propios de este módulo en
este draft.

## APIs

`/companies`, `/partners`, `/memberships`, `/users`, `/roles`,
`/permissions`, `/feature-flags`. Detalle en `08-Backend/`.

## Validations

- No se puede `InviteUser` con un email ya existente **con Membership
  activo en la misma empresa** — `UserAlreadyExists` (el mismo email sí
  puede tener Membership en otra empresa distinta, sin conflicto).
- No se puede `AssignRole` con un `Role` inexistente — `RoleNotFound`.
- No se puede `CreateMembership` duplicado (mismo `User` + `Company`).

## Errors

`CompanyNotFound`, `PartnerNotFound`, `UserAlreadyExists`,
`MembershipAlreadyExists`, `RoleNotFound`, `InsufficientPermissions`
(error transversal, usado por todos los módulos cuando RBAC rechaza una
acción), `NoActiveCompanySelected` (si un `User` con múltiples memberships
intenta operar sin haber elegido empresa activa).

## Acceptance Criteria

1. **Dado** una empresa nueva completando el onboarding, **cuando** se crea
   su primer usuario, **entonces** queda automáticamente asignado como
   Admin de esa empresa vía `Membership`, sin paso manual adicional de
   soporte.
2. **Dado** un `User` con `Membership` en dos empresas distintas,
   **cuando** inicia sesión, **entonces** el sistema le pide seleccionar la
   empresa activa antes de permitir cualquier acción que dependa de
   `company_id`.
3. **Dado** un cambio de plan (`SubscriptionUpgraded`), **cuando** el
   evento se procesa, **entonces** los `FeatureFlag` correspondientes se
   activan automáticamente sin que el Admin de la empresa tenga que
   solicitarlo.
4. **Dado** un `Partner` con tres `Company` vinculadas, **cuando** su Admin
   consulta el panel, **entonces** ve el listado de las tres empresas pero
   **no** una vista mezclada de datos operativos entre ellas (regla #2).

## Future Improvements

- Reportes consolidados cross-Company para Partners (hoy: regla #2 lo
  restringe a agregados explícitos, no vistas mezcladas libres).

## Open Questions

1. ¿La suscripción SaaS (`02-Business/`) se factura por `Company`
   individual o de forma consolidada al `Partner`? Afecta directamente
   `06-Modules/05-Finanzas/` Open Question #3.
2. ¿El rol "soporte de plataforma" (cross-tenant) es un `User` especial
   dentro de este mismo esquema o un sistema de acceso completamente
   separado, fuera de `05-Database/`? Afecta el diseño de auditoría en
   `13-Security/`.
3. ¿Qué tan agregados deben ser los reportes cross-Company de un Partner
   para no violar el aislamiento de datos (regla #2) — a nivel de qué
   granularidad exacta?
