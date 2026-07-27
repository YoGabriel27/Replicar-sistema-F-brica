# 02-Business

> Modelo de negocio: cómo se vende FIELDOPS, a quién, estructura de precios/planes, tamaño de mercado y competencia (incluido Protecnus y similares).

**Estado:** `Draft v0.1`
**Depende de:** `01-Product/README.md`
**De este documento dependen:** `06-Modules/05-Finanzas`, `06-Modules/07-Configuracion`, `15-Roadmap/`

---

## Purpose

Fijar cómo FIELDOPS genera ingresos y a qué mercado se dirige, para que
Finanzas y Configuración (planes, límites, feature flags) se diseñen sobre
un modelo real y no se improvisen después de construido el producto.

## Responsibilities

Cubre: modelo de ingresos, estructura de planes, mercado objetivo y
posicionamiento competitivo. **No cubre**: tablas de facturación (→
`05-Database/`), pantallas de precios/checkout (→ `14-UX/`), ni la lógica de
facturación por OT (→ `06-Modules/05-Finanzas/`).

## Scope

SaaS B2B multiempresa (multi-tenant), venta directa a empresas de
mantenimiento/seguridad/activos. **Supuesto a validar** (ver Open Questions):
mercado inicial LatAm, moneda base configurable por tenant.

## Functional Description

### Modelo de ingresos

Suscripción recurrente por empresa (tenant), con el plan determinado por
**escala de uso**, no por bloqueo de funcionalidad core:

| Dimensión de plan | Ejemplo de variable |
|---|---|
| Activos gestionados | hasta 500 / 5.000 / ilimitado |
| Usuarios back-office | por asiento |
| Técnicos de campo (app móvil) | por asiento |
| Almacenamiento de evidencia (fotos, adjuntos) | por GB incluido, excedente cobrado |
| BI avanzado / tableros custom | add-on sobre el plan base |

Ninguna funcionalidad de seguridad, auditoría o trazabilidad de OT se
condiciona al plan — eso rompería el principio de producto #1 y #4 de
`01-Product/README.md`.

### Mercado objetivo

**Lanzamiento inicial confirmado: Argentina.** Empresas de servicios de
mantenimiento, seguridad electrónica (símil Protecnus) y gestión de
activos que hoy operan con hojas de cálculo, sistemas legacy o soluciones
parciales (solo CRM, o solo inventario, sin integrarse entre sí).

### Modelo de reventa/partner (confirmado en el MVP)

Se contempla desde el lanzamiento un `Partner` que administra varias
empresas bajo un mismo contrato comercial (ver
`06-Modules/07-Configuracion/README.md`). Queda abierto si la facturación
del plan SaaS es por `Company` individual o consolidada al `Partner` (ver
Open Questions) — el modelo de datos ya lo soporta en ambos casos.

### Competencia

Protecnus (benchmark funcional directo) y plataformas CMMS/field-service
generalistas (Fracttal, UpKeep y similares). Diferenciador propuesto: un
único sistema que integra CRM + Operaciones + Logística + RRHH + Finanzas +
BI sobre el mismo dato, en vez de módulos desconectados.

## Business Rules

1. El plan se factura por tenant (empresa), nunca por usuario final del
   portal de cliente (ese acceso, si existe, es gratuito para el cliente
   del cliente).
2. Todo plan incluye auditoría, soft delete y RBAC — no son add-ons.
3. El excedente de uso (activos, storage) se factura al ciclo siguiente, no
   corta el servicio de forma abrupta a mitad de operación (una OT en
   curso no se bloquea por límite de plan).
4. Los precios se definen en la moneda base del tenant (multimoneda desde
   el modelo de datos, ver `05-Database/`).

## Data Model

N/A en este documento — las entidades de facturación (`Plan`,
`Subscription`, `Invoice`) se modelan en `06-Modules/05-Finanzas/` y
`05-Database/`. Aquí solo se fijan las reglas de negocio que esas entidades
deben cumplir.

## UX

N/A — pantallas de planes, upgrade/downgrade y checkout viven en `14-UX/`
y `06-Modules/07-Configuracion/`.

## Security

Datos de facturación (medios de pago, datos fiscales) requieren el mismo
nivel de aislamiento por tenant que el resto del sistema (ver
`13-Security/`); si se procesan tarjetas, evaluar alcance PCI-DSS en
`Open Questions`.

## API

N/A — vive en `08-Backend/` y `06-Modules/05-Finanzas/`.

## Events

N/A a nivel de negocio — eventos de facturación (`SubscriptionUpgraded`,
`PlanLimitReached`) se definen en `06-Modules/05-Finanzas/` y
`06-Modules/07-Configuracion/`.

## Dependencies

Depende de `01-Product/README.md` (a quién se le vende). Alimenta
directamente las reglas de `06-Modules/05-Finanzas/` (facturación) y
`06-Modules/07-Configuracion/` (límites de plan como feature flags).

## Future Improvements

- Programa de partners/revendedores (una empresa que administra varias
  empresas cliente bajo un mismo contrato).
- Marketplace de integraciones/add-ons de terceros.

## Open Questions

1. ¿Existen precios/tiers ya definidos externamente para el mercado
   argentino, o se diseñan desde cero aquí?
2. **Pendiente de definir:** ¿el procesamiento de pagos es directo
   (requiere alcance PCI) o vía pasarela externa tokenizada (Stripe y
   similares, ver `11-Integrations/`)? Bloquea `06-Modules/05-Finanzas/`
   y `13-Security/`.
3. ¿La suscripción del `Partner` se factura consolidada o por `Company`
   individual? (heredada de `06-Modules/07-Configuracion/README.md` Open
   Question #1).

> Resueltas en esta iteración: país/moneda de lanzamiento (Argentina/ARS,
> ver `05-Database/README.md`) y modelo de reventa/partner (confirmado
> en el MVP, ver `06-Modules/07-Configuracion/README.md`).
