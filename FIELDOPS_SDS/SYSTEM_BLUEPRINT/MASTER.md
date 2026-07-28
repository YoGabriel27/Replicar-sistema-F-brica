# FIELDOPS — Software Design System (SDS)
> Documento maestro. Todo el blueprint cuelga de aquí. Ningún módulo ni línea de
> código se escribe sin que su documento correspondiente exista y esté aprobado.

**Estado:** `Draft v0.1`
**Última actualización:** 2026-07-27
**Dueño del documento:** Product / Arquitectura

---

## 0. Qué es esto

FIELDOPS es un sistema de gestión de operaciones de campo (field service /
CMMS) — inspirado funcionalmente en Protecnus — para empresas que administran
**activos, mantenimiento, órdenes de trabajo (OT), cuadrillas, clientes y
logística**, con capas adicionales de CRM, RRHH, Finanzas y BI sobre el mismo
núcleo operativo.

Este archivo es el **índice y las reglas del juego**. No contiene el detalle
funcional de cada módulo — eso vive en `06-Modules/`. Aquí se fija:

1. La visión y el alcance del producto.
2. La arquitectura y el stack técnico (innegociable salvo ADR que lo cambie).
3. El estándar que **todo** documento del blueprint debe seguir.
4. El orden de construcción — documentación antes que código, siempre.

---

## 1. Visión de producto (resumen — detalle en `01-Product/`)

Plataforma multiempresa para compañías de servicios de mantenimiento,
seguridad electrónica, activos industriales o infraestructura, que necesitan:

- Registrar y trazar activos a lo largo de su ciclo de vida.
- Programar y ejecutar mantenimiento preventivo y correctivo (OT).
- Coordinar cuadrillas, técnicos e inventario en campo (offline-first).
- Gestionar la relación comercial con el cliente (CRM) sobre esos mismos activos.
- Facturar, controlar costos y ver todo desde BI ejecutivo.

**Escala objetivo de diseño:** 100.000 empresas · 5.000.000 de activos ·
50.000.000 de órdenes de trabajo. Multiempresa, multimoneda, multipaís,
con soporte offline en campo e IA embebida en los flujos (no como feature
aparte).

## 2. Módulos del sistema

| # | Módulo | Carpeta | Resumen |
|---|--------|---------|---------|
| 1 | CRM | `06-Modules/01-CRM` | Clientes, contratos, oportunidades, ciclo comercial |
| 2 | Operaciones | `06-Modules/02-Operaciones` | Activos, mantenimiento preventivo/correctivo, órdenes de trabajo |
| 3 | Logística | `06-Modules/03-Logistica` | Inventario, repuestos, rutas, despacho de cuadrillas |
| 4 | RRHH | `06-Modules/04-RRHH` | Técnicos, turnos, competencias, asistencia |
| 5 | Finanzas | `06-Modules/05-Finanzas` | Facturación, costos por OT, cuentas por cobrar |
| 6 | BI | `06-Modules/06-BI` | Tableros ejecutivos y operativos (Power BI embebido) |
| 7 | Configuración | `06-Modules/07-Configuracion` | Multiempresa, roles, parametrización, feature flags |

Cada módulo tiene su propia carpeta con la documentación completa siguiendo
la plantilla de la sección 5.

## 3. Arquitectura y stack (resumen — detalle en `04-Architecture/`)

**Frontend:** React + Next.js + TypeScript + Tailwind + shadcn/ui + React
Query + Zustand
**API:** .NET 9 (preferido) o FastAPI — decisión formal pendiente, ver ADR en
`17-Decisions/`
**Base de datos:** PostgreSQL
**Cache / colas:** Redis
**Almacenamiento de archivos:** Storage (S3-compatible)
**BI:** Power BI embebido
**Mobile:** app offline-first para técnicos en campo (`09-Mobile/`)

### Reglas de arquitectura (obligatorias en todo el backend)

SOLID · DDD (Domain-Driven Design) · Clean Architecture / Hexagonal ·
CQRS · REST + OpenAPI · JWT + OAuth2 · RBAC · Auditoría en todas las
entidades de negocio · Soft Delete (nunca borrado físico de datos de negocio)
· Feature Flags · Logging estructurado · Caching · patrón Outbox para eventos
· Repository · Specification.

Ningún módulo puede documentarse o construirse violando estas reglas. Si un
caso lo exige, se documenta la excepción como ADR en `17-Decisions/`.

## 4. Base de datos (resumen — detalle en `05-Database/`)

Diseñada desde el día 1 para la escala objetivo (100k empresas / 5M activos /
50M OT), multiempresa (tenant_id en toda tabla de negocio), multimoneda,
multipaís, con estrategia definida para sincronización offline. Cada tabla se
documenta con: campos, tipos, índices, constraints y enums — sin excepción.

## 5. Estándar de documentación (obligatorio para cada .md del blueprint)

