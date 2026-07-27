# 07-Frontend

> Arquitectura frontend: estructura de carpetas, componentes compartidos, manejo de estado (Zustand/React Query), convenciones de UI.

**Estado:** `Draft v0.1`
**Depende de:** `04-Architecture/README.md`, `06-Modules/*` (pantallas y permisos), `14-UX/`
**De este documento dependen:** ninguno directamente — es capa de consumo

---

## Purpose

Fijar cómo se organiza el código del frontend web antes de escribir el
primer componente, para que los 7 módulos de `06-Modules/` se traduzcan en
una estructura consistente y no en convenciones distintas por feature.

## Responsibilities

Cubre: estructura de carpetas, convención de estado (servidor vs. UI),
mapeo módulo→ruta, manejo de autenticación en el cliente. **No cubre**:
tokens de diseño/estilo (→ `14-UX/`), contrato de API (→ `08-Backend/`),
ni la app móvil (→ `09-Mobile/`, stack y convenciones propias).

## Scope

Aplicación web completa (back-office: Admin, Supervisor, Comercial,
Finanzas, RRHH, Gerencia). El portal de cliente final, si se confirma
(`01-Product/` Open Question #1), se documenta como app separada cuando se
apruebe.

## Functional Description

### Stack (fijado en `MASTER.md §3`)

React + Next.js (App Router) + TypeScript + Tailwind + shadcn/ui +
React Query + Zustand.

### Estructura de carpetas

```
src/
├── app/                  ← rutas (App Router), una por módulo/pantalla
│   ├── assets/            (Operaciones — Asset & Maintenance)
│   ├── work-orders/       (Operaciones — Work Order)
│   ├── dispatch/          (Logística)
│   ├── workforce/         (RRHH)
│   ├── clients/           (CRM)
│   ├── billing/           (Finanzas)
│   ├── bi/                (BI — solo lectura, embeds Power BI)
│   └── settings/          (Configuración)
├── features/              ← lógica de negocio de cliente, un folder por módulo
│   └── {modulo}/{api-hooks, components, types}
├── components/            ← componentes compartidos (shadcn/ui + propios)
├── stores/                ← Zustand — solo estado de UI (ver Business Rules)
├── lib/
│   ├── api-client/         ← cliente generado desde el OpenAPI de 08-Backend
│   └── auth/                ← resolución de sesión/tenant activo
└── styles/                 ← tokens Tailwind (ver 14-UX)
```

Cada carpeta de `app/` corresponde 1:1 a un módulo de `06-Modules/` — las
pantallas ya documentadas en la sección "Screens" de cada módulo son la
fuente de verdad de qué vive en cada ruta.

### Manejo de estado

- **Estado de servidor** (datos que vienen del backend: activos, OT,
  clientes, etc.) vive **solo** en React Query — cache, invalidación y
  refetch, nunca copiado a Zustand.
- **Estado de UI** (filtros activos, modal abierto, selección en tabla)
  vive **solo** en Zustand — nunca duplicado en React Query.
- Actualizaciones en tiempo real (p. ej. Panel de Despacho de Logística,
  Dashboards de BI) usan invalidación de React Query disparada por
  WebSocket/SSE o polling corto (decisión técnica pendiente, ver Open
  Questions).

## Business Rules

Convenciones obligatorias (no reglas de negocio de dominio):

1. Ningún componente en `features/` llama `fetch` directo — todo pasa por
   hooks de React Query definidos en `lib/api-client/`.
2. El tenant activo (`company_id` del usuario autenticado) se resuelve una
   sola vez en el layout raíz de `app/` — ninguna pantalla individual
   vuelve a chequearlo.
3. Todo formulario comparte el mismo esquema de validación entre creación y
   edición (un único schema, no dos validaciones divergentes).
4. Ocultar un botón/acción por rol es solo UX — la autorización real
   siempre se valida en el backend (`08-Backend/`, nunca confiar en el
   cliente).

## Data Model

N/A — el frontend consume tipos TypeScript generados desde el contrato
OpenAPI de `08-Backend/`, no define su propio modelo de datos.

## UX

Tokens de diseño, tipografía y patrones de pantalla en `14-UX/`. Este
documento solo fija que todo componente usa clases utilitarias de Tailwind
+ primitivos de shadcn/ui — sin CSS ad hoc fuera de ese sistema.

## Security

El token de sesión se maneja server-side (cookie httpOnly), no en
`localStorage` — evita exposición a XSS. Detalle completo de
autenticación/RBAC en `13-Security/`.

## API

Cliente TypeScript autogenerado desde el OpenAPI expuesto por
`08-Backend/` — nunca se escriben tipos de request/response a mano en
`features/`.

## Events

Actualizaciones en tiempo real vía WebSocket/SSE (a confirmar, ver Open
Questions) para: Panel de Despacho (Logística) y Dashboards (BI). El resto
de módulos usa refetch estándar de React Query.

## Dependencies

Depende de `04-Architecture/` (contrato general), `06-Modules/*`
(pantallas y permisos por rol) y `14-UX/` (sistema de diseño). No tiene
dependientes directos — es la capa más externa del sistema.

## Future Improvements

- Soporte offline en web (hoy el offline-first es exclusivo de
  `09-Mobile/`).
- Arquitectura de micro-frontends si el equipo crece lo suficiente como
  para justificarlo (no se adopta en el MVP).

## Open Questions

1. ¿El tiempo real (Panel de Despacho, Dashboards de BI) se implementa con
   WebSocket/SSE o con polling corto de React Query? Bloquea el diseño de
   `08-Backend/` en esa parte.
2. ¿Se requiere white-labeling (tema visual por empresa) para el modelo de
   reventa/partner, si ese modelo se confirma (`02-Business/` Open
   Question #4)?
