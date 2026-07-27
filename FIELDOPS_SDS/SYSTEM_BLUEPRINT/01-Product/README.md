# 01-Product

> Visión de producto, personas (perfiles de usuario), problemas que resuelve, propuesta de valor y casos de uso principales, independiente de cómo se implementa técnicamente.

**Estado:** `Draft v0.1`
**Depende de:** `00-Project-Charter/CHARTER.md`
**De este documento dependen:** `02-Business/`, `03-Domain-Model/`, `06-Modules/*`

---

## Purpose

Definir qué es FIELDOPS desde la perspectiva del usuario y del negocio del
cliente (la empresa que lo contrata) — antes de modelar dominio, base de
datos o código. Responde: ¿para quién es esto, qué problema real resuelve,
y cómo se ve un día de uso exitoso?

## Responsibilities

Cubre: personas de usuario, problemas y propuesta de valor, principios de
producto, casos de uso principales de punta a punta. **No cubre**: precios ni
modelo comercial (→ `02-Business/`), entidades de dominio (→ `03-Domain-Model/`),
ni especificación pantalla-por-pantalla de cada módulo (→ `06-Modules/`).

## Scope

Empresas que prestan servicios de mantenimiento, seguridad electrónica o
gestión de activos a terceros (modelo B2B), con operación de campo mediante
técnicos/cuadrillas y activos instalados en sitios de sus clientes. Excluye,
por ahora, uso puramente interno sin componente de campo (ver Open Questions).

## Functional Description

### Personas

| Persona | Rol en el sistema | Necesidad principal |
|---|---|---|
| **Administrador de operaciones** | Configura empresa, ve todo | Visibilidad total y control de configuración |
| **Supervisor / Coordinador** | Planifica y asigna | Programar OT, asignar cuadrillas, ver cumplimiento |
| **Técnico de campo** | Ejecuta OT | App simple, funciona sin señal, registra evidencia rápido |
| **Comercial / CRM** | Gestiona clientes y contratos | Ver estado de contrato y activos del cliente sin pedirlo a Operaciones |
| **Finanzas** | Factura y cobra | Costo real por OT, sin reconciliar manualmente |
| **Gerencia / Ejecutivo** | Consume BI | Tableros confiables sin pedirle reportes a nadie |
| **Cliente final** (opcional, portal) | Consulta su servicio | Ver estado de sus activos y OT abiertas |

### Un día típico (extremo a extremo)

1. Un cliente reporta una falla o vence un mantenimiento programado → se
   genera una OT (manual o automática) en **Operaciones**.
2. **Logística** asigna cuadrilla y repuestos disponibles según ubicación y
   disponibilidad de **RRHH**.
3. El **técnico** recibe la OT en su app móvil, la ejecuta offline si es
   necesario, registra evidencia (fotos, checklist, firma) y la cierra.
4. **Finanzas** genera el costo/factura asociado a la OT cerrada.
5. **CRM** refleja el historial de servicio en la ficha del cliente.
6. **BI** actualiza KPIs de cumplimiento, tiempo de respuesta y costo por
   activo en tiempo real para **Gerencia**.

Ninguna de estas transiciones es manual entre módulos: es el mismo dato
fluyendo por bounded contexts distintos (detalle en `03-Domain-Model/`).

## Business Rules

Principios de producto (no de UI, no de dominio técnico):

1. El activo es el centro del sistema — todo (OT, contrato, factura,
   historial) cuelga de un activo trazable, nunca al revés.
2. Un técnico nunca debe necesitar señal de red para ejecutar una OT ya
   asignada.
3. Ningún módulo pide al usuario un dato que otro módulo ya tiene (p. ej.
   Finanzas no vuelve a pedir la ubicación del activo: la lee de Operaciones).
4. Todo KPI de BI debe poder explicarse trazando hasta la OT o transacción
   que lo originó — no hay métricas "de caja negra".

## Data Model

N/A — vive en `03-Domain-Model/` y `05-Database/`. Este documento habla de
personas y flujos, no de entidades ni tablas.

## UX

Principios (detalle de pantallas en `14-UX/`):
- El supervisor vive en una vista tipo Linear/Notion (tableros, filtros
  rápidos, todo accionable sin recargar).
- El técnico vive en una app mobile minimalista — cada pantalla resuelve
  una sola tarea.
- Gerencia vive en BI (Power BI embebido), no en las pantallas operativas.

## Security

A nivel de producto (detalle técnico en `13-Security/`): cada persona ve
solo lo que su rol permite; el cliente final (si el portal se habilita) solo
ve sus propios activos y OT, nunca los de otros clientes de la misma empresa.

## API

N/A — vive en `04-Architecture/` y `08-Backend/`.

## Events

N/A — vive en `04-Architecture/` y cada módulo en `06-Modules/`.

## Dependencies

Depende de `00-Project-Charter/CHARTER.md` (no puede contradecirlo). De este
documento dependen directamente `02-Business/` (a quién se le vende esto) y
`06-Modules/*` (cada módulo debe poder trazarse a una persona y un caso de
uso definido aquí).

## Future Improvements

- Portal de cliente final como producto separado con su propio ciclo de
  release (mencionado pero no detallado en este draft).
- Casos de uso para operación *interna* sin componente de campo (empresas
  que solo gestionan activos propios, sin clientes externos).

## Open Questions

1. ¿El portal de cliente final es parte del MVP o de una fase posterior?
   (afecta `06-Modules/01-CRM` y `15-Roadmap/`)
2. ¿Se apunta primero al vertical de seguridad electrónica (símil Protecnus)
   o se lanza agnóstico de vertical desde el día 1?
3. ¿Existen ya clientes/diseño de referencia (más allá de Protecnus) cuyos
   flujos debamos validar antes de cerrar los casos de uso de arriba?
