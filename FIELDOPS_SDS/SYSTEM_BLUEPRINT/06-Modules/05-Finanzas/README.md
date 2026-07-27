# Módulo: Finanzas

> Facturación a clientes, costeo por orden de trabajo y cuentas por cobrar.

**Estado:** `Draft v0.1`
**Depende de:** `03-Domain-Model/README.md`, `06-Modules/02-Operaciones` (consume `WorkOrderClosed`), `06-Modules/03-Logistica` (consume `InventoryItemConsumed`), `06-Modules/01-CRM` (vigencia de contrato)
**De este documento dependen:** `06-Modules/06-BI`, `02-Business/` (el plan SaaS se factura con el mismo motor, ver Open Questions)

---

## Purpose

Especificar cómo el costo real de una OT nace automáticamente al cerrarse
(sin que Finanzas lo capture a mano) y cómo ese costo se consolida en una
factura al cliente — el principio de producto de "nadie reconcilia
manualmente" (`01-Product/README.md`).

## Vision

Que Finanzas nunca tenga que preguntarle a Operaciones o Logística cuánto
costó una OT — el dato ya está ahí, generado por el propio cierre.

## Scope

**Dentro:** `CostEntry`, `Invoice`, `Payment`, `AccountReceivable`.
**Fuera:** definición de planes/precios del SaaS FIELDOPS en sí (→
`02-Business/`), procesamiento de pago con proveedor externo (→
`11-Integrations/`).

## Bounded Context

**Billing** — qué se cobra y qué cuesta cada trabajo.

## Entities

| Entidad | Notas |
|---|---|
| `CostEntry` | costo real de una `WorkOrder`, generado al cierre |
| `Invoice` | consolida uno o más `CostEntry` de un cliente en un periodo |
| `Payment` | pago registrado contra una `Invoice` |
| `AccountReceivable` | saldo pendiente por cliente |

## Relationships

- `WorkOrder` (externo) 1—0..1 `CostEntry`.
- `CostEntry` *—1 `Invoice` (consolidación).
- `Invoice` 1—* `Payment` (permite pagos parciales, ver Business Rules).
- `Client` (externo, CRM) 1—* `AccountReceivable`.

## Commands

