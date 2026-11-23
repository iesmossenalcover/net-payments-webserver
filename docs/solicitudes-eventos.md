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
- [ ] 3. Infraestructura: `AppDbContext`, repositorio, migración.
- [ ] 4. Comandos/queries (MediatR) + endpoints + policies.
- [ ] 5. Crear evento desde solicitud (transacción + asignar destinatarios).
- [ ] 6. Borrado de evento → solicitud a revisión.
- [ ] 7. Emails.
- [ ] 8. Front (`../next-payments-app`): pantallas profesor y admin.

## Pendiente de definir

- Campos concretos del formulario (de momento solo los mínimos; se descartó guardarlo como JSON).
- Flujo de comentarios (quién, cuándo, si notifica).
