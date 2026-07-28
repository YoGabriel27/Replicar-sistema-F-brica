# Módulo: Operaciones

> Núcleo del sistema: activos, mantenimiento preventivo/correctivo y órdenes de trabajo (OT) de principio a fin.

**Estado:** `Draft v0.1`
**Depende de:** `03-Domain-Model/README.md`, `04-Architecture/README.md`, `05-Database/README.md`
**De este documento dependen:** `06-Modules/03-Logistica` (dispatch), `06-Modules/05-Finanzas` (costeo), `06-Modules/06-BI` (KPIs), `09-Mobile/`, `10-AI/`, `11-Integrations/` (envío de remitos por email)

---

## Purpose

Especificar el módulo que sostiene el resto del sistema: sin un `Asset` no
existe `Contract` que lo cubra (CRM), sin un `WorkOrder` cerrado no hay
`CostEntry` (Finanzas) ni KPI (BI) ni Dispatch (Logística) que ejecutar.
Este documento es la referencia obligatoria antes de tocar cualquier
pantalla, endpoint o tabla de Operaciones.

## Vision

Que un supervisor sepa en todo momento el estado de cada activo y cada OT
sin preguntarle a nadie, y que un técnico pueda ejecutar su trabajo sin
señal de red y sin fricción — un checklist, evidencia, y cerrar.

## Scope

**Dentro:** ciclo de vida completo del `Asset`, planes de mantenimiento
(`MaintenancePlan`), y el ciclo de vida completo de la `WorkOrder` desde su
creación (manual o automática) hasta su cierre con evidencia.
**Fuera:** quién ejecuta la OT y con qué repuesto (→ `06-Modules/03-Logistica`,
bounded context Dispatch & Inventory), costeo/facturación (→
`06-Modules/05-Finanzas`), disponibilidad del técnico (→ `06-Modules/04-RRHH`).

## Bounded Context

Este módulo cubre **dos** bounded contexts de `03-Domain-Model/`, a
propósito separados internamente aunque compartan módulo de producto:

- **Asset & Maintenance** — qué activos existen y qué mantenimiento requieren.
- **Work Order** — qué trabajo se ejecuta, cuándo y con qué evidencia.

Un activo existe y tiene historial aunque nunca se le haya abierto una OT.

## Entities

| Entidad | Contexto | Notas |
|---|---|---|
| `Asset` | Asset & Maintenance | pertenece a un `Client` y un `Company` (tenant) |
| `MaintenancePlan` | Asset & Maintenance | frecuencia (tiempo o uso), referencia `Asset` |
| `ChecklistTemplate` | Asset & Maintenance | **constructor de formularios dinámicos** (checklist, desplegables, firma) — un Admin lo arma sin programar; referenciada por `MaintenancePlan` y por `WorkOrder` |
| `WorkOrder` | Work Order | referencia `Asset`, tiene `WorkOrderStatus` |
| `WorkOrderEvidence` | Work Order | fotos (con estampado de GPS/fecha/hora), checklist completado, firma; referencia `WorkOrder` |
| `WorkOrderReceipt` | Work Order | **remito en PDF**, generado en el dispositivo al cierre, firmado por el cliente; referencia `WorkOrder` |

## Relationships

- `Asset` 1—* `WorkOrder` (un activo tiene muchas OT a lo largo del tiempo).
- `Asset` 1—* `MaintenancePlan` (un activo puede tener más de un plan, p. ej.
  preventivo mensual + revisión anual).
- `MaintenancePlan` genera `WorkOrder` automáticamente al vencer (evento
  `MaintenancePlanTriggered`, ver `03-Domain-Model/`).
- `WorkOrder` 1—* `WorkOrderEvidence`.
- `WorkOrder` 1—0..1 `WorkOrderReceipt` (el remito solo existe si la OT se
  cerró con firma del cliente — no toda OT tiene uno, p. ej. tareas sin
  presencia del cliente en el sitio).
- `WorkOrder` *—1 `Dispatch` (externo, `06-Modules/03-Logistica`) — Operaciones
  no decide quién ejecuta, solo consume el resultado de la asignación.
- `WorkOrder` 1—0..1 `CostEntry` (externo, `06-Modules/05-Finanzas`), generado
  al cierre.

## Commands

