# Project Charter — FIELDOPS

**Estado:** `Draft v0.1`
**Depende de:** `MASTER.md`
**De este documento dependen:** `01-Product/`, `02-Business/`, `15-Roadmap/`

---

## Purpose

Fijar el marco de decisión del proyecto FIELDOPS antes de definir producto o
arquitectura: por qué se construye, quién lo patrocina, qué se considera
éxito y qué restricciones no son negociables. Sirve como referencia para
resolver disputas de alcance más adelante — si una decisión contradice este
documento, este documento gana salvo que se actualice explícitamente.

## Responsibilities

Este documento cubre: objetivo de negocio del proyecto, alcance macro,
stakeholders, criterios de éxito medibles, restricciones y riesgos.
**No cubre**: visión de producto detallada (→ `01-Product/`), modelo de
negocio/precios (→ `02-Business/`), ni especificación funcional de módulos
(→ `06-Modules/`).

## Scope

**Dentro de alcance (visión completa del proyecto):**
- Plataforma web multiempresa (CRM, Operaciones, Logística, RRHH, Finanzas,
  BI, Configuración).
- App móvil offline-first para técnicos de campo.
- Escalable a 100.000 empresas, 5.000.000 de activos, 50.000.000 de OT.
- Multimoneda y multipaís desde el diseño de datos, aunque el lanzamiento
  inicial se limite a un país/moneda.

**Fuera de alcance (por ahora, sujeto a revisión en `15-Roadmap/`):**
- Integraciones de hardware/IoT específicas de fabricantes de sensores o
  paneles de alarma — se documentan como extensión futura en `11-Integrations/`.
- Verticales fuera de mantenimiento/seguridad/activos (p. ej. salud, retail)
  no están contempladas en el MVP.
- Certificaciones de cumplimiento específicas por país (ver `Open Questions`).

**Benchmark funcional:** Protecnus y plataformas CMMS/field-service
comparables (el detalle de qué se replica y qué se mejora vive en
`01-Product/` y `02-Business/`, no aquí).

## Functional Description

N/A — este documento no describe comportamiento del sistema. El "día a día"
de uso del producto se documenta en `01-Product/README.md` (visión) y en
cada carpeta de `06-Modules/` (detalle operativo).

## Business Rules

Principios y restricciones que gobiernan el proyecto completo, no negociables
sin pasar por un ADR en `17-Decisions/`:

1. Ningún módulo se construye sin su documento aprobado en `06-Modules/`
   (regla ya fijada en `MASTER.md §8`).
2. Toda entidad de negocio es multiempresa desde el modelo de datos (no se
   añade `tenant_id` después).
3. Ningún dato de negocio se borra físicamente (soft delete + auditoría,
   ver `13-Security/`).
4. El sistema se diseña offline-first para el rol técnico de campo desde el
   primer módulo de Operaciones — no se agrega como parche posterior.
5. La IA se documenta y construye embebida en el flujo de cada módulo que la
   necesite (`10-AI/`), nunca como módulo aislado de "IA" desconectado del
   resto.
6. Todo cambio de alcance que afecte más de un módulo requiere actualizar
   este Charter y el `MASTER.md` antes de tocar código.

## Data Model

N/A — vive en `05-Database/`. Este documento no fija tablas ni entidades.

## UX

N/A a nivel de pantallas — vive en `14-UX/`. Principio de proyecto: la
experiencia debe sostener la escala objetivo (50M de OT) sin degradarse;
cualquier decisión de UX que no escale se rechaza en revisión de diseño.

## Security

Expectativas de seguridad a nivel de proyecto (detalle técnico en
`13-Security/`):
- Aislamiento estricto de datos entre empresas (tenant isolation) es
  requisito de lanzamiento, no mejora futura.
- RBAC desde el primer módulo construido.
- Auditoría de cambios en activos, OT y datos financieros desde el día 1.

## API

N/A — vive en `04-Architecture/` y `08-Backend/`.

## Events

N/A — vive en `04-Architecture/` (patrón Outbox) y en cada módulo.

## Dependencies

**Stakeholders / patrocinadores:** definir nombres y roles reales antes de
cerrar este documento (actualmente placeholder — ver Open Questions).
**Depende de:** benchmark competitivo de Protecnus y similares (input externo
al blueprint, no versionado aquí).
**De este Charter dependen directamente:** `01-Product/`, `02-Business/`,
`15-Roadmap/` — no se redactan en contradicción con lo fijado aquí.

## Future Improvements

- Definir presupuesto y línea de tiempo formal una vez cerrado `01-Product/`
  y `02-Business/` (el Charter no fija fechas todavía, a propósito: fijar
  fecha antes de conocer alcance real es el error más común en proyectos de
  este tamaño).
- Evaluar expansión a verticales fuera de mantenimiento/seguridad una vez
  el core esté estable en producción.

## Open Questions

1. ¿Quiénes son los stakeholders/patrocinadores formales del proyecto?
2. ¿.NET 9 o FastAPI para el backend? — **Resuelto:** .NET 9 (`ADR-0002`).
3. ¿Existe un presupuesto/plazo ya fijado externamente, o el roadmap se
   construye de cero en `15-Roadmap/`?
4. ¿Qué certificaciones de cumplimiento aplican en Argentina (protección
   de datos, facturación electrónica AFIP, etc.)? Pendiente — bloquea
   parte de `13-Security/`.

> Resuelto en esta iteración: país/moneda de lanzamiento — **Argentina
> (ARS)**, ver `05-Database/README.md` y `02-Business/README.md`.
