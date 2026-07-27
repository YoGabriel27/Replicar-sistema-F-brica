# 08-Backend

> Arquitectura backend: servicios, capas (dominio/aplicación/infraestructura), estructura de proyectos .NET/FastAPI, convenciones de API.

**Estado:** `Draft v0.1`
**Depende de:** `04-Architecture/README.md`, `05-Database/README.md`, `ADR-0002`, todos los `06-Modules/*`
**De este documento dependen:** `07-Frontend/` (contrato API), `09-Mobile/` (endpoint de sync), `10-AI/` (orquestación de servicios de IA), `12-Infrastructure/`

---

## Purpose

Traducir la arquitectura de `04-Architecture/` y la decisión de `ADR-0002`
(.NET 9) en una estructura de solución concreta, resolviendo las preguntas
abiertas que quedaron pendientes ahí: monolito vs. microservicios y el
transporte concreto del Outbox.

## Responsibilities

Cubre: estructura de la solución .NET, convención de capas por módulo,
implementación concreta de CQRS/Outbox, middleware transversal (tenant,
auth). **No cubre**: esquema de tablas (→ `05-Database/`), reglas de
negocio por módulo (→ `06-Modules/*`), ni topología de despliegue (→
`12-Infrastructure/`).

## Scope

Backend transaccional completo (.NET 9) y el punto de integración con los
servicios de IA en Python/FastAPI (consecuencia de `ADR-0002`). No cubre el
almacén de lectura de BI en detalle físico (eso vive en `05-Database/`).

## Functional Description

### Decisión: monolito modular (resuelve `04-Architecture/` Open Question #1)

Un único servicio desplegable en el MVP, con los bounded contexts como
**módulos internos** con límites de código estrictos — no microservicios
separados desde el día 1. Los límites ya trazados por bounded context
(`03-Domain-Model/`) son exactamente las costuras por donde se extraería
un módulo a servicio independiente si la escala lo exige más adelante.

```
src/
├── Modules/
│   ├── Configuracion/       (Identity & Tenancy)
│   │   ├── Domain/
│   │   ├── Application/     (Commands, Queries, Handlers — CQRS)
│   │   ├── Infrastructure/   (Repository, EF Core, Outbox writer)
│   │   └── Api/              (Controllers REST + OpenAPI)
│   ├── CRM/          (misma estructura de 4 capas)
│   ├── Operaciones/  (Asset & Maintenance + Work Order)
│   ├── Logistica/    (Dispatch & Inventory)
│   ├── RRHH/         (Workforce)
│   ├── Finanzas/     (Billing)
│   └── BI/            ← sin Domain/ (ADR-0001): solo Application (Queries)
│                          + Infrastructure (lectura de proyecciones)
├── Shared/
│   ├── Kernel/         (entidades base, Specification, Repository genérico)
│   ├── Tenancy/         (middleware de resolución de company_id)
│   └── Outbox/           (infraestructura común de publicación de eventos)
└── Host/                  (composition root — ASP.NET Core Web API,
                             registra todos los módulos, un solo proceso)
```

Ningún `Module` referencia el `Infrastructure` de otro directamente — solo
se comunican vía eventos (Outbox) o consultando el `Api` público del otro
módulo.

### Decisión: transporte del Outbox (resuelve `04-Architecture/` Open Question #3)

**Patrón Outbox sobre PostgreSQL + relay a Redis Streams** — se reutiliza
Redis (ya en el stack para cache, `MASTER.md §3`) en vez de introducir un
broker dedicado nuevo en el MVP. Un `BackgroundService` (Hosted Service)
lee la tabla `outbox_event` recién comiteada en la misma transacción que el
cambio de negocio, y la publica a un stream de Redis; los consumidores
(incluida la proyección de BI) leen de ahí.

### CQRS con MediatR (o equivalente)

Cada módulo separa `Commands` (escritura, un Handler cada uno) de
`Queries` (lectura, pueden leer directo de proyecciones donde aplique).
Ningún Controller contiene lógica de negocio — solo despacha al mediador.

## Business Rules

Convenciones obligatorias de este backend:

1. Todo Handler de `Application` valida los invariantes de dominio
   (`03-Domain-Model/README.md` §Business Rules) antes de persistir — nunca
   se salta la capa de Dominio por conveniencia.
2. Ningún módulo llama directo a la base de datos ni al código de
   `Infrastructure` de otro módulo — solo vía evento (Outbox) o API pública.
3. Todo `DbContext` aplica un **global query filter por `company_id`** en
   toda entidad de negocio — ninguna query individual debe filtrar
   manualmente (defensa en profundidad junto a RLS de `05-Database/`).
4. Todo endpoint requiere autenticación salvo una lista explícita y
   documentada de excepciones (health check, login).

## Data Model

N/A en detalle — las migraciones de EF Core se derivan 1:1 de
`05-Database/README.md`, sin agregar columnas o tablas no documentadas ahí
sin actualizar ese documento primero.

## UX

N/A — vive en `07-Frontend/` y `09-Mobile/`.

## Security

Autenticación JWT + OAuth2, autorización por política (policy-based
authorization de ASP.NET Core) resuelta contra `Role`/`Permission` del
módulo Configuración. Detalle completo en `13-Security/`.

## API

REST + OpenAPI por módulo, expuesto a través de un API Gateway/BFF único
(`04-Architecture/README.md`). Versionado de API: a definir (ver Open
Questions).

## Events

Outbox → Redis Streams (ver Functional Description). Consumidores:
proyecciones de BI (`06-Modules/06-BI/`) y handlers cross-módulo (p. ej.
Finanzas escuchando `WorkOrderClosed` de Operaciones).

## Dependencies

Depende de `04-Architecture/` (patrones), `05-Database/` (esquema) y todos
los `06-Modules/*` (reglas de negocio a implementar). De este documento
dependen `07-Frontend/` (contrato API), `09-Mobile/` (endpoint de sync) y
`10-AI/` (cómo el backend invoca servicios de IA externos).

## Future Improvements

- Extracción de uno o más `Modules/` a microservicio independiente si el
  volumen de un bounded context específico lo justifica (las costuras ya
  están trazadas por diseño).
- Reemplazo de Redis Streams por un broker dedicado (Kafka o similar) si el
  volumen de eventos supera lo que Redis puede sostener con buen
  rendimiento.

## Open Questions

1. **Resuelto:** versionado de API **por URL** (`/v1/...`) — aplicado
   desde el primer contrato OpenAPI publicado por cada módulo.
2. ¿En qué punto de volumen de eventos se revisita el relay de Redis
   Streams hacia un broker dedicado? (criterio de decisión, no fecha fija).
3. **Resuelto (ver `10-AI/README.md`):** el servicio de IA en
   Python/FastAPI se despliega **junto al backend .NET, en el mismo
   despliegue** — no como servicio separado desde el día 1. Se orquesta
   como un proceso adicional dentro de la misma unidad de despliegue
   definida en `12-Infrastructure/`, manteniendo el límite de código
   (nunca de repositorio ni proceso) entre ambos runtimes.