`CreateAsset`, `UpdateAsset`, `RetireAsset`, `CreateMaintenancePlan`,
`UpdateMaintenancePlan`, `CreateChecklistTemplate`, `UpdateChecklistTemplate`
(constructor de formularios — Admin arma campos: texto, checkbox,
desplegable, firma, foto, sin programar), `CreateWorkOrder` (manual),
`StartWorkOrder`, `AddWorkOrderEvidence`, `GenerateWorkOrderReceipt`
(genera el PDF/remito en el dispositivo al cierre), `ResendWorkOrderReceipt`
(reenvío por email desde el panel web), `CloseWorkOrder`, `CancelWorkOrder`,
`ReopenWorkOrder` (excepcional, requiere permiso de Supervisor).

## Queries

`GetAssetById`, `ListAssetsByClient`, `GetAssetMaintenanceHistory`,
`ListWorkOrdersByStatus`, `GetWorkOrderById`, `ListWorkOrdersDueSoon`,
`ListOverdueWorkOrders`, `ListWorkOrdersByTechnician` (proyectada desde
Dispatch, solo lectura aquí).

## Events

**Emitidos:** `AssetCreated`, `AssetRetired`, `MaintenancePlanTriggered`,
`WorkOrderCreated`, `WorkOrderStarted`, `WorkOrderClosed`,
`WorkOrderCancelled`, `WorkOrderReopened`, `WorkOrderReceiptGenerated`
(consumido por `06-Modules/05-Finanzas`/`11-Integrations` para el envío
por email desde el panel web).
**Consumidos:** `WorkOrderDispatched` (de Logística, habilita `StartWorkOrder`),
`TechnicianAssigned` (de RRHH/Logística, se refleja en la vista de la OT).

## Business Rules

1. Un `Asset` retirado (`RetireAsset`) no puede recibir nuevas `WorkOrder` —
   solo conserva su historial.
2. `CreateWorkOrder` desde un `MaintenancePlan` es automático al vencer la
   frecuencia definida — no depende de que un humano lo dispare.
3. `StartWorkOrder` solo es válido si la OT ya fue `WorkOrderDispatched` (no
   se puede iniciar trabajo sin cuadrilla asignada).
4. `CloseWorkOrder` **requiere** al menos una `WorkOrderEvidence` — invariante
   heredada de `03-Domain-Model/README.md` regla #3, sin excepción ni
   siquiera para roles de Administrador.
5. `CancelWorkOrder` solo es válido antes de `StartWorkOrder` — una OT en
   ejecución no se cancela, se cierra (con o sin éxito, documentado en la
   evidencia).
6. `ReopenWorkOrder` **sí existe** como capacidad del sistema (decisión
   confirmada): una OT cerrada puede reabrirse, siempre auditada — requiere
   motivo obligatorio (texto), queda registrada en el historial de estado
   de la OT (quién, cuándo, por qué) y emite `WorkOrderReopened`. Al
   reabrir, la OT vuelve a `En ejecución`, no a `Creada` — conserva su
   evidencia previa, no la borra.
7. Un `Asset` **puede tener varios `MaintenancePlan` activos del mismo
   tipo simultáneamente** (decisión confirmada) — p. ej. dos preventivos
   mensuales con checklists distintos por razones operativas distintas.
   Cada `MaintenancePlan` vence y genera OT de forma independiente; no hay
   deduplicación automática si dos planes vencen el mismo día para el
   mismo activo (se crean dos OT separadas, a propósito).
8. **Toda foto de `WorkOrderEvidence` lleva coordenadas GPS, fecha y hora
   estampadas de forma visible sobre la propia imagen** (no solo como
   metadato aparte) — la captura es exclusivamente por la cámara interna
   de la app (nunca desde la galería del dispositivo), para que el
   estampado no pueda evitarse.
