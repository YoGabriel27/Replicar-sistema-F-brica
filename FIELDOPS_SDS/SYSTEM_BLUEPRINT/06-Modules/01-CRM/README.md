# Módulo: CRM

> Clientes, contratos de servicio, oportunidades comerciales y todo el ciclo de relación comercial sobre los activos gestionados.

**Estado:** `Draft v0.1`
**Depende de:** `03-Domain-Model/README.md`, `06-Modules/02-Operaciones` (referencia `Asset`)
**De este documento dependen:** `06-Modules/05-Finanzas` (contrato vigente habilita facturación), `06-Modules/06-BI`

---

## Purpose

Especificar con qué clientes existe relación comercial, bajo qué contrato,
y qué activos cubre ese contrato — la ficha que un Comercial consulta sin
tener que preguntarle a Operaciones.

## Vision

Que el equipo comercial vea el estado completo del cliente (contrato,
activos, historial de servicio) en una sola pantalla, sin depender de otro
equipo para responder una pregunta básica.

## Scope

**Dentro:** `Client`, `Contract`, `Opportunity`, historial comercial.
**Fuera:** el activo en sí y su mantenimiento (→ `06-Modules/02-Operaciones`),
facturación (→ `06-Modules/05-Finanzas`).

## Bounded Context

**CRM** — con quién tenemos relación comercial y bajo qué contrato.

## Entities

| Entidad | Notas |
|---|---|
| `Client` | pertenece a un `Company` (tenant) |
| `Contract` | referencia `Client` y cubre uno o más `Asset` (externo, Operaciones) |
| `Opportunity` | oportunidad comercial en curso, referencia `Client` |

## Relationships

