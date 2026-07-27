# Módulo: Logística

> Inventario de repuestos, rutas de campo y despacho/asignación de cuadrillas a órdenes de trabajo.

**Estado:** `Draft v0.1`
**Depende de:** `03-Domain-Model/README.md`, `06-Modules/02-Operaciones` (consume `WorkOrderCreated`), `06-Modules/04-RRHH` (disponibilidad de técnicos)
**De este documento dependen:** `06-Modules/02-Operaciones` (habilita `StartWorkOrder`), `09-Mobile/`

---

## Purpose

Especificar cómo una `WorkOrder` creada en Operaciones llega a tener una
cuadrilla asignada y los repuestos disponibles antes de que el técnico
pueda iniciarla — sin este módulo, `StartWorkOrder` nunca se habilita
(regla #3 de `06-Modules/02-Operaciones`).

## Vision

Que un coordinador asigne la cuadrilla correcta a cada OT en segundos, sin
llamar a nadie para saber quién está libre y dónde, y que un técnico nunca
llegue a un sitio sin el repuesto que necesita.

## Scope

**Dentro:** asignación de OT a cuadrillas (`Dispatch`), inventario de
repuestos por bodega (`InventoryItem`, `Warehouse`), rutas de campo básicas.
**Fuera:** quién compone la cuadrilla y su disponibilidad horaria (→
`06-Modules/04-RRHH`), ejecución misma de la OT (→ `06-Modules/02-Operaciones`),
costo del repuesto usado (→ `06-Modules/05-Finanzas`).

## Bounded Context

**Dispatch & Inventory** — quién y con qué repuesto se ejecuta el trabajo.
No decide *qué* trabajo se hace (eso es Work Order) ni *quién puede*
trabajar (eso es Workforce/RRHH) — solo *asigna* uno a otro.

## Entities

| Entidad | Notas |
|---|---|
| `Crew` | grupo de uno o más `Technician` (referencia externa a RRHH) |
| `Dispatch` | asignación de una `WorkOrder` a un `Crew` en un momento dado |
| `InventoryItem` | repuesto/insumo, con cantidad disponible por `Warehouse` |
| `Warehouse` | bodega física o móvil (vehículo de cuadrilla) |
| `Route` | secuencia de sitios/OT asignadas a una `Crew` en una jornada |

## Relationships

- `Dispatch` referencia exactamente una `WorkOrder` (externa) y un `Crew`.
- `Crew` se compone de uno o más `Technician` (externo, RRHH) — Logística
  no es dueña de esa entidad, solo la referencia.
- `Route` agrupa varios `Dispatch` de un mismo `Crew` en una jornada.
- `InventoryItem` pertenece a un `Warehouse`; un `Dispatch` puede reservar
  `InventoryItem` (repuesto esperado para esa OT).

## Commands

`CreateCrew`, `AssignWorkOrderToCrew` (crea `Dispatch`), `ReassignWorkOrder`
(cambia `Dispatch` antes de iniciar), `BuildRoute`, `ReserveInventoryItem`,
`ConsumeInventoryItem` (al cerrar la OT, referencia consumo real vs
reservado), `ReceiveInventoryStock`.

## Queries

`ListAvailableCrews`, `GetDispatchByWorkOrder`, `ListRouteByCrewAndDate`,
`GetInventoryLevelByWarehouse`, `ListLowStockItems`.

## Events

**Emitidos:** `WorkOrderDispatched` (consumido por Operaciones, habilita
`StartWorkOrder`), `InventoryItemReserved`, `InventoryItemConsumed`,
`InventoryLowStock`.
**Consumidos:** `WorkOrderCreated` (de Operaciones, dispara la necesidad de
asignar), `WorkOrderClosed` (de Operaciones, dispara `ConsumeInventoryItem`
real vs. reservado), `TechnicianAvailabilityChanged` (de RRHH).

## Business Rules

1. `AssignWorkOrderToCrew` solo es válido si el `Crew` tiene al menos un
   `Technician` con disponibilidad confirmada por RRHH para ese horario.
2. `ReassignWorkOrder` solo es válida antes de que Operaciones reciba
   `StartWorkOrder` — una vez iniciada la ejecución, un cambio de cuadrilla
   pasa por `ReopenWorkOrder` en Operaciones, no por reasignación directa.
3. `ReserveInventoryItem` no garantiza el repuesto — es una intención; el
   consumo real se confirma en `ConsumeInventoryItem` al cierre de la OT,
   pudiendo diferir (repuesto adicional usado, o no usado).
4. `InventoryLowStock` se emite automáticamente al cruzar un umbral
   configurable por `InventoryItem` — no requiere revisión manual periódica.
5. Una `Route` no reordena automáticamente sus `Dispatch` si se agrega una
   OT urgente a mitad de jornada — requiere confirmación explícita del
   coordinador (evitar que el sistema reordene la ruta de un técnico ya en
   camino sin que nadie lo confirme).

## Permissions

| Acción | Admin | Supervisor/Coordinador | Técnico | RRHH |
|---|---|---|---|---|
| Asignar/reasignar OT a cuadrilla | ✅ | ✅ | ❌ | ❌ |
| Ver su propia ruta del día | — | ✅ (todas) | ✅ (solo la suya) | ❌ |
| Gestionar inventario/bodega | ✅ | ✅ | Consultar disponibilidad | ❌ |
| Confirmar disponibilidad de técnico | ❌ | Consultar | ❌ | ✅ |

## Screens

- **Panel de Despacho** (web, coordinador): OT pendientes de asignar junto
  a cuadrillas disponibles, asignación por arrastrar-soltar.
- **Mapa de Rutas del día**: cuadrillas y sus OT asignadas georreferenciadas.
- **Inventario por Bodega**: niveles de stock, alertas de bajo stock.
- **Mobile — Mi Ruta**: orden de sitios a visitar en el día, con navegación.

## Wireframes

Ver `14-UX/`. El Panel de Despacho es la pantalla más crítica del módulo —
debe mostrar disponibilidad real de cuadrillas sin refrescar, consumiendo
`TechnicianAvailabilityChanged` de RRHH en tiempo real.

## Forms

- **Asignación de OT a cuadrilla:** selección de `Crew`, confirmación de
  repuesto a reservar (opcional).
- **Recepción de inventario:** ítem, cantidad, bodega destino.

## Filters

Por cuadrilla, bodega, estado de despacho (pendiente/asignado/en ruta),
nivel de stock (bajo/normal), fecha de ruta.

## Reports

Tiempo promedio entre `WorkOrderCreated` y `WorkOrderDispatched` (eficiencia
de asignación), rotación de inventario por bodega, cuadrillas con mayor
carga de OT.

## KPIs

`Tiempo medio de asignación`, `% OT reasignadas` (indicador de fricción en
planificación), `# alertas de bajo stock activas`.

## Notifications

Bajo stock de repuesto crítico (a Coordinador), nueva OT asignada (a
Técnico, comparte disparador con `06-Modules/02-Operaciones`), cambio de
ruta confirmado (a Técnico).

## Automations

`InventoryLowStock` automático por umbral (regla #4). Sugerencia (no
asignación automática) de la cuadrilla más cercana/disponible al crear el
panel de despacho — decisión final siempre humana en el MVP (ver AI).

## AI

Candidato para `10-AI/`: sugerencia de cuadrilla óptima por cercanía,
disponibilidad y carga actual (no asignación automática sin confirmación
humana en el MVP — ver Future Improvements para asignación autónoma).

## APIs

`/crews`, `/dispatches`, `/routes`, `/inventory-items`, `/warehouses`.
Detalle de contratos en `08-Backend/`.

## Validations

- No se asigna `Dispatch` a un `Crew` sin disponibilidad confirmada (regla #1).
- No se reasigna una OT ya iniciada (regla #2) — el error correcto es
  `WorkOrderAlreadyStarted`.

## Errors

`CrewNotAvailable`, `WorkOrderAlreadyStarted` (al intentar reasignar),
`InventoryItemInsufficientStock` (al confirmar consumo real superior a lo
reservado, no bloquea el cierre de OT pero genera alerta).

## Acceptance Criteria

1. **Dado** una `WorkOrder` recién creada, **cuando** el coordinador la
   asigna a un `Crew` sin disponibilidad confirmada por RRHH, **entonces**
   el sistema rechaza la asignación con `CrewNotAvailable`.
2. **Dado** una `WorkOrder` ya iniciada (`StartWorkOrder` ejecutado),
   **cuando** se intenta `ReassignWorkOrder`, **entonces** el sistema
   rechaza con `WorkOrderAlreadyStarted`.
3. **Dado** un `InventoryItem` que cruza su umbral mínimo, **cuando** se
   confirma un consumo que lo reduce por debajo del umbral, **entonces** se
   emite `InventoryLowStock` sin intervención manual.

## Future Improvements

- Asignación automática (no solo sugerida) de cuadrilla cuando el volumen
  de OT lo justifique, con reglas de override humano.
- Optimización de rutas multi-parada (hoy: orden simple, no
  ruteo óptimo tipo VRP).

## Open Questions

1. ¿Una `Crew` puede tener OT de más de un `Warehouse`/bodega móvil
   simultáneamente, o cada cuadrilla opera desde una única bodega asignada?
2. ¿El umbral de `InventoryLowStock` es global por tipo de ítem o
   configurable por tenant/bodega individualmente?