9. `GenerateWorkOrderReceipt` se ejecuta **en el dispositivo, en el
   momento del cierre** — no es un reporte generado después en el
   servidor. Requiere la firma del cliente capturada en pantalla; si el
   cliente no está presente para firmar, la OT puede cerrarse igual
   (regla #4 ya cubre evidencia obligatoria) pero sin `WorkOrderReceipt`
   asociado.
10. `ChecklistTemplate` se arma con un constructor visual (campos de
    texto, checkbox, lista desplegable, firma, foto) — un Admin no
    requiere intervención de un programador para crear o modificar una
    plantilla.
11. **Un Técnico puede crear una OT "espontánea"** directamente desde el
    campo (aporte del colega — "partes de trabajo espontáneos") sin pasar
    por Supervisor: se auto-asigna a su propio `Crew` y **omite el flujo
    normal de `Dispatch`** de `06-Modules/03-Logistica` (no tiene sentido
    despachar algo a quien ya está en el sitio ejecutándolo). Emite
    `WorkOrderCreated` y `WorkOrderDispatched` en el mismo momento, no en
    dos pasos — sigue requiriendo `Asset` válido y no retirado (regla #1).

## Permissions

| Acción | Admin | Supervisor | Técnico | Comercial (CRM) | Cliente (portal) |
|---|---|---|---|---|---|
| Crear/editar Asset | ✅ | ✅ | ❌ | ❌ | ❌ |
| Ver historial de Asset | ✅ | ✅ | Solo asignados | ✅ (solo lectura) | Solo el suyo |
| Crear OT manual | ✅ | ✅ | ✅ (solo "espontánea", ver regla #11) | ❌ | ❌ |
| Iniciar/cerrar OT | ✅ | ✅ | Solo asignada | ❌ | ❌ |
| Reabrir OT (motivo obligatorio) | ✅ | ✅ | ❌ | ❌ | ❌ |

## Screens

- **Listado de Activos** (web, supervisor/admin): tabla filtrable estilo
  Linear/Notion, con estado de mantenimiento (al día / próximo a vencer /
  vencido) como badge visual.
- **Ficha de Activo**: historial completo de OT, planes de mantenimiento
  activos, documentos, **mapa con la ubicación del activo**.
- **Constructor de Formularios (Admin)**: arma/edita `ChecklistTemplate`
  arrastrando campos (texto, checkbox, desplegable, firma, foto) — sin
  código.
- **Tablero de Órdenes de Trabajo** (web): kanban por `WorkOrderStatus`
  (Creada → Despachada → En ejecución → Cerrada), filtrable por técnico,
  cliente, prioridad.
- **Detalle de OT**: checklist, evidencia, historial de estado,
  **visor de fotos en alta calidad** (zoom, sin recompresión agresiva),
  **mapa con la ubicación exacta de la tarea**, remito PDF si existe.
- **Repositorio de Remitos**: listado centralizado de `WorkOrderReceipt`,
  descarga y reenvío por email (ver `11-Integrations/`).
- **Mobile — Mis OT**: lista simple de OT asignadas al técnico, ordenadas
  por urgencia.
- **Mobile — Ejecutar OT**: un solo flujo lineal — checklist → fotos →
  firma → cerrar, funcional 100% offline; genera el remito en el
  dispositivo al cierre (regla #9).

## Wireframes

Referencia visual completa en `14-UX/` (sistema de diseño). Principio para
este módulo: el tablero de OT es la pantalla que más tiempo consume un
supervisor — debe cargar y filtrar sin recarga completa de página (React
Query + optimistic UI).

## Forms

- **Alta de Activo:** identificador, tipo, ubicación, cliente/contrato
  asociado, fecha de instalación, plan de mantenimiento inicial (opcional).
- **Creación manual de OT:** activo, prioridad, descripción, checklist a
  usar (heredado del plan de mantenimiento si aplica).
- **Checklist de ejecución (mobile):** dinámico, generado desde la
  plantilla `ChecklistTemplate` del `MaintenancePlan` o de la OT.

## Filters

Por estado de OT, activo, cliente, técnico asignado, rango de fecha,
prioridad, y "vencidas" como filtro rápido predefinido (regla de negocio,
no solo UI).

## Reports

Cumplimiento de mantenimiento preventivo por cliente/periodo, tiempo
promedio de resolución (MTTR) por tipo de activo, OT cerradas sin
evidencia (debe dar siempre cero — reporte de auditoría, no operativo).

## KPIs

`% cumplimiento de mantenimiento preventivo`, `MTTR` (tiempo medio de
resolución), `# OT vencidas`, `# OT abiertas por técnico`. Todos trazables
a eventos de este módulo (ADR-0001, regla de producto #4).

## Notifications

Mantenimiento próximo a vencer (a Supervisor, N días antes, configurable),
OT asignada (a Técnico), OT vencida sin iniciar (a Supervisor), evidencia
faltante al intentar cerrar (in-app, bloqueante, no solo aviso).

## Automations

`MaintenancePlan` → genera `WorkOrder` automáticamente al vencer frecuencia
(regla #2). Escalamiento automático: OT vencida N días sin iniciar notifica
a Supervisor y sube prioridad.

## AI

Candidatos a documentar en detalle en `10-AI/`, no se construyen aquí:
- Sugerencia de causa probable de falla basada en historial del activo al
  crear una OT correctiva.
- Mantenimiento predictivo (anticipar falla antes del plan fijo) — marcado
  como mejora futura, no MVP (ver Future Improvements).
- Asistencia en checklist: validar por foto que un paso del checklist se
  completó razonablemente (no reemplaza la firma del técnico).

## APIs

Contrato REST por bounded context (detalle de rutas en `08-Backend/`):
`/assets`, `/assets/{id}/history`, `/maintenance-plans`,
`/checklist-templates`, `/work-orders`, `/work-orders/{id}/evidence`,
`/work-orders/{id}/receipt`, `/work-orders/{id}/close`. Todos con
`company_id` implícito vía autenticación (regla `04-Architecture/` #4).

## Validations

- No se puede crear `WorkOrder` sobre un `Asset` retirado (regla #1).
- No se puede `StartWorkOrder` sin `WorkOrderDispatched` previo (regla #3).
- No se puede `CloseWorkOrder` sin evidencia (regla #4) — validación tanto
  en frontend (UX bloqueante) como en el Handler de Aplicación (autoridad
  real, nunca confiar solo en el cliente).

## Errors

Catálogo conceptual (códigos exactos en `08-Backend/`): `AssetNotFound`,
`AssetRetiredCannotCreateWorkOrder`, `WorkOrderInvalidStatusTransition`,
`WorkOrderEvidenceRequiredToClose`, `WorkOrderNotDispatched`,
`EvidencePhotoMissingGeoStamp` (foto no capturada por la cámara interna de
la app — regla #8).

## Acceptance Criteria

1. **Dado** un `MaintenancePlan` activo cuya frecuencia vence hoy,
   **cuando** corre el proceso de vencimiento, **entonces** se crea una
   `WorkOrder` automáticamente y se emite `MaintenancePlanTriggered`.
2. **Dado** una `WorkOrder` sin `WorkOrderDispatched`, **cuando** un técnico
   intenta `StartWorkOrder`, **entonces** el sistema rechaza la acción con
   `WorkOrderNotDispatched`.
3. **Dado** una `WorkOrder` en ejecución sin evidencia, **cuando** se
   intenta `CloseWorkOrder`, **entonces** el sistema rechaza con
   `WorkOrderEvidenceRequiredToClose`, tanto si se intenta desde web como
   desde mobile sin conexión (la validación se re-verifica al sincronizar).
4. **Dado** un técnico sin señal de red, **cuando** completa un checklist y
   agrega evidencia, **entonces** la OT se marca localmente como
   pendiente-de-sync y se cierra en el servidor apenas reconecta, sin
   pérdida de datos.
5. **Dado** una `WorkOrder` cerrada, **cuando** un Supervisor o Admin
   ejecuta `ReopenWorkOrder` sin proveer un motivo, **entonces** el sistema
   rechaza la acción — el motivo es obligatorio, no opcional.

## Future Improvements

- Mantenimiento predictivo real (ver `10-AI/`), hoy fuera del MVP.
- `ChecklistTemplate` versionada (que un cambio en la plantilla no afecte
  checklists ya en curso) — el constructor dinámico (regla #10) lo hace
  más urgente que en el diseño anterior, no menos.

## Open Questions

1. ¿La "prioridad" de una OT es un campo libre o un enum fijo con reglas de
   escalamiento automático asociadas a cada nivel?
2. ¿Existe un límite razonable de reaperturas por OT, o se permite sin
   tope siempre que quede auditado? (relevante para detectar mal uso del
   flujo, no para bloquearlo de entrada).
3. ¿El envío de `WorkOrderReceipt` por email usa el mismo proveedor que se
   defina en `11-Integrations/` para otras notificaciones, o uno propio
   dedicado a documentos legales/remitos?

> Resueltas en esta iteración: reapertura de OT (regla #6), múltiples
> planes de mantenimiento simultáneos por activo (regla #7), y — a partir
> del aporte de un colega sobre la app de campo — estampado de evidencia
> fotográfica (regla #8), remito generado en el dispositivo (regla #9) y
> constructor de formularios dinámicos (regla #10).