Todo documento de `01-Product/` en adelante — especialmente los de
`06-Modules/` — debe tener estas secciones, en este orden. Si una sección no
aplica, se deja explícitamente como `N/A` con una línea de justificación (no
se omite):

1. **Purpose** — por qué existe este documento/módulo.
2. **Responsibilities** — qué cubre y qué NO cubre.
3. **Scope** — límites funcionales y técnicos.
4. **Functional Description** — cómo funciona, en prosa.
5. **Business Rules** — reglas de negocio explícitas, numeradas.
6. **Data Model** — entidades, relaciones, campos clave.
7. **UX** — pantallas, flujos, wireframes o su referencia.
8. **Security** — permisos, roles, datos sensibles.
9. **API** — endpoints relevantes (contrato, no implementación).
10. **Events** — eventos que emite/consume (para Outbox/CQRS).
11. **Dependencies** — de qué otros módulos depende / quién depende de este.
12. **Future Improvements** — lo que se sabe que falta y se pospuso a propósito.
13. **Open Questions** — decisiones aún no tomadas.

### Plantilla extendida para especificación de módulo (`06-Modules/*`)

Además del estándar anterior, cada módulo detalla:

Purpose, Vision, Scope, Bounded Context, Entities, Relationships, Commands,
Queries, Events, Business Rules, Permissions, Screens, Wireframes, Forms,
Filters, Reports, KPIs, Notifications, Automations, AI, APIs, Validations,
Errors, Acceptance Criteria.

## 6. UI/UX — referencias de inspiración

Linear, Notion, Stripe, Atlassian, Microsoft (Fluent) y Power BI. Cada
pantalla del sistema se documenta con: widgets, filtros, acciones, tablas,
gráficos, permisos, estados y validaciones — ver `14-UX/`.

## 7. Estructura de carpetas del blueprint

```
SYSTEM_BLUEPRINT/
├── MASTER.md                 ← este archivo
├── 00-Project-Charter/       ← objetivo, stakeholders, alcance, éxito medible
├── 01-Product/                ← visión de producto, personas, casos de uso
├── 02-Business/                ← modelo de negocio, pricing, mercado
├── 03-Domain-Model/           ← lenguaje ubicuo, bounded contexts, entidades core
├── 04-Architecture/           ← arquitectura técnica, decisiones, diagramas
├── 05-Database/               ← modelo de datos completo, migraciones
├── 06-Modules/                ← especificación funcional de cada módulo
│   ├── 01-CRM/
│   ├── 02-Operaciones/
│   ├── 03-Logistica/
│   ├── 04-RRHH/
│   ├── 05-Finanzas/
│   ├── 06-BI/
│   └── 07-Configuracion/
├── 07-Frontend/                ← arquitectura frontend, componentes, estado
├── 08-Backend/                 ← arquitectura backend, servicios, capas
├── 09-Mobile/                  ← app de campo offline-first
├── 10-AI/                      ← features de IA embebidas por módulo
├── 11-Integrations/            ← integraciones externas (pasarelas, IoT, etc.)
├── 12-Infrastructure/          ← despliegue, entornos, CI/CD, escalamiento
├── 13-Security/                ← seguridad transversal, cumplimiento, auditoría
├── 14-UX/                       ← sistema de diseño, patrones de pantalla
├── 15-Roadmap/                 ← fases de entrega
├── 16-Glossary/                ← lenguaje ubicuo compartido entre todos los docs
├── 17-Decisions/                ← ADRs (Architecture Decision Records)
├── 18-Standards/                ← convenciones de código, naming, commits
└── 19-Releases/                 ← changelog y versionado del propio blueprint
```

## 8. Orden de construcción (no negociable)

```
MASTER.md  →  01-Product  →  02-Business  →  03-Domain-Model  →
04-Architecture  →  05-Database  →  06-Modules  →  07-Frontend  →
08-Backend  →  09-Mobile  →  10-AI  →  13-Security  →  [ recién ahí: código ]
```

No se escribe código de un módulo hasta que exista y esté aprobado su
documento en `06-Modules/`. Un módulo sin documento aprobado no se construye,
sin excepción.

## 9. Estado de avance del blueprint

