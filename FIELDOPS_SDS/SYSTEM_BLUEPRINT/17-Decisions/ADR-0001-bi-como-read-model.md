# ADR-0001 — BI como read-model CQRS, no bounded context propio

**Estado:** ✅ Aceptado (decisión tomada por el equipo de arquitectura ante
falta de restricción externa — ver `03-Domain-Model/README.md`, Open
Question #1)
**Fecha:** 2026-07-27

## Contexto

`03-Domain-Model/README.md` dejó abierto si BI es un bounded context con
entidades agregadas propias, o un modelo de lectura puro sobre los demás
contextos (CRM, Operaciones, Logística, RRHH, Finanzas). La arquitectura ya
exige CQRS de forma transversal (`MASTER.md §3`), lo que hace natural
resolver esto usando el mismo patrón en vez de inventar uno nuevo solo para
BI.

## Decisión

**BI es un read-model puro (lado Query de CQRS), no un bounded context de
negocio.** No tiene entidades propias con reglas de negocio ni comandos que
las muten — solo proyecciones/vistas materializadas construidas a partir de
los eventos de dominio de los demás contextos (`WorkOrderClosed`,
`InvoiceIssued`, `AssetCreated`, etc., definidos en `03-Domain-Model/`).

Consecuencia directa: `06-Modules/06-BI/` no define entidades nuevas — solo
tableros/KPIs derivados, y su especificación debe trazar cada KPI a los
eventos/entidades de otros módulos que lo alimentan (coherente con la regla
de producto #4 en `01-Product/README.md`: "ningún KPI de caja negra").

## Alternativas consideradas

1. **Bounded context propio con entidades agregadas** (p. ej. `Metric`,
   `Dashboard` como entidades de negocio con su propio ciclo de vida) —
   descartado: duplicaría lógica ya resuelta en los contextos fuente y
   rompería la trazabilidad exigida por producto.
2. **Consultar directamente las tablas transaccionales desde Power BI** —
   descartado: no escala a 50M de OT sin degradar el sistema operativo; se
   requieren proyecciones/vistas materializadas separadas.

## Consecuencias

- `04-Architecture/README.md` debe especificar el mecanismo de proyección
  (event handlers que actualizan read-models, ver patrón Outbox).
- `05-Database/README.md` debe modelar el almacén de lectura de BI como
  esquema/base separada de la transaccional (mismo Postgres o réplica,
  decisión técnica pendiente de detallar ahí).
- `06-Modules/06-BI/README.md`, cuando se redacte, no debe incluir sección
  "Entities" propias — debe listar los eventos/entidades fuente de cada KPI.
