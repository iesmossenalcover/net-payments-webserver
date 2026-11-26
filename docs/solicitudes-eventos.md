# Solicitudes de eventos — resumen de implementación

Documento de trabajo para poder retomar la implementación si se interrumpe.
Las reglas de negocio vienen del documento de diseño (cerrado); aquí solo se recoge
cómo se aplican en este repo y en qué punto está el trabajo.

## Objetivo

Los profesores piden eventos desde la app; el admin los revisa, pide correcciones,
aprueba/rechaza y crea el evento a partir de la solicitud. Todo dentro de este backend
y del front actual (`../next-payments-app`).

## Decisiones (adaptación al repo)

| Tema | Decisión |
|---|---|
| Autenticación | La actual: login Google → cookie. Nada de Firebase ni token por petición. |
| Profesor | Rol `reader` o superior (policy `READER`). Solo ve/edita **sus** solicitudes (`RequesterUserId == UserId` de la cookie). |
| Admin | Rol `advancedadmin` o `superuser` (policy `ADVANCED_ADMIN`). Puede crear sus propias solicitudes y aprobarlas. |
| Ids | `long` como el resto de entidades. |
| "App de pagos" | Es este backend: crear el `Event` y pasar la solicitud a `CREADA` va en una sola transacción (`IUnitOfWork`). |
| Concurrencia | Sin campo `version`. Cada transición es un `UPDATE ... WHERE id = ? AND status = <esperado>`; 0 filas → «Esta solicitud ya ha cambiado de estado». |
| Formulario | **Fuera de foco por ahora.** Solo los campos mínimos del diseño como columnas: título, descripción, fecha inicio/fin, precio, fecha límite de pago. Se ampliará más adelante. El admin completa/ajusta todo al crear el evento (`EventData`), con el front precargado desde la solicitud. |
| Destinatarios | Tabla propia (`Person`/`Group` + id). Al crear el evento se asignan automáticamente. |
| Borrado de evento | Se bloquea si algún `EventPerson` tiene `Paid = true` (hoy se borraba en cascada). «Devolver» = el admin desmarca el pago. Si el evento venía de una solicitud → la solicitud vuelve a `PENDIENTE_REVISION`, `EventId = null`, actor SISTEMA. |
| Actor en transiciones automáticas | Quien las provoca (p. ej. el admin que edita en `PENDIENTE_CREACION`); SISTEMA (`ActorUserId = null`) solo si no hay usuario. |
| Emails | Servicio Gmail existente (`IGoogleAdminApi.SendHtmlEmail`). Admin: nueva / corregida. Profesor: corrección pedida, rechazada, **eliminada**, evento creado. Destinatario admin por configuración. |

## Estados y transiciones

Estados: `PendingReview`, `PendingCorrection`, `PendingCreation`, `Created`, `Rejected` (final, se conserva con el motivo para el historial), `Deleted` (borrado lógico, `DeletedAt`).

| Origen | Acción | Actor | Destino |
|---|---|---|---|
| — | Enviar | Profesor | PendingReview |
| PendingReview | Aprobar (fija `ChangesDeadline` + email al profesor) | Admin | PendingCreation |
| PendingReview | Pedir cambios (motivo) | Admin | PendingCorrection |
| PendingReview | Editar | Admin | PendingReview |
| PendingCorrection | Corregir y reenviar | Profesor propietario | PendingReview |
| PendingCreation | Editar | Admin | PendingReview |
| PendingCreation | Editar, solo hasta `ChangesDeadline` (email al admin) | Profesor propietario | PendingReview |
| PendingCreation | Pedir corrección (motivo) | Admin | PendingCorrection |
| PendingCreation | Crear evento | Admin | Created (si falla, no cambia) |
| Created | Se borra el evento | Sistema | PendingReview (`EventId = null`) |
| Review/Correction/Creation | Rechazar (motivo + email) | Admin | Rejected |
| Review/Correction/Creation/Rejected | Eliminar (email) | Admin | Deleted |

Reglas: `Created` es inmutable y no se elimina; `Rejected` es inmutable pero sí se puede eliminar (decidido el 2026-11-26: rechazar ya no borra, para mantener el historial); el admin no edita en `PendingCorrection`;
el profesor edita en `PendingCorrection` y en `PendingCreation` mientras no haya pasado `ChangesDeadline`
(**cambio sobre el diseño original**, decidido el 2026-11-23); `Deleted` no se ve para nadie; cada cambio
de estado se registra en `EventRequestTransition` (origen, destino, actor, motivo, fecha).

Plazo de cambios: al aprobar, el admin elige `ChangesDeadline` y el profesor recibe un email con la fecha.
Solo limita al profesor: el admin puede crear el evento en cualquier momento, incluso antes del plazo.
El plazo y la elección de Google Calendar (`SyncToCalendar`) se borran si la solicitud vuelve a revisión o corrección; al reaprobar se eligen de nuevo.

