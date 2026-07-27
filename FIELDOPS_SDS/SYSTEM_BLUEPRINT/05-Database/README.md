# 05-Database

> Modelo de datos completo: tablas, campos, tipos, índices, constraints, enums, estrategia multiempresa/multimoneda/multipaís y offline.

**Estado:** `Draft v0.1`
**Depende de:** `03-Domain-Model/README.md`, `04-Architecture/README.md`, `ADR-0001`
**De este documento dependen:** `06-Modules/*`, `08-Backend/`, `09-Mobile/`

---

## Purpose

Fijar el esquema físico de PostgreSQL derivado 1:1 de las entidades ya
acordadas en `03-Domain-Model/`, sin introducir conceptos nuevos, y resolver
cómo ese esquema soporta la escala objetivo (100k empresas, 5M activos, 50M
OT), multiempresa, multimoneda, multipaís y sincronización offline.

## Responsibilities

Cubre: tablas core, tipos de datos, estrategia de tenant isolation,
particionamiento/índices para volumen, y el mecanismo físico del read-model
de BI (ADR-0001). **No cubre**: lógica de negocio (→ cada
`06-Modules/*`), ni el bus de eventos en sí (→ `04-Architecture/`,
`08-Backend/`).

## Scope

Esquema transaccional completo derivado de las 7 entidades-raíz de
`03-Domain-Model/` más sus entidades asociadas, y el esquema/almacén
separado para BI. No fija aún el proveedor cloud de hosting (→
`12-Infrastructure/`).

## Functional Description

### Estrategia multiempresa (tenant isolation)

**Fila compartida con `company_id` obligatorio** (no schema-per-tenant ni
database-per-tenant) — elegido por simplicidad operativa a 100k tenants:
tener 100k schemas o bases sería inviable de mantener. El aislamiento se
garantiza con:
- `company_id` como columna NOT NULL en toda tabla de negocio, indexada
  como parte de la clave de casi todos los índices compuestos.
- Row-Level Security (RLS) de PostgreSQL activado por tabla, filtrando por
  `company_id` **de la sesión activa** — no del usuario directamente, ya
  que un `User` puede tener acceso a más de una `Company` (modelo de
  reventa/partner, ver `06-Modules/07-Configuracion/README.md`). El valor
  de `company_id` en la sesión se fija al elegir la empresa activa, no al
  autenticarse.

### Multiempresa: tablas `partner` y `user_company_membership`

Confirmado el modelo de reventa/partner desde el MVP
(`06-Modules/07-Configuracion/README.md`):
- `partner`: entidad opcional que agrupa una o más filas de `company`.
- `company.partner_id`: FK nullable — una empresa puede no tener partner.
- `user_company_membership`: tabla puente `user_id` + `company_id` +
  `role_id`, reemplaza lo que en un diseño 1:1 hubiera sido un simple
  `company_id` en `app_user`. El rol es por membership, no global al
  usuario.

### Tablas core (derivadas de `03-Domain-Model/`, resumen — DDL completo pendiente)

| Tabla | Bounded Context | Notas de escala/tipo |
|---|---|---|
| `partner` | Identity & Tenancy | agrupa empresas bajo un mismo contrato de reventa (opcional) |
| `company` | Identity & Tenancy | ~100k filas, PK `company_id` (UUID), FK `partner_id` nullable |
| `app_user`, `user_company_membership`, `role`, `permission` | Identity & Tenancy | RBAC; el rol vive en el membership, no en el usuario |
| `client`, `contract` | CRM | `company_id` + índice por `client_id` |
| `asset` | Asset & Maintenance | ~5M filas — índice por `(company_id, client_id)` y por ubicación |
| `maintenance_plan`, `checklist` | Asset & Maintenance | referencian `asset_id` |
| `work_order` | Work Order | **~50M filas — partición por rango de fecha (mensual)**, índice por `(company_id, asset_id, status)` |
| `work_order_evidence` | Work Order | referencia `work_order_id`, almacenamiento de archivos vía Storage (no BLOB en Postgres) |
| `crew`, `dispatch` | Dispatch & Inventory | `dispatch` referencia `work_order_id` + `crew_id` |
| `inventory_item`, `warehouse` | Dispatch & Inventory | `company_id` + índice por `warehouse_id` |
| `technician`, `shift`, `skill`, `attendance` | Workforce | `company_id` obligatorio |
| `cost_entry` | Billing | referencia `work_order_id`, moneda (ver abajo) |
| `invoice`, `payment`, `account_receivable` | Billing | `company_id`, moneda, referencia `client_id` |

