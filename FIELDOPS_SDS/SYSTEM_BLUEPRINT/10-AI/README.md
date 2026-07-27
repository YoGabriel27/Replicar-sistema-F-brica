# 10-AI

> Features de IA embebidas en los flujos de cada módulo (no un módulo aparte): qué problema resuelve la IA en cada caso, modelos, datos de entrada/salida.

**Estado:** `Draft v0.1`
**Depende de:** `ADR-0002` (consecuencia: microservicios Python/FastAPI para IA), todos los `06-Modules/*` (cada uno propuso su candidato en su sección "AI")
**De este documento dependen:** `08-Backend/` (orquestación), `12-Infrastructure/` (despliegue del servicio de IA)

---

## Purpose

Consolidar en un solo lugar todas las features de IA que cada módulo dejó
propuestas en su sección "AI", y fijar el patrón técnico común para
construirlas — sin esto, cada módulo tendería a implementar IA de forma
distinta y desconectada (justo lo que `00-Project-Charter/CHARTER.md`
regla #5 prohíbe).

## Responsibilities

Cubre: catálogo consolidado de features de IA por módulo, su estado
(MVP/futuro), y el patrón de integración técnica. **No cubre**:
entrenamiento de modelos específicos ni selección de proveedor de LLM
concreto (ver Open Questions) — eso se decide al construir cada feature.

## Scope

Todas las features de IA mencionadas en `06-Modules/*`. **Ninguna está en
el MVP** — este documento es, en la práctica, el roadmap de IA del sistema,
no una especificación de construcción inmediata.

## Functional Description

### Catálogo consolidado

| Feature | Módulo fuente | Estado |
|---|---|---|
| Sugerencia de causa probable de falla | Operaciones | Futuro |
| Mantenimiento predictivo | Operaciones | Futuro |
| Validación de checklist por foto | Operaciones | Futuro |
| Sugerencia de cuadrilla óptima (no auto-asignación) | Logística | Futuro |
| Sugerencia de necesidades de capacitación | RRHH | Futuro |
| Scoring de probabilidad de cierre de oportunidad | CRM | Futuro |
| Predicción de riesgo de mora por cliente | Finanzas | Futuro |
| Detección de anomalías en KPIs + insights en lenguaje natural | BI | Futuro |

### Patrón de integración (consecuencia de `ADR-0002`)

Todo servicio de IA se implementa como **microservicio interno en
Python/FastAPI**, nunca expuesto directo a internet ni al frontend — se
invoca desde el backend .NET (`08-Backend/`) vía API interna, como
cualquier otro `Module`, pero fuera del monolito modular por estar en otro
runtime. El backend .NET sigue siendo dueño de la validación de negocio;
el servicio de IA solo sugiere, nunca decide ni ejecuta.

**Decisión de despliegue (confirmada):** el servicio de IA se despliega
**junto al backend .NET, en la misma unidad de despliegue** (mismo host o
grupo de contenedores, ver `12-Infrastructure/`) — no como servicio
independiente con su propia infraestructura desde el día 1. El límite
sigue siendo de **código y proceso** (dos runtimes distintos, comunicación
solo vía API interna), no de infraestructura de despliegue. Esto simplifica
operar el MVP; si el volumen de inferencia lo justifica más adelante, se
separa sin cambiar el contrato entre ambos (ver Future Improvements).

## Business Rules

1. Ninguna sugerencia de IA ejecuta una acción de negocio directamente —
   siempre requiere confirmación humana explícita (coherente con
   `06-Modules/03-Logistica` regla de automatización: sugerencia de
   cuadrilla, no asignación automática).
2. Todo servicio de IA es interno, invocado solo desde el backend .NET —
   nunca se expone una API de IA directo al frontend o a la app móvil.
3. Ningún dato enviado a un servicio de IA cruza el límite de tenant
   (`company_id`) sin consentimiento explícito — no se entrena ni se
   infiere mezclando datos de distintas empresas salvo que el negocio
   apruebe explícitamente un modelo compartido (ver Open Questions).

## Data Model

N/A propio — cada feature de IA consume, de solo lectura, entidades o
proyecciones ya definidas en su módulo fuente (`06-Modules/*`) o en
`06-Modules/06-BI/`. No se introducen entidades de negocio nuevas.

## UX

N/A en este documento — cuando se construya cada feature, su presentación
(p. ej. dónde aparece la sugerencia de causa de falla en la pantalla de
`06-Modules/02-Operaciones`) se documenta en `14-UX/` y en el módulo fuente
correspondiente.

## Security

El aislamiento por tenant (regla #3) es el punto crítico de este
documento: si se usa un LLM de terceros, ningún dato de un tenant debe
enviarse a un proveedor externo sin que el contrato de esa integración
garantice que no se usa para entrenar modelos compartidos con otros
clientes (detalle en `13-Security/` cuando se elija proveedor).

## API

Servicios internos FastAPI, uno por feature o agrupados por módulo fuente
— contrato exacto se define al construir cada uno, no en este draft.

## Events

Cada servicio de IA puede consumir eventos de dominio de su módulo fuente
(p. ej. mantenimiento predictivo consumiría el historial de
`WorkOrderClosed` de un `Asset`) — de solo lectura, nunca emite eventos que
muten estado de negocio directamente (regla #1).

## Dependencies

Depende de `ADR-0002` (decisión de stack) y de la sección "AI" de cada
`06-Modules/*`. De este documento depende `08-Backend/` (cómo orquesta la
llamada al servicio de IA) y `12-Infrastructure/` (dónde se despliega ese
servicio Python, separado del backend .NET).

## Future Improvements

Este documento completo **es** la lista de mejoras futuras — cada fila del
catálogo se convierte en su propia especificación detallada cuando el
negocio decida priorizarla. Adicionalmente: separar el servicio de IA a su
propia unidad de despliegue si el volumen de inferencia crece lo
suficiente (hoy comparte despliegue con el backend .NET, ver Functional
Description).

## Open Questions

1. ¿Se usa un LLM/proveedor de terceros (p. ej. vía API) o se entrenan
   modelos propios por feature? Afecta directamente la regla de
   aislamiento de tenant (#3) y el diseño de `13-Security/`.
2. ¿Se permite entrenar un modelo compartido cross-tenant con
   consentimiento explícito de las empresas (mejora la calidad de
   predicción con más datos), o cada tenant se mantiene estrictamente
   aislado incluso para IA?
3. ¿Cuál de las 8 features del catálogo se prioriza primero cuando se
   decida invertir en IA? No hay orden fijado en este draft.