- `Client` 1—* `Contract`, 1—* `Opportunity`.
- `Contract` cubre 1—* `Asset` (externo). **Decisión (resuelve
  `03-Domain-Model/` Open Question #3): un `Contract` puede cubrir activos
  de múltiples sitios/ubicaciones de un mismo cliente** — no es 1:1
  contrato-sitio.
- `Opportunity` ganada (`WinOpportunity`) puede originar un `Contract`
  nuevo (borrador, no automático — ver Business Rules).

## Commands

`CreateClient`, `UpdateClient`, `CreateContract`, `AddAssetToContract`,
`RenewContract`, `TerminateContract`, `CreateOpportunity`,
`UpdateOpportunityStage`, `WinOpportunity`, `LoseOpportunity`.

## Queries

`GetClientById`, `ListContractsByClient`, `GetContractCoverage` (lista de
activos cubiertos), `ListOpportunitiesByStage`, `GetClientServiceHistory`
(proyección de OT cerradas, solo lectura desde Operaciones).

## Events

**Emitidos:** `ClientCreated`, `ContractCreated`, `ContractRenewed`,
`ContractTerminated`, `OpportunityWon`, `OpportunityLost`.
**Consumidos:** `WorkOrderClosed` (de Operaciones, alimenta el historial de
servicio del cliente sin duplicar la entidad `WorkOrder`).

## Business Rules

1. Un `Asset` puede estar cubierto por un solo `Contract` **vigente** a la
   vez — no se permite doble cobertura activa simultánea del mismo activo.
2. `TerminateContract` no borra ni afecta el `Asset` ni su historial de
   mantenimiento — solo termina la relación comercial (Operaciones sigue
   funcionando sobre ese activo si el cliente lo retiene por otra vía).
3. `WinOpportunity` genera un **borrador** de `Contract`, no uno activo —
   requiere confirmación explícita de un Comercial antes de vincular
   activos y activarlo.
4. Una `WorkOrder` sobre un activo con `Contract` terminado **sigue
   generando trazabilidad** en Operaciones, pero facturarla (ver
   `06-Modules/05-Finanzas` regla #6) requiere aprobación explícita —
   nunca se factura automáticamente fuera de contrato.

## Permissions

| Acción | Admin | Comercial | Supervisor (Operaciones) | Cliente (portal) |
|---|---|---|---|---|
| Crear/editar Client, Contract | ✅ | ✅ | ❌ | ❌ |
| Ver activos cubiertos por contrato | ✅ | ✅ | ✅ (solo lectura) | Solo los suyos |
| Gestionar Opportunity | ✅ | ✅ | ❌ | ❌ |
| Terminar contrato | ✅ | ✅ (con motivo) | ❌ | ❌ |

## Screens

- **Ficha de Cliente**: contratos, activos cubiertos, historial de
  servicio, oportunidades abiertas.
- **Pipeline Comercial**: kanban por etapa de `Opportunity`.
- **Detalle de Contrato**: activos cubiertos, vigencia, historial de
  renovación.

## Wireframes

Ver `14-UX/`. La Ficha de Cliente cruza datos de tres módulos (CRM,
Operaciones vía historial, Finanzas vía estado de facturación) — debe
poder cargar cada bloque de forma independiente sin bloquear el resto si
uno tarda.

## Forms

- **Alta de Cliente:** datos básicos, contacto.
- **Alta de Contrato:** cliente, vigencia, selección de activos a cubrir
  (o creación de contrato vacío para agregar activos después).
- **Alta de Oportunidad:** cliente, valor estimado, etapa inicial.

## Filters

Por etapa de oportunidad, estado de contrato (vigente/por vencer/terminado),
cliente.

## Reports

Contratos próximos a vencer, tasa de conversión de oportunidades por
etapa, clientes con activos sin contrato vigente (alerta de riesgo de
ingreso).

## KPIs

`% contratos renovados`, `valor de pipeline abierto`, `tiempo promedio de
ciclo de venta` (creación → `OpportunityWon`).

## Notifications

Contrato próximo a vencer (a Comercial, N días antes), oportunidad
estancada sin cambio de etapa por N días.

## Automations

`ContractRenewed` recordatorio automático antes del vencimiento (regla de
notificación arriba). `WinOpportunity` genera borrador de `Contract`
automáticamente (regla #3), sin activarlo.

## AI

Candidato para `10-AI/`: probabilidad de cierre de una `Opportunity`
basada en histórico similar (no MVP).

## APIs

`/clients`, `/contracts`, `/opportunities`. Detalle en `08-Backend/`.

## Validations

- No se puede `AddAssetToContract` si el activo ya tiene un `Contract`
  vigente activo (regla #1) — error `AssetAlreadyCoveredByActiveContract`.
- No se puede reactivar un `Contract` terminado — se crea uno nuevo.

## Errors

`ClientNotFound`, `AssetAlreadyCoveredByActiveContract`,
`ContractAlreadyTerminated`, `OpportunityInvalidStageTransition`.

## Acceptance Criteria

1. **Dado** un `Asset` ya cubierto por un `Contract` vigente, **cuando** se
   intenta cubrirlo con un segundo contrato activo, **entonces** el sistema
   rechaza con `AssetAlreadyCoveredByActiveContract`.
2. **Dado** un `Contract` terminado, **cuando** Operaciones cierra una
   `WorkOrder` sobre un activo que cubría ese contrato, **entonces** la OT
   se cierra con normalidad (regla #4) pero su facturación queda marcada
   como "requiere aprobación" en Finanzas.
3. **Dado** una `Opportunity` marcada como `WinOpportunity`, **cuando** se
   ejecuta el comando, **entonces** se crea un `Contract` en estado
   borrador, no activo, sin activos vinculados todavía.

## Future Improvements

- Portal de cliente con visibilidad directa de su contrato (hoy solo
  visibilidad de activos y OT, ver `01-Product/` Open Question #1).
- Scoring de oportunidades con IA (ver sección AI).

## Open Questions

1. ¿El portal de cliente final debe mostrar el contenido del `Contract`
   (condiciones, vigencia) o solo el estado de sus activos/OT? (heredada de
   `01-Product/README.md` Open Question #1).
2. ¿Se requiere un flujo de aprobación multi-nivel para `TerminateContract`,
   o basta con motivo obligatorio y rol Comercial/Admin?
