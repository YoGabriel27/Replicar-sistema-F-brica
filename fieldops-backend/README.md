# FieldOps Backend — scaffold generado desde el SYSTEM_BLUEPRINT

Este código se generó siguiendo exactamente lo fijado en
`SYSTEM_BLUEPRINT/08-Backend/README.md` (monolito modular, Clean
Architecture por bounded context, CQRS con MediatR, Outbox sobre
PostgreSQL + relay a Redis Streams) y
`SYSTEM_BLUEPRINT/06-Modules/07-Configuracion/README.md` (módulo
implementado: `Company`, `Partner`, `User`, `Membership`, `Role`,
`Permission`, `FeatureFlag`).

## ⚠️ No verificado por compilación

El entorno donde se generó este scaffold **no tiene el SDK de .NET
instalado** y no tiene acceso de red para descargarlo. El código está
escrito siguiendo las convenciones de .NET 9 / C# 13, pero **no se
compiló ni se corrió** — al abrirlo localmente es esperable necesitar
ajustes menores (versiones exactas de paquetes NuGet, algún using
faltante, etc.).

## Cómo continuar localmente

```bash
dotnet restore
dotnet build
```

Necesitás PostgreSQL corriendo (connection string en
`src/Host/FieldOps.Host/appsettings.json`) y Redis para el Outbox.

## Qué falta (a propósito, fuera del scaffold)

- Migraciones de EF Core (`dotnet ef migrations add InitialCreate`).
- Implementación concreta de `IOutboxWriter` (hoy solo la interfaz y el
  `BackgroundService` de relay están como placeholder).
- El resto de los 6 módulos de negocio (`06-Modules/*`) — Configuración es
  el primero por ser la base de aislamiento multiempresa de todos los demás.
- Validación FluentValidation de cada Command (paquete ya referenciado,
  validadores concretos no escritos).
- Tests (unitarios de Domain, de integración de Application).

## Estructura

Ver `SYSTEM_BLUEPRINT/08-Backend/README.md §Functional Description` para
la explicación completa de por qué la solución está organizada así.
