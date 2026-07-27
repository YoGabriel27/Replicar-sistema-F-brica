# Módulo: RRHH

> Técnicos y cuadrillas: turnos, competencias/certificaciones, asistencia y disponibilidad.

**Estado:** `Draft v0.1`
**Depende de:** `03-Domain-Model/README.md`
**De este documento dependen:** `06-Modules/03-Logistica` (disponibilidad para Dispatch), `06-Modules/02-Operaciones` (competencias requeridas por tipo de activo)

---

## Purpose

Especificar quién puede trabajar, cuándo y con qué competencia — la fuente
de verdad que Logística consulta antes de asignar una OT a una cuadrilla.
Sin este módulo, `AssignWorkOrderToCrew` (Logística, regla #1) no tiene
sobre qué validar disponibilidad.

## Vision

Que la disponibilidad de un técnico sea siempre confiable en tiempo real —
sin este módulo actualizado, Logística asignaría OT a gente que en
realidad está de licencia o sin la certificación necesaria.

## Scope

**Dentro:** perfil del técnico, turnos, competencias/certificaciones,
registro de asistencia y disponibilidad resultante.
**Fuera:** nómina y liquidación de sueldos (explícitamente fuera del MVP,
ver Open Questions), asignación de OT en sí (→ `06-Modules/03-Logistica`).

## Bounded Context

**Workforce** — quién puede trabajar, cuándo y con qué competencia. No
decide qué se le asigna (eso es Dispatch & Inventory).

## Entities

| Entidad | Notas |
|---|---|
| `Technician` | perfil del trabajador de campo |
| `Shift` | turno planificado (fecha, horario) |
| `Skill` | competencia o certificación (con posible fecha de vencimiento) |
| `Attendance` | registro real de presencia/ausencia frente al `Shift` planificado |

## Relationships

- `Technician` 1—* `Shift` (turnos planificados a lo largo del tiempo).
- `Technician` *—* `Skill` (un técnico tiene varias competencias; una
  competencia la tienen varios técnicos).
- `Shift` 1—0..1 `Attendance` (el registro real de si se cumplió el turno).
- `Technician` es referenciado externamente por `Crew` (Logística) — RRHH
  no modela `Crew`, solo expone qué técnicos existen y su disponibilidad.

## Commands

`CreateTechnician`, `UpdateTechnician`, `AssignSkill`, `RevokeSkill` (p. ej.
certificación vencida), `ScheduleShift`, `CancelShift`, `RegisterAttendance`,
`RequestLeave` (licencia/ausencia planificada).

## Queries

`GetTechnicianById`, `ListTechniciansBySkill`, `GetTechnicianAvailability`
(para un rango de fecha/hora, consumida por Logística), `ListShiftsByDate`,
`GetAttendanceHistory`.

## Events

**Emitidos:** `TechnicianAvailabilityChanged` (consumido por Logística en
tiempo real, ver `06-Modules/03-Logistica`), `SkillExpired`,
`AttendanceRegistered`.
**Consumidos:** ninguno core — RRHH es mayormente fuente, no consumidor, en
este primer diseño (ver Open Questions si esto cambia).

## Business Rules

1. Un `Technician` con una `Skill` vencida no debe aparecer como disponible
   para OT que requieran esa competencia — `SkillExpired` dispara
   recalculo de `TechnicianAvailabilityChanged` automáticamente.
2. `RequestLeave` aprobado cambia la disponibilidad del técnico para ese
   rango de fecha sin necesidad de cancelar manualmente cada `Shift`
   afectado — el sistema lo resuelve.
3. `RegisterAttendance` es el registro de la realidad, no una simple
   confirmación del `Shift` planificado — pueden diferir (llegó tarde, no
   se presentó), y esa diferencia es dato relevante para RRHH, no se
   descarta.
4. Toda `TechnicianAvailabilityChanged` debe emitirse en el momento del
   cambio (aprobación de licencia, vencimiento de skill, registro de
   ausencia) — Logística depende de esto en tiempo real, no en batch.

## Permissions

| Acción | Admin | RRHH | Supervisor/Coordinador (Logística) | Técnico |
|---|---|---|---|---|
| Crear/editar Technician | ✅ | ✅ | ❌ | ❌ (solo su propio perfil básico) |
| Asignar/revocar Skill | ✅ | ✅ | ❌ | ❌ |
| Programar Shift | ✅ | ✅ | Consultar | Consultar el suyo |
| Registrar Attendance | ✅ | ✅ | ❌ | ✅ (su propia asistencia) |
| Aprobar licencia | ✅ | ✅ | ❌ | Solicitar (no aprobar) |

## Screens

- **Ficha de Técnico**: skills, turnos, historial de asistencia.
- **Calendario de Turnos**: vista semanal/mensual por técnico o equipo.
- **Gestión de Competencias**: skills con fecha de vencimiento, alertas de
  próximas a vencer.
- **Mobile — Mi Turno**: técnico ve su turno, marca asistencia, solicita
  licencia.

## Wireframes

Ver `14-UX/`. El Calendario de Turnos debe poder cruzarse visualmente con
el Panel de Despacho de Logística (mismo dato de disponibilidad, dos
vistas).

## Forms

- **Alta de Técnico:** datos básicos, skills iniciales.
- **Asignación de Skill:** tipo de competencia, fecha de obtención, fecha
  de vencimiento (si aplica).
- **Solicitud de licencia:** rango de fechas, motivo.

## Filters

Por skill, disponibilidad (disponible/de licencia/turno activo), rango de
fecha de turno.

## Reports

Skills próximas a vencer por técnico, ausentismo por periodo, cobertura de
turnos vs. demanda de OT (cruzado con Logística/Operaciones para BI).

## KPIs

`% técnicos con skills vigentes`, `% cumplimiento de turno` (asistencia vs
planificado), `# licencias activas`.

## Notifications

Skill próxima a vencer (a Técnico y a RRHH, con antelación configurable),
licencia aprobada/rechazada (a Técnico), turno próximo a iniciar (a
Técnico).

## Automations

`SkillExpired` automático a la fecha de vencimiento — sin intervención
manual. Recalculo automático de `TechnicianAvailabilityChanged` ante
cualquier cambio relevante (regla #4).

## AI

Candidato para `10-AI/`: sugerencia de necesidades de capacitación basada
en skills próximas a vencer cruzadas con demanda proyectada de OT por tipo
de activo (no MVP).

## APIs

`/technicians`, `/technicians/{id}/availability`, `/shifts`, `/skills`,
`/attendance`, `/leave-requests`. Detalle en `08-Backend/`.

## Validations

- No se puede `AssignSkill` con fecha de vencimiento en el pasado.
- No se puede `RegisterAttendance` para un `Shift` que no existe o no
  pertenece al técnico autenticado.

## Errors

`TechnicianNotFound`, `SkillAlreadyExpired` (al intentar asignar una
vencida sin querer), `ShiftNotFound`, `LeaveRequestOverlapsExistingShift`
(alerta, no bloqueo automático — requiere decisión humana).

## Acceptance Criteria

1. **Dado** una `Skill` que vence hoy, **cuando** se cumple la fecha de
   vencimiento, **entonces** el sistema emite `SkillExpired` y actualiza
   `TechnicianAvailabilityChanged` sin intervención manual.
2. **Dado** una licencia aprobada para un rango de fechas, **cuando**
   Logística consulta disponibilidad de ese técnico en ese rango,
   **entonces** aparece como no disponible sin que nadie haya cancelado
   manualmente sus turnos individuales.
3. **Dado** un técnico que marca asistencia distinta a su turno planificado
   (p. ej. llegó tarde), **cuando** se registra, **entonces** queda
   almacenada la diferencia (planificado vs. real), no solo un booleano de
   cumplimiento.

## Future Improvements

- Nómina y liquidación de sueldos como módulo separado (hoy explícitamente
  fuera de alcance).
- Sugerencias de capacitación basadas en IA (ver sección AI).

## Open Questions

1. ¿Nómina/liquidación se contempla como módulo futuro del mismo sistema,
   o queda permanentemente fuera de alcance (integración con un sistema
   externo de nómina)? Afecta si `Technician` necesita más campos a futuro.
2. ¿`RequestLeave` requiere aprobación de un único rol (RRHH) o puede
   variar por tipo de licencia (p. ej. licencia médica vs. vacaciones con
   distintos aprobadores)?
