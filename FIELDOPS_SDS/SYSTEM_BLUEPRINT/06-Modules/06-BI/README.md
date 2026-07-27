# Módulo: BI

> Tableros ejecutivos y operativos embebidos (Power BI) sobre los datos de todos los módulos.

**Estado:** `Draft v0.1`
**Depende de:** `ADR-0001` (BI como read-model, no bounded context de negocio), todos los `06-Modules/*` (fuente de eventos)
**De este documento dependen:** ninguno — es hoja terminal del sistema (solo lectura)

---

## Purpose

Especificar cómo se construyen los tableros y KPIs consolidados a partir de
los eventos de los demás módulos, sin duplicar su lógica de negocio — en
línea con `ADR-0001`.

## Vision

Que Gerencia consuma tableros confiables sin pedirle un reporte a nadie, y
que cualquier número mostrado pueda trazarse hasta el evento que lo generó.

## Scope

**Dentro:** definición de dashboards, KPIs consolidados cross-módulo,
mecanismo de proyección (read-model). **Fuera:** entidades de negocio
propias (prohibido por `ADR-0001`), reportes ad-hoc totalmente
personalizables por usuario final (marcado como mejora futura).

## Bounded Context

**N/A por diseño** (`ADR-0001`): BI no es un bounded context de negocio —
es el lado *Query* de CQRS sobre los demás contextos. Este documento
existe para especificar *qué* se proyecta y *desde dónde*, no para
introducir un dominio nuevo.

## Entities

**N/A — ver `ADR-0001`.** En su lugar, este módulo define **proyecciones**
(read-models), no entidades de negocio:

| Proyección | Alimentada por (eventos) |
|---|---|
| `OperationalDashboardView` | `WorkOrderCreated`, `WorkOrderClosed`, `MaintenancePlanTriggered` |
| `AssetHealthView` | `AssetCreated`, `AssetRetired`, `WorkOrderClosed` |
| `FinancialDashboardView` | `CostEntryGenerated`, `InvoiceIssued`, `PaymentRegistered`, `InvoiceOverdue` |
| `WorkforcePerformanceView` | `TechnicianAvailabilityChanged`, `AttendanceRegistered` |
| `CommercialPipelineView` | `OpportunityWon`, `OpportunityLost`, `ContractRenewed` |

## Relationships