`GenerateCostEntry` (automático, disparado por `WorkOrderClosed`),
`ConsolidateInvoice`, `IssueInvoice`, `VoidInvoice`, `RegisterPayment`,
`ApproveOutOfContractBilling` (excepción de la regla #4 de CRM).

## Queries

`GetCostEntryByWorkOrder`, `ListPendingInvoices`,
`GetAccountReceivableByClient`, `GetInvoiceById`, `ListOverdueInvoices`.

## Events

**Emitidos:** `CostEntryGenerated`, `InvoiceIssued`, `PaymentRegistered`,
`InvoiceOverdue`.
**Consumidos:** `WorkOrderClosed` (de Operaciones, dispara
`GenerateCostEntry`), `InventoryItemConsumed` (de Logística, se suma al
costo de la OT correspondiente), `ContractTerminated` (de CRM, marca la OT
como pendiente de aprobación para facturar).

## Business Rules

1. `CostEntry` se genera automáticamente al recibir `WorkOrderClosed` — no
   existe una vía manual de captura de costo para una OT.
2. El costo de repuestos consumidos (`InventoryItemConsumed`) se suma al
   `CostEntry` de la OT correspondiente en cuanto Logística lo confirma.
3. `Invoice` consolida uno o más `CostEntry` del mismo cliente dentro de un
   periodo de facturación — no se emite una factura por cada OT
   individualmente salvo que el ciclo de facturación del cliente sea diario.
4. `InvoiceOverdue` se emite automáticamente al vencer la fecha de pago
   pactada, sin revisión manual.
5. Todo monto se almacena junto a su `currency_code` (regla heredada de
   `05-Database/` #5) — nunca se asume una moneda por defecto al calcular.
6. Una OT sobre un `Contract` terminado (regla CRM #4) requiere
   `ApproveOutOfContractBilling` explícito antes de poder incluirse en un
   `ConsolidateInvoice` — nunca se cuela automáticamente.
7. Un `Payment` puede ser parcial — una `Invoice` permanece en estado
   "parcialmente pagada" hasta cubrir el monto total, sin bloquear pagos
   sucesivos.

## Permissions

| Acción | Admin | Finanzas | Supervisor | Comercial | Cliente (portal) |
|---|---|---|---|---|---|
| Ver costo por OT | ✅ | ✅ | ✅ (solo lectura) | ❌ | ❌ |
| Consolidar/emitir factura | ✅ | ✅ | ❌ | ❌ | ❌ |
| Registrar pago | ✅ | ✅ | ❌ | ❌ | ❌ |
| Ver estado de sus facturas | — | — | — | ✅ (de sus clientes) | ✅ (solo las suyas) |
| Aprobar facturación fuera de contrato | ✅ | ✅ | ❌ | ❌ | ❌ |

## Screens

- **Costos por OT**: listado de `CostEntry` generados, filtrable por
  facturado/pendiente.
- **Consolidación de Facturación**: selección de `CostEntry` pendientes por
  cliente/periodo para generar una `Invoice`.
- **Listado de Facturas**: estado (pendiente/pagada parcial/pagada/vencida).
- **Registro de Pago**: monto, medio, fecha, contra qué factura.

## Wireframes

Ver `14-UX/`. La pantalla de Consolidación debe dejar visible, sin
ambigüedad, cuáles `CostEntry` están pendientes de aprobación
(`ApproveOutOfContractBilling`) para que no se olviden fuera del ciclo de
facturación normal.

## Forms

- **Consolidar factura:** cliente, periodo, `CostEntry` a incluir.
- **Registrar pago:** factura, monto, medio de pago, fecha.

## Filters

Por cliente, estado de factura, rango de fecha, moneda.

## Reports

Cartera vencida (aging), costo promedio por tipo de activo (cruzado con
Operaciones), margen por contrato (costo real vs. facturado).

## KPIs

`DSO` (días promedio de cobro), `% facturas vencidas`, `costo promedio por
OT`.

## Notifications

Factura vencida (a Finanzas y a Comercial del cliente), pago registrado (a
Finanzas), `CostEntry` sin facturar tras N días (alerta de fuga de
ingreso).

## Automations

`GenerateCostEntry` automático (regla #1). `InvoiceOverdue` automático
(regla #4).

## AI

Candidato para `10-AI/`: predicción de riesgo de mora por cliente basada en
historial de pagos (no MVP).

## APIs

`/cost-entries`, `/invoices`, `/payments`, `/accounts-receivable`. Detalle
en `08-Backend/`.

## Validations

- No se puede `IssueInvoice` sin al menos un `CostEntry` incluido —
  `InvoiceEmptyCannotIssue`.
- No se puede `RegisterPayment` por un monto mayor al saldo pendiente de la
  factura — se rechaza, no se acepta como crédito a favor en este diseño
  (ver Open Questions).

## Errors

`CostEntryNotFound`, `InvoiceEmptyCannotIssue`, `PaymentExceedsBalance`,
`OutOfContractBillingNotApproved` (al intentar consolidar sin aprobación
previa).

## Acceptance Criteria

1. **Dado** una `WorkOrder` que se cierra, **cuando** el evento
   `WorkOrderClosed` se procesa, **entonces** se genera automáticamente un
   `CostEntry` sin intervención manual.
2. **Dado** un `CostEntry` de una OT sobre contrato terminado sin
   aprobación, **cuando** se intenta `ConsolidateInvoice` incluyéndolo,
   **entonces** el sistema rechaza con `OutOfContractBillingNotApproved`.
3. **Dado** una `Invoice` con saldo pendiente de $100, **cuando** se
   registra un `Payment` de $150, **entonces** el sistema rechaza con
   `PaymentExceedsBalance`.

## Future Improvements

- Manejo de crédito a favor del cliente por sobre-pago (hoy rechazado, ver
  Open Questions).
- Conversión automática de moneda si el contrato y la empresa usan monedas
  distintas.

## Open Questions

1. ¿Se permite pago parcial que exceda el saldo como crédito a favor del
   cliente, o se mantiene el rechazo estricto del MVP?
2. ¿Cómo se maneja impuestos (IVA u otros) por país? Fuera de alcance
   detallar aquí — depende del país de lanzamiento (heredada de
   `02-Business/` Open Question #1).
3. ¿Este mismo motor de `Invoice`/`Payment` se reutiliza para facturar la
   suscripción SaaS de FIELDOPS a la empresa cliente (`02-Business/`), o es
   un sistema de facturación completamente separado?