| Documento | Estado |
|---|---|
| MASTER.md | ✅ Draft v0.1 |
| 00-Project-Charter | ✅ Draft v0.1 |
| 01-Product | ✅ Draft v0.1 |
| 02-Business | ✅ Draft v0.1 |
| 03-Domain-Model | ✅ Draft v0.1 |
| 04-Architecture | ✅ Draft v0.1 |
| 05-Database | ✅ Draft v0.1 |
| 06-Modules/* (7) | ✅ 7/7 en Draft v0.1 (Operaciones actualizado — ver nota abajo) |
| 07-Frontend | ✅ Draft v0.1 |
| 08-Backend | ✅ Draft v0.1 |
| 09-Mobile | ✅ Draft v0.1 |
| 10-AI | ✅ Draft v0.1 |
| 13-Security | ✅ Draft v0.1 |
| Código | 🔓 Desbloqueado — ver §11 antes de empezar |

## 10. Estado del blueprint: completo

Los 19 documentos del `SYSTEM_BLUEPRINT` están en `Draft v0.1`. Dos ADRs
tomados (`ADR-0001`: BI como read-model; `ADR-0002`: backend en .NET 9) y
dos decisiones adicionales resueltas dentro de `08-Backend/`: monolito
modular (no microservicios desde el día 1) y Outbox sobre PostgreSQL con
relay a Redis Streams.

Esto **no** significa que el blueprint esté cerrado — "Draft v0.1" es a
propósito: cada documento tiene preguntas abiertas reales, no retóricas,
que conviene resolver antes de construir el módulo o capa correspondiente.

## 11. Preguntas abiertas: 7 de 9 resueltas

De las 9 preguntas de mayor impacto identificadas, se resolvieron 7 en esta
iteración:

- **Reventa/partner:** confirmado desde el MVP — `06-Modules/07-Configuracion/`
  rediseñado con `Partner`/`Membership` (ya no es 1:1 `User`↔`Company`).
- **País/moneda de lanzamiento:** Argentina (ARS) — `05-Database/`,
  `02-Business/` y `00-Project-Charter/` actualizados.
- **Particionamiento de `work_order`:** mensual.
- **Read-model de BI:** mismo clúster PostgreSQL, schema `bi` separado.
- **Servicio de IA:** desplegado junto al backend .NET (misma unidad de
  despliegue, límite solo de código/proceso).
- **Framework mobile:** React Native, confirmado.
- **Versionado de API:** por URL (`/v1/...`).
- **2FA:** requisito de lanzamiento (no post-MVP) — `13-Security/`
  actualizado, impacta `07-Frontend/` y `09-Mobile/` desde el día 1.

**Quedan explícitamente pendientes, sin bloquear:**

1. **Procesamiento de pagos** — directo (PCI) vs. pasarela externa
   tokenizada. Bloquea el detalle final de `06-Modules/05-Finanzas/` y
   `13-Security/`, pero no impide construir el resto.
2. **Certificaciones de cumplimiento en Argentina** (protección de datos,
   facturación electrónica AFIP) — bloquea el cierre final de
   `13-Security/`, no el resto del sistema.

## 12. Próximo paso

Con el blueprint documental completo y las decisiones de mayor impacto
resueltas, las opciones naturales son:
(a) resolver las 2 preguntas pendientes (pagos y cumplimiento AR) antes de
tocar Finanzas o Security en código,
(b) empezar a construir el módulo `Configuración` (ahora con
`Partner`/`Membership`, base de todo lo demás) siguiendo
`08-Backend/README.md`, o
(c) generar el scaffold inicial del repositorio (estructura de carpetas de
`07-Frontend/` y `08-Backend/`, sin lógica de negocio aún) como punto de
partida concreto para el equipo.

## 13. Reconciliación con especificación de un colega (app de campo)

Un colega del equipo redactó independientemente una especificación de la
app de campo/panel web que coincidía en gran parte con
`06-Modules/02-Operaciones/` y `09-Mobile/`, pero aportó 7 detalles
concretos que no estaban documentados. Ya incorporados:

1. Constructor de formularios dinámicos sin código →
   `ChecklistTemplate` (`06-Modules/02-Operaciones/` regla #10).
2. Estampado de GPS/fecha/hora sobre la foto (no solo como metadato) →
   regla #8 del mismo documento, regla #6 de `09-Mobile/`.
3. Alerta de poco almacenamiento en el dispositivo → `09-Mobile/` regla #7.
4. Botón de sincronización manual → `09-Mobile/` regla #5.
5. Remito PDF generado y firmado en el dispositivo al cierre →
   `WorkOrderReceipt`, regla #9 de Operaciones.
6. Repositorio central de remitos + reenvío por email → nueva pantalla en
   Operaciones, dependencia agregada hacia `11-Integrations/`.
7. Visor de fotos en alta calidad + mapa de ubicación → nuevas pantallas
   en Operaciones.

**Hallazgo adicional no anticipado:** el colega asumía que un técnico
puede crear tareas ("partes de trabajo espontáneos") sin pasar por un
Supervisor — esto contradecía la tabla de permisos original, que
restringía la creación de OT a Admin/Supervisor. Se resolvió a favor del
colega: **regla #11** de `06-Modules/02-Operaciones/` — el técnico puede
crear una OT "espontánea" que se auto-asigna a su propio `Crew`, saltando
el flujo normal de `Dispatch`.

Este tipo de reconciliación es exactamente para lo que sirve el
`Draft v0.1` de cada documento — no se trata como error del blueprint
original, sino como el proceso normal de refinar con más información.
