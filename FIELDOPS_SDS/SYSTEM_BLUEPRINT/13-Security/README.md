# 13-Security

> Seguridad transversal: autenticación, autorización, cifrado, cumplimiento normativo, auditoría, manejo de datos sensibles.

**Estado:** `Draft v0.1`
**Depende de:** `06-Modules/07-Configuracion` (RBAC), `04-Architecture/README.md`, `05-Database/README.md`
**De este documento dependen:** **todos los módulos y capas** — ninguno puede contradecir lo fijado aquí

---

## Purpose

Consolidar en un solo documento los requisitos de seguridad que hasta
ahora quedaron mencionados de forma dispersa en `MASTER.md`,
`00-Project-Charter/`, `04-Architecture/` y `05-Database/`, para que
autenticación, autorización, auditoría y cifrado se implementen de forma
consistente en todo el sistema.

## Responsibilities

Cubre: autenticación, autorización (RBAC), aislamiento multiempresa,
auditoría de acciones sensibles, cifrado, y consideraciones de
cumplimiento. **No cubre**: seguridad de infraestructura (firewalls, VPC,
gestión de secretos) — eso vive en `12-Infrastructure/`.

## Scope

Todo el sistema: backend, frontend web, app móvil y los servicios de IA
(`10-AI/`) cuando existan. Aplica a los 7 módulos de negocio por igual —
ninguno tiene una excepción propia sin pasar por un ADR.

## Functional Description

### Autenticación

JWT (access token de corta duración + refresh token) emitido por el
backend tras validar credenciales contra `User`/`Company`
(`06-Modules/07-Configuracion/`). **2FA (doble factor) es requisito desde
el MVP** (decisión confirmada) — no una mejora post-lanzamiento; el flujo
de login incluye un segundo factor (TOTP o SMS, a definir en detalle
técnico de `08-Backend/`) antes de emitir el access token. OAuth2
disponible para login de terceros si el negocio lo requiere (no
confirmado, ver Open Questions).

### Autorización (RBAC)

Policy-based authorization en el backend (`08-Backend/`), resuelta contra
`Role`/`Permission` de `06-Modules/07-Configuracion/`. El frontend
(`07-Frontend/README.md` regla #4) solo oculta acciones por UX — la
autoridad real vive exclusivamente en el backend.

### Aislamiento multiempresa (defensa en profundidad)

Dos capas independientes, ninguna sustituye a la otra:
1. Middleware de resolución de `company_id` (`04-Architecture/README.md`
   regla #4).
2. Row-Level Security de PostgreSQL por `company_id`
   (`05-Database/README.md` §Estrategia multiempresa).

### Auditoría

Toda tabla de negocio tiene columnas de auditoría estándar
(`created_at/by`, `updated_at/by`, `05-Database/README.md` regla #4).
**Adicionalmente**, las acciones sensibles listadas abajo requieren motivo
obligatorio y quedan en un **log de auditoría append-only** separado
(nunca se edita ni se soft-deletea un registro de este log):

- `ReopenWorkOrder` (Operaciones)
- `TerminateContract` (CRM)
- `VoidInvoice`, `ApproveOutOfContractBilling` (Finanzas)
- `ToggleFeatureFlag` manual (Configuración, uso excepcional de soporte)
- Cualquier acceso cross-tenant de soporte de plataforma
  (`06-Modules/07-Configuracion/` Open Question #2)

### Cifrado

TLS en tránsito en todas las comunicaciones (web, mobile, servicios
internos). Cifrado at-rest dependiente del proveedor cloud (definir en
`12-Infrastructure/`). Datos de pago, si se almacenan directamente en vez
de tokenizarse vía pasarela externa, requieren cifrado a nivel de campo
(ver Open Questions de `02-Business/` sobre alcance PCI).

## Business Rules

1. Ninguna de las acciones sensibles listadas arriba se ejecuta sin motivo
   obligatorio, registrado en el log de auditoría append-only.
2. RBAC se valida siempre en el backend — ocultar un botón en el frontend
   nunca es la única defensa.
3. Row-Level Security está activo en toda tabla de negocio sin excepción
   manual por consulta individual.
4. Ningún dato de tarjeta de pago se almacena en texto plano; si no se usa
   tokenización de una pasarela externa (`11-Integrations/`), se cifra a
   nivel de campo.
5. Ningún dato de un tenant se envía a un servicio de IA de terceros sin
   garantía contractual de no uso para entrenamiento compartido
   (heredado de `10-AI/README.md` regla #3).
6. **2FA es obligatorio para todo `User`** desde el MVP (decisión
   confirmada) — el login no está completo sin el segundo factor validado,
   sin excepción por rol.

## Data Model

Tabla de auditoría append-only (`audit_log` o equivalente) separada de las
columnas de auditoría estándar por fila — registra acción, actor, motivo,
timestamp y entidad afectada. Detalle de columnas exactas se define junto
con `05-Database/` al construir esta tabla.

## UX

N/A como pantallas propias — la seguridad se aplica, no se muestra, salvo
una eventual vista de "historial de auditoría" para Admin (mejora futura,
no MVP).

## Security

Este documento **es** la sección de seguridad transversal del blueprint —
no aplica una subsección adicional dentro de sí mismo.

## API

N/A — la implementación concreta (middlewares, policies) vive en
`08-Backend/`.

## Events

Acciones sensibles auditadas (ver Functional Description) generan un
registro en el log de auditoría de forma síncrona con la acción misma —
nunca de forma diferida ni opcional.

## Dependencies

Depende de `06-Modules/07-Configuracion/` (RBAC), `04-Architecture/`
(middleware) y `05-Database/` (RLS). Todos los módulos y capas dependen de
este documento — ninguno implementa su propia variante de autenticación,
autorización o auditoría.

## Future Improvements

- Single Sign-On (SSO/SAML) para clientes empresariales grandes.
- Vista de auditoría navegable para Admin de empresa (hoy el log existe,
  pero no tiene pantalla propia).

## Open Questions

1. ¿Qué certificaciones de cumplimiento aplican en Argentina (heredado de
   `00-Project-Charter/CHARTER.md` Open Question #4)? Bloquea el diseño
   final de cifrado at-rest y retención de datos.
2. **Pendiente:** ¿el procesamiento de pagos requiere alcance PCI-DSS
   directo, o se delega enteramente a una pasarela externa tokenizada?
   (heredado de `02-Business/README.md` Open Question #2).
3. **Resuelto:** 2FA es requisito de lanzamiento (regla #6) — impacta el
   flujo de autenticación de `07-Frontend/` y `09-Mobile/` desde el día 1.
