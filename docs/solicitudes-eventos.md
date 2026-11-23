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

Estados: `PendingReview`, `PendingCorrection`, `PendingCreation`, `Created`, `Deleted` (borrado lógico, `DeletedAt`).

| Origen | Acción | Actor | Destino |
|---|---|---|---|
| — | Enviar | Profesor | PendingReview |
| PendingReview | Aprobar | Admin | PendingCreation |
| PendingReview | Pedir cambios (motivo) | Admin | PendingCorrection |
| PendingReview | Editar | Admin | PendingReview |
| PendingCorrection | Corregir y reenviar | Profesor propietario | PendingReview |
| PendingCreation | Editar | Admin | PendingReview |
| PendingCreation | Pedir corrección (motivo) | Admin | PendingCorrection |
| PendingCreation | Crear evento | Admin | Created (si falla, no cambia) |
| Created | Se borra el evento | Sistema | PendingReview (`EventId = null`) |
| Review/Correction/Creation | Rechazar (motivo + email) | Admin | Deleted |
| Review/Correction/Creation | Eliminar (email) | Admin | Deleted |

Reglas: `Created` es inmutable y no se elimina; el admin no edita en `PendingCorrection`;
el profesor solo edita en `PendingCorrection`; `Deleted` no se ve para nadie; cada cambio
de estado se registra en `EventRequestTransition` (origen, destino, actor, motivo, fecha).

## Modelo

- `EventRequest`: `Id`, `RequesterUserId`, `CourseId`, `Status`, `Title`, `Description`,
  `StartDate`, `EndDate?`, `Price`, `PaymentDeadline?`, `EventId?` (único, FK a `Event` con `SET NULL`), `CreatedAt`, `UpdatedAt`, `DeletedAt?`.
- `EventRequestRecipient`: `EventRequestId`, `Type` (Person/Group), `ReferenceId`.
- `EventRequestTransition`: `EventRequestId`, `From?`, `To`, `ActorUserId?`, `Reason?`, `Date`.
- `EventRequestComment`: `EventRequestId`, `AuthorUserId`, `Text`, `Date` (flujo aún sin definir).

## Endpoints

Profesor (policy `READER`, siempre filtrado por propietario):
- `GET /api/event-requests/mine`, `GET /api/event-requests/mine/{id}`
- `POST /api/event-requests` (enviar)
- `PUT /api/event-requests/mine/{id}` (corregir y reenviar)

Admin (policy `ADVANCED_ADMIN`):
- `GET /api/event-requests?status=`, `GET /api/event-requests/{id}`
- `PUT /api/event-requests/{id}` (editar)
- `POST /api/event-requests/{id}/approve`
- `POST /api/event-requests/{id}/request-correction` (`reason`)
- `POST /api/event-requests/{id}/reject` (`reason`)
- `POST /api/event-requests/{id}/delete`
- `POST /api/event-requests/{id}/create-event` (body `EventData`)

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
- [ ] 9. Probar el flujo completo en local (migración aplicada) y desplegar.

## Pendiente de definir

- Campos concretos del formulario (de momento solo los mínimos; se descartó guardarlo como JSON).
- Flujo de comentarios (quién, cuándo, si notifica).
- Alumnos individuales como destinatarios: el back los admite, pero el profesor (reader) no puede buscar
  personas (`/api/people/filter` es ADMIN). De momento el formulario solo propone grupos
  (`/api/groups/selector` se abrió a READER). Decidir si se abre la búsqueda de alumnos al profesorado.