## Modelo

- `EventRequest`: `Id`, `RequesterUserId`, `CourseId`, `Status`, `Title`, `Description`,
  `StartDate`, `EndDate?`, `Price`, `PaymentDeadline?`, `ChangesDeadline?`, `SyncToCalendar` (migración `EventRequestSyncToCalendar`), `EventId?` (único, FK a `Event` con `SET NULL`), `CreatedAt`, `UpdatedAt`, `DeletedAt?`.
- `EventRequestRecipient`: `EventRequestId`, `Type` (Person/Group), `ReferenceId`.
- `EventRequestTransition`: `EventRequestId`, `From?`, `To`, `ActorUserId?`, `Reason?`, `Date`.
- `EventRequestComment`: `EventRequestId`, `AuthorUserId`, `Text`, `Date` (flujo aún sin definir).

## Endpoints

Profesor (policy `READER`, siempre filtrado por propietario):
- `GET /api/event-requests/mine` (solo del curso actual), `GET /api/event-requests/mine/{id}`
- `POST /api/event-requests` (enviar)
- `PUT /api/event-requests/mine/{id}` (corregir y reenviar, o editar aprobada dentro del plazo)
- `GET /api/event-requests/groups/{groupId}/people` (alumnos de un grupo en el curso actual, solo id y nombre)

Admin (policy `ADVANCED_ADMIN`):
- `GET /api/event-requests?status=&courseId=` (sin `courseId`, el curso actual; `/api/courses/selector` se abrió a ADVANCED_ADMIN para el filtro), `GET /api/event-requests/{id}`
- `PUT /api/event-requests/{id}` (editar)
- `POST /api/event-requests/{id}/approve` (`changesDeadline`, `syncToCalendar`)
- `POST /api/event-requests/{id}/request-correction` (`reason`)
- `POST /api/event-requests/{id}/reject` (`reason`)
- `POST /api/event-requests/{id}/delete`
- `POST /api/event-requests/{id}/create-event` (body `EventData`). Si la solicitud tiene `SyncToCalendar` (lo elige el admin al aprobar, sin valor por defecto en el front), tras crear el evento (fuera de la transacción) lo crea en Google Calendar; si falla, el evento queda creado y se devuelve `CalendarSynced = false` + `CalendarError`.

## Progreso

Rama: `feature/event-requests`. Commits a nombre del usuario, sin coautor.

- [x] 1. Bloquear borrado de eventos con pagos (`DeleteEventCommand`) — cambio independiente.
- [x] 2. Dominio: entidades + máquina de estados (`src/Domain/Entities/EventRequests`).
- [x] 3. Infraestructura: `AppDbContext`, `EventRequestsRepository`, migración `EventRequests`.
- [x] 4. Comandos/queries (`src/Application/EventRequests`) + endpoints (`src/WebServer/Handlers/EventRequests.cs`, `RegisterRoutes.cs`).
- [x] 5. Crear evento desde solicitud (`CreateEventFromRequestCommand`: transacción + asignar destinatarios).
- [x] 6. Borrado de evento → solicitud a revisión (`DeleteEventCommand`).
- [x] 7. Emails (`EventRequestNotifier`). Configuración: `EventRequestAdminEmailRecipient`, `FrontEventRequestUrl`, `FrontAdminEventRequestUrl` ({id}). **Hay que añadirlas en producción**; si faltan, no se envía y queda aviso en el log.
- [x] 8. Front (`../next-payments-app`, rama `feature/event-requests`):
  - Profesor: `/admin/event-requests` (las mías), `/create`, `/[id]` (corregir si está en corrección).
  - Admin: `/admin/event-requests/review` (por defecto, lo que le toca: revisión + creación), `/review/[id]` (acciones según estado; «Crear esdeveniment» precarga `EventFields` con la solicitud).
  - Menú lateral y `next-routes.conf` actualizados.
  - `/admin/calendar` (READER): calendario a pantalla completa con eventos y solicitudes, el mismo que el selector de fechas (`components/calendar/EventCalendar.tsx`).
- [ ] 9. Probar el flujo completo en local (migración aplicada) y desplegar.

## Pendiente de definir

- Campos concretos del formulario (de momento solo los mínimos; se descartó guardarlo como JSON).
- Flujo de comentarios (quién, cuándo, si notifica).
- Destinatarios (2026-11-26): el formulario lista los grupos y cada uno se despliega para elegir alumnos.
  Grupo entero → destinatario `Group` (incluye altas posteriores); solo algunos → un destinatario `Person`
  por alumno. El detalle devuelve el grupo de cada alumno (`GroupId`/`GroupName`) para agruparlos.