`work_order` es, por volumen, la tabla que determina la estrategia de
particionamiento del sistema completo — el resto puede vivir sin partición
en el MVP.

### Multimoneda / multipaís

**Lanzamiento inicial confirmado: Argentina, moneda base ARS** — el diseño
multipaís/multimoneda se mantiene desde el modelo de datos (no es
específico a Argentina), pero `company.default_currency = 'ARS'` y
`company.country_code = 'AR'` son el default de onboarding, no un valor
hardcodeado en el esquema. Toda tabla con montos (`cost_entry`, `invoice`,
`payment`) lleva columna `currency_code` (ISO 4217) junto al monto — nunca
un monto sin su moneda explícita en la misma fila. Conversión entre
monedas (si se necesita) es responsabilidad de `06-Modules/05-Finanzas/`,
no de este esquema.

### Offline (soporte a `09-Mobile/`)

Las tablas que el técnico necesita offline (`work_order` asignadas a su
`crew`, `checklist`, `work_order_evidence` en creación) se sincronizan por
un mecanismo de cola local → Outbox al reconectar (detalle del protocolo de
sync en `09-Mobile/`, no aquí). Este documento solo garantiza que toda
tabla sincronizable tiene una columna `updated_at` + `sync_version` para
resolver conflictos de escritura concurrente.

### Read-model de BI (ADR-0001)

Vive en un **esquema PostgreSQL separado** (`bi` schema, mismo clúster en
el MVP — evaluar réplica/motor separado si el volumen lo exige, Open
Question de `04-Architecture/`), poblado por proyecciones asíncronas desde
el Outbox. Sus tablas son vistas materializadas o tablas desnormalizadas
por KPI, no un espejo 1:1 del esquema transaccional.

## Business Rules

1. Ninguna tabla de negocio omite `company_id`, salvo catálogos
   explícitamente globales (p. ej. tabla de monedas ISO 4217).
2. `work_order` se particiona **mensualmente** por fecha desde el diseño
   inicial (decisión confirmada, ver Open Questions) — no se agrega
   partición después de llegar a producción con datos.
3. Soft delete obligatorio: toda tabla de negocio tiene `deleted_at`
   (nullable) en vez de `DELETE` físico (regla `MASTER.md §3` y Charter #3).
4. Auditoría obligatoria: toda tabla de negocio tiene `created_at`,
   `created_by`, `updated_at`, `updated_by`.
5. Ningún monto se almacena sin `currency_code` en la misma fila.

## Data Model

Ver "Tablas core" arriba. El DDL completo (columnas exactas, constraints,
enums de `work_order_status`, `dispatch_status`, etc.) se documenta como
migraciones versionadas cuando arranque `08-Backend/` — este documento fija
el contrato, no el script.

## UX

N/A — vive en `14-UX/` y cada `06-Modules/*`.

## Security

RLS por `company_id` en toda tabla de negocio (ver Functional Description).
Cifrado at-rest a nivel de proveedor cloud (definir en `12-Infrastructure/`).
Detalle completo de manejo de datos sensibles en `13-Security/`.

## API

N/A — vive en `08-Backend/`.

## Events

N/A como mecanismo (vive en `04-Architecture/`) — pero toda tabla que
participa de un evento de dominio (`03-Domain-Model/`) debe tener las
columnas necesarias para reconstruir el payload de ese evento sin joins
excesivos (p. ej. `work_order` trae denormalizado lo mínimo para publicar
`WorkOrderClosed` sin 5 joins).

## Dependencies

Depende de `03-Domain-Model/README.md` (entidades) y `04-Architecture/README.md`
(dónde vive el read-model de BI, ADR-0001). De este documento dependen
`08-Backend/` (migraciones concretas) y `09-Mobile/` (protocolo de sync
sobre las columnas `updated_at`/`sync_version` aquí fijadas).

## Future Improvements

- Evaluar partición adicional de `asset` por `company_id` (hash) si 5M de
  filas empieza a mostrar degradación en índices compuestos.
- Evaluar mover el read-model de BI a un motor OLAP dedicado si el volumen
  de KPIs crece más allá de lo que vistas materializadas en Postgres
  puedan sostener con buen rendimiento.

## Open Questions

1. **Resuelto:** particionamiento de `work_order` — **mensual** (ver
   Business Rules #2 y Functional Description).
2. **Resuelto:** el read-model de BI queda en el **mismo clúster
   PostgreSQL, schema `bi` separado** (ver §Read-model de BI) — no se
   introduce un motor/réplica distinta en el MVP.
3. ¿Se requiere retención/archivado de `work_order` histórica (>N años) o
   se conserva todo indefinidamente? Afecta el diseño de partición y
   políticas de archivado.