Cada proyección es unidireccional: consume eventos de un bounded context
fuente y nunca escribe de vuelta hacia él (regla de negocio #3).

## Commands

**N/A de negocio.** Único comando de este módulo es técnico/administrativo:
`RebuildProjection` (reconstruir una proyección desde el histórico de
eventos, uso excepcional — p. ej. tras un bug de proyección).

## Queries

`GetOperationalDashboard`, `GetAssetHealthReport`, `GetFinancialDashboard`,
`GetWorkforcePerformanceReport`, `GetCommercialPipelineReport` — todas de
solo lectura contra las proyecciones, nunca contra las tablas
transaccionales de origen (`ADR-0001`).

## Events

**Emitidos:** ninguno de negocio (posible evento técnico `ProjectionRebuilt`).
**Consumidos:** todos los eventos de dominio listados en la tabla de
"Entities" arriba — este módulo es, por diseño, el mayor consumidor de
eventos del sistema.

## Business Rules

1. Todo KPI mostrado debe poder trazarse al evento/entidad de origen que lo
   generó — ningún KPI se calcula con una regla de negocio que no exista ya
   documentada en el módulo fuente.
2. Las proyecciones se actualizan de forma asíncrona (eventual
   consistency) — cada dashboard declara un SLA de frescura de datos (ver
   Open Questions para el valor exacto).
3. BI nunca escribe a los módulos fuente — es estrictamente unidireccional
   (regla ya anticipada en `04-Architecture/README.md` regla #3).

## Permissions

| Acción | Gerencia/Ejecutivo | Finanzas | Supervisor | Admin |
|---|---|---|---|---|
| Ver Dashboard Ejecutivo (todos los KPIs) | ✅ | ❌ | ❌ | ✅ |
| Ver Dashboard Financiero | ✅ | ✅ | ❌ | ✅ |
| Ver Dashboard Operativo | ✅ | ❌ | ✅ (de su ámbito) | ✅ |
| Ejecutar `RebuildProjection` | ❌ | ❌ | ❌ | ✅ (soporte técnico) |

## Screens

Dashboard Ejecutivo, Dashboard Operativo, Dashboard Financiero, Dashboard
de Fuerza de Trabajo — todos embebidos vía Power BI (`MASTER.md §3`),
filtrables por periodo y, si corresponde, por cliente o tipo de activo.

## Wireframes

Ver `14-UX/`. Inspiración explícita en Power BI/Linear (`01-Product/README.md`
§UX) — widgets, filtros y gráficos, nunca tablas crudas sin contexto visual.

## Forms

**N/A** — BI no captura datos, solo los consulta y filtra.

## Filters

Por periodo, cliente, tipo de activo, técnico/cuadrilla — cruzando
proyecciones de distintos módulos fuente en una sola vista cuando el
dashboard lo requiera (p. ej. rentabilidad por cliente cruza Finanzas+CRM).

## Reports

Reportes consolidados cross-módulo: rentabilidad por cliente (Finanzas +
CRM), cumplimiento vs. costo (Operaciones + Finanzas), carga de trabajo por
cuadrilla (Logística + RRHH).

## KPIs

Consolidación de todos los KPIs ya definidos por módulo fuente
(`06-Modules/02-Operaciones` §KPIs, `06-Modules/03-Logistica` §KPIs, etc.) —
este módulo no inventa KPIs nuevos sin anclarlos a un módulo fuente,
salvo los explícitamente cross-módulo listados en "Reports".

## Notifications

Alerta de KPI fuera de umbral configurado (p. ej. MTTR sube más de X% mes a
mes) — notificación a Gerencia/Supervisor según el dashboard afectado.

## Automations

Recalculo automático de cada proyección al recibir su evento fuente (regla
#2). Alertas de umbral (ver Notifications) evaluadas en cada actualización
de proyección.

## AI

Candidato para `10-AI/`: detección de anomalías en KPIs (caídas o subidas
atípicas) e insights automáticos en lenguaje natural sobre tendencias — no
MVP.

## APIs

`/bi/dashboards`, `/bi/kpis` — estrictamente de solo lectura, sin
comandos de negocio expuestos.

## Validations

**N/A** — al no haber comandos de negocio, no hay validaciones de negocio
que aplicar en este módulo (más allá de RBAC sobre qué dashboard puede ver
cada rol).

## Errors

`ProjectionStale` (si el lag de una proyección supera el SLA definido —
ver Open Questions), `DashboardNotFound`.

## Acceptance Criteria

1. **Dado** un `WorkOrderClosed` emitido por Operaciones, **cuando** se
   procesa, **entonces** `OperationalDashboardView` refleja el cambio
   dentro del SLA de frescura definido, sin intervención manual.
2. **Dado** un KPI mostrado en cualquier dashboard, **cuando** se audita su
   origen, **entonces** se puede trazar a un evento y módulo fuente
   documentado — nunca a un cálculo ad-hoc sin trazabilidad.
3. **Dado** un usuario con rol Supervisor, **cuando** intenta acceder al
   Dashboard Financiero, **entonces** el sistema lo rechaza por RBAC (no
   tiene el permiso listado en la tabla de Permissions).

## Future Improvements

- Reportes ad-hoc personalizables por el usuario final (hoy: dashboards
  predefinidos únicamente).
- Insights automáticos vía IA (ver sección AI).

## Open Questions

1. ¿Cuál es el SLA de frescura de datos aceptable por dashboard (segundos,
   minutos)? Bloquea el diseño técnico de la proyección en `08-Backend/`.
2. ¿Se permite consolidar métricas cross-tenant para un eventual partner o
   reseller? (heredada de `02-Business/` Open Question #4 — si ese modelo
   se confirma, este módulo necesita una vista adicional agregada).
