# 09-Mobile

> App de campo offline-first para técnicos: sincronización, almacenamiento local, flujos críticos sin conectividad.

**Estado:** `Draft v0.1`
**Depende de:** `04-Architecture/README.md` (regla offline #5), `05-Database/README.md` (columnas de sync), `06-Modules/02-Operaciones`, `06-Modules/04-RRHH`
**De este documento dependen:** `08-Backend/` (endpoint de sincronización), `13-Security/` (almacenamiento seguro de sesión en dispositivo)

---

## Purpose

Especificar cómo un técnico ejecuta su trabajo sin señal de red, sin
perder datos y sin bloquear su jornada — la app móvil no es una versión
reducida del sitio web, es un producto propio con un único usuario en
mente: el técnico de campo.

## Responsibilities

Cubre: stack móvil, estrategia de almacenamiento local y sincronización,
resolución de conflictos, flujos soportados. **No cubre**: pantallas de
administración, configuración, BI o facturación — explícitamente fuera de
alcance de esta app (regla de Scope).

## Scope

**Dentro:** ejecutar OT asignadas y **crear OT espontáneas propias**
(`06-Modules/02-Operaciones` regla #11), ver ruta del día
(`06-Modules/03-Logistica`, solo lectura), marcar asistencia/turno y
solicitar licencia (`06-Modules/04-RRHH`).
**Fuera:** cualquier pantalla de administración, activos, clientes,
finanzas o BI — eso vive solo en `07-Frontend/` (back-office web).

## Functional Description

### Stack (confirmado)

**React Native** (comparte TypeScript/React con el equipo de
`07-Frontend/`, reduce la curva de aprendizaje entre ambos equipos) +
almacenamiento local embebido (SQLite vía una librería de sync offline,
p. ej. WatermelonDB o equivalente) para las tablas necesarias sin
conexión.

### Datos disponibles offline

Subconjunto de `05-Database/` relevante al técnico, descargado al iniciar
sesión y mientras hay señal: `WorkOrder` asignadas a su `Crew`,
`ChecklistTemplate` de esas OT, su `Shift` del día, su `Route` (solo
lectura). Toda tabla sincronizable usa las columnas
`updated_at`/`sync_version` fijadas en `05-Database/README.md` §Offline.

### Flujo de sincronización

1. Toda acción del técnico (`StartWorkOrder`, `AddWorkOrderEvidence`,
   `GenerateWorkOrderReceipt`, `CloseWorkOrder`, `RegisterAttendance`) se
   escribe primero en la cola local, nunca espera respuesta de red para
   reflejarse en la UI.
2. La sincronización es **automática al detectar conexión**, pero el
   técnico también tiene un **botón de "Sincronización Manual"** (aporte
   del colega) para forzarla sin esperar al detector automático — útil en
   redes intermitentes donde la detección tarda en confirmar.
3. Al sincronizar, la app envía la cola completa en orden de creación a un
   endpoint de sincronización batch (`08-Backend/`, `/sync`).
4. El backend re-valida cada comando con las mismas reglas que si viniera
   de la web (`06-Modules/02-Operaciones` regla #4: no se puede cerrar sin
   evidencia, ni ahí ni acá) — la validación offline en el cliente es UX,
   no autoridad.
5. Si el servidor rechaza un comando ya ejecutado localmente (p. ej. la OT
   fue reabierta remotamente mientras el técnico trabajaba offline), el
   conflicto se muestra explícitamente al técnico — nunca se resuelve en
   silencio con una regla automática que pueda ocultar una pérdida de
   trabajo.

### Captura de evidencia fotográfica (aporte del colega)

La cámara **interna de la app** (nunca la galería del dispositivo) estampa
de forma visible sobre la imagen las coordenadas GPS, fecha y hora en el
momento de la captura — coherente con
`06-Modules/02-Operaciones/README.md` regla #8. El archivo se guarda
localmente ya estampado; la subida en segundo plano (regla de negocio #4
de este documento) sube la imagen final, no la original sin marca.

### Remito firmado en el dispositivo (aporte del colega)

Al ejecutar `CloseWorkOrder`, si el cliente está presente, la app dispara
`GenerateWorkOrderReceipt` (`06-Modules/02-Operaciones/README.md` regla
#9): arma el PDF localmente con los datos de la OT y una pantalla de firma
táctil para el cliente. El PDF firmado queda disponible para entregarlo o
enviarlo apenas haya señal — no depende de que el backend lo genere.

### Alerta de almacenamiento (aporte del colega)

Como toda evidencia (fotos ya estampadas, PDFs de remito) se guarda
localmente hasta sincronizar, la app monitorea el espacio disponible del
dispositivo y **alerta al técnico cuando el almacenamiento está bajo**,
indicando que debe sincronizar pronto — antes de que falle una captura
por falta de espacio, no después.

## Business Rules

1. La app nunca bloquea una acción del técnico por falta de red — toda
   operación se encola localmente (regla heredada de
   `04-Architecture/README.md` #5).
2. La sincronización respeta el orden de creación local de los comandos —
   no se reordenan al enviar.
3. Un conflicto de sincronización se muestra al técnico, nunca se resuelve
   automáticamente de forma silenciosa (regla #4 de Functional Description).
4. La evidencia fotográfica se sube en segundo plano — no bloquea el
   avance del checklist mientras se transmite.
5. El botón de Sincronización Manual dispara el mismo flujo que la
   sincronización automática — no es un camino alternativo con reglas
   distintas, solo un disparador adicional.
6. Ninguna foto de evidencia se acepta si no viene de la cámara interna de
   la app con el estampado de GPS/fecha/hora aplicado (regla heredada de
   `06-Modules/02-Operaciones/README.md` regla #8) — la app ni siquiera
   ofrece la opción de adjuntar desde la galería.
7. La alerta de almacenamiento bajo se dispara **antes** de que una
   captura falle por falta de espacio, con margen suficiente para que el
   técnico alcance a sincronizar — no es una notificación reactiva a un
   error ya ocurrido.

## Data Model

Espejo local (SQLite) del subconjunto de entidades listado en "Datos
disponibles offline" — no se define un modelo nuevo, es un subconjunto de
`05-Database/README.md` con las mismas columnas de auditoría/sync.

## UX

Un flujo lineal por pantalla (checklist → fotos → firma → cerrar), ya
descrito en `06-Modules/02-Operaciones/README.md` §Screens. Ver `14-UX/`
para el sistema de diseño compartido con la web.

## Security

Token de sesión almacenado en el almacén seguro nativo del dispositivo
(Keychain en iOS, Keystore en Android) — nunca en almacenamiento plano,
dado que el técnico puede permanecer offline (y por tanto sin re-login)
por horas. Detalle completo en `13-Security/`.

## API

Mismo contrato REST del backend (`08-Backend/`) más un endpoint específico
`/sync` que acepta un lote de comandos pendientes en una sola llamada.

## Events

Consume `WorkOrderDispatched` y `TechnicianAvailabilityChanged` para
mantener la vista de "Mi Ruta" actualizada cuando hay señal; en su
ausencia, opera sobre el último dato descargado.

## Dependencies

Depende de `04-Architecture/` (patrón offline ya anticipado), `05-Database/`
(columnas de sync) y de `06-Modules/02-Operaciones` /
`06-Modules/04-RRHH` (flujos soportados). De este documento depende
directamente `08-Backend/` (debe exponer `/sync`) y `13-Security/`
(almacenamiento seguro del token).

## Future Improvements

- Soporte offline para roles adicionales (hoy exclusivo del técnico).
- Mapas offline para la vista de ruta (hoy la ruta se ve, pero la
  navegación en sí puede depender de un mapa online).
- Compresión/calidad configurable de fotos ya estampadas, si el volumen de
  evidencia empieza a presionar el almacenamiento del dispositivo con más
  frecuencia de la esperada (relacionado con la alerta de la regla #7).

## Open Questions

1. **Resuelto:** framework mobile confirmado como **React Native**.
2. ¿Qué tan atrás en el tiempo puede quedar un técnico sin sincronizar
   antes de que la app fuerce una re-sincronización completa (vs. cola
   incremental)? Definir un límite razonable (p. ej. jornada laboral).
3. ¿A qué porcentaje/valor absoluto de almacenamiento libre se dispara la
   alerta de la regla #7? Definir un umbral concreto, no solo el principio.

> Agregado en esta iteración a partir del aporte de un colega sobre la
> app de campo: botón de sincronización manual (regla #5), estampado
> obligatorio de GPS/fecha/hora en fotos (regla #6), alerta de
> almacenamiento bajo (regla #7), y generación del remito firmado en el
> dispositivo (ver Functional Description).
