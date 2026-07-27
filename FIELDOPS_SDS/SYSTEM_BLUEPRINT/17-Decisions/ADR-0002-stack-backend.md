# ADR-0002 — Backend: .NET 9 (no FastAPI)

**Estado:** ✅ Aceptado
**Fecha:** 2026-07-27
**Resuelve:** `00-Project-Charter/CHARTER.md` Open Question #3

## Contexto

`MASTER.md` dejó el stack de API abierto entre .NET 9 y FastAPI. La
decisión bloqueaba `04-Architecture/` y `08-Backend/`. Los requisitos que
más pesan: DDD + Clean/Hexagonal + CQRS obligatorios (`MASTER.md §3`),
escala objetivo de 100k empresas / 50M de OT, y equipos de backend
probablemente grandes y de larga duración (proyecto multi-año, no un
prototipo).

## Decisión

**.NET 9** como runtime y lenguaje principal del backend.

Razones:
- Tooling maduro para DDD/CQRS en equipos grandes (MediatR o equivalente
  para el pipeline de comandos/queries; convenciones muy establecidas para
  Clean/Hexagonal Architecture).
- Tipado fuerte de punta a punta reduce errores en un dominio con muchas
  entidades relacionadas (`03-Domain-Model/`) y facilita mantener contratos
  estables a través de 100k tenants.
- Rendimiento y manejo de concurrencia adecuados para 50M de OT y jobs en
  background (recordatorios de mantenimiento preventivo, generación de
  read-models de BI vía Outbox — ver ADR-0001).
- Ecosistema first-class para PostgreSQL (EF Core / Npgsql) sin fricción.
- Facilita exponer contratos OpenAPI consistentes, requisito de `MASTER.md`.

## Alternativas consideradas

1. **FastAPI (Python)** — más rápido para prototipar, ecosistema fuerte en
   IA/ML (relevante para `10-AI/`). Descartado como framework *principal*
   de todo el backend transaccional porque el tipado dinámico y el
   ecosistema de DDD/CQRS son menos maduros a esta escala y con equipos
   grandes; sí se conserva como candidato para servicios específicos de IA
   (ver Consecuencias).
2. **Node.js/TypeScript en el backend** (compartir lenguaje con el
   frontend) — descartado por preferencia explícita de patrones DDD/Clean
   Architecture más estandarizados en .NET para este tamaño de dominio.

## Consecuencias

- `08-Backend/README.md` se documenta asumiendo .NET 9, Clean/Hexagonal
  Architecture, EF Core sobre PostgreSQL.
- `10-AI/README.md`, cuando se redacte, puede proponer microservicios en
  Python/FastAPI específicamente para cargas de IA/ML, orquestados desde el
  backend .NET vía API interna — no es una contradicción de este ADR, es un
  límite de servicio explícito a definir ahí.
- Este ADR se puede revisar solo mediante un nuevo ADR que lo reemplace
  explícitamente (regla `MASTER.md §3`).
