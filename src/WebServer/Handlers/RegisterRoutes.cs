namespace WebServer.Handlers;
public static class RegisterRoutes
{
    public static void MapRoutes(this WebApplication app)
    {
        app.MapGet("/api/health", () => "up")
            .WithName("Health")
            .WithOpenApi();

        // auth related actions are in WebServer layer dueto related session storage.
        app.MapPost("/api/signin", Authentication.Auth.SigninPost)
            .WithName("signin")
            .WithOpenApi();

        app.MapPost("/api/signup", Authentication.Auth.SignupPost)
            .RequireAuthorization()
            .WithName("signup")
            .WithOpenApi();

        app.MapGet("/api/identity", Authentication.Auth.GetIdentity)
            .RequireAuthorization()
            .WithName("identity")
            .WithOpenApi();

        // Tasks
        app.MapPost("/api/tasks/people", Tasks.UploadPeople)
            .RequireAuthorization()
            .WithName("Upload people")
            .WithOpenApi();

        // People
        app.MapGet("/api/people/{id}", People.GetPerson)
            .RequireAuthorization()
            .WithName("Get person by id")
            .WithOpenApi();

        app.MapGet("/api/people", People.ListPeople)
            .RequireAuthorization()
            .WithName("List people by course")
            .WithOpenApi();

        app.MapPost("/api/people", People.CreatePerson)
            .RequireAuthorization()
            .WithName("Create person")
            .WithOpenApi();

        app.MapPut("/api/people/{id}", People.UpdatePerson)
            .RequireAuthorization()
            .WithName("Update person")
            .WithOpenApi();

        app.MapDelete("/api/people/{id}", People.DeletePerson)
            .RequireAuthorization()
            .WithName("Delete person")
            .WithOpenApi();

        // Courses
        app.MapGet("/api/courses/selector", Courses.GetCoursesSelector)
            .RequireAuthorization()
            .WithName("Get courses selector")
            .WithOpenApi();

        // Groups
        app.MapGet("/api/groups/selector", Groups.GetGroupsSelector)
            .RequireAuthorization()
            .WithName("Get groups selector")
            .WithOpenApi();

        // Events
        app.MapGet("/api/events", Events.ListCourseEvents)
            .RequireAuthorization()
            .WithName("List current course events")
            .WithOpenApi();

        app.MapGet("/api/events/{id}", Events.GetEvent)
            .RequireAuthorization()
            .WithName("Get event by id")
            .WithOpenApi();

        app.MapPost("/api/events", Events.CreateEvent)
            .RequireAuthorization()
            .WithName("Create event")
            .WithOpenApi();

        app.MapPut("/api/events/{id}", Events.UpdateEvent)
            .RequireAuthorization()
            .WithName("Update event")
            .WithOpenApi();

        app.MapDelete("/api/events/{id}", Events.DeleteEvent)
            .RequireAuthorization()
            .WithName("Delete event")
            .WithOpenApi();

        app.MapPost("/api/events/active", Events.ListActivePersonEvents)
            .WithName("Get active events by person document Id")
            .WithOpenApi();

        // Events People
        app.MapPost("/api/events/{eventCode}/people", Events.SetPeopleToEvent)
            .RequireAuthorization()
            .WithName("Set people to event")
            .WithOpenApi();

        app.MapGet("/api/events/{eventCode}/people", Events.GetPeopleEvent)
            .RequireAuthorization()
            .WithName("Get people in event")
            .WithOpenApi();

        app.MapGet("/api/events/{eventCode}/payments", Events.ListEventPayments)
                .RequireAuthorization()
                .WithName("List event payments")
                .WithOpenApi();

        app.MapGet("/api/events/{eventCode}/summary", Events.ListEventSummary)
                .WithName("List event summary")
                .WithOpenApi();


        // Orders
        app.MapPost("/api/orders", Orders.CreateOrder)
            .WithName("Create order")
            .WithOpenApi();

        app.MapPost("/api/orders/confirm", Orders.ConfirmOrderPost)
            .WithName("Confirm order post")
            .WithOpenApi();

        app.MapGet("/api/order/info", Orders.GetOrderInfo)
            .WithName("Get order info")
            .WithOpenApi();

        // Admin Info

        app.MapGet("/api/admin", AdminInfo.GetAdminInfo)
            .RequireAuthorization()
            .WithName("Get admin info")
            .WithOpenApi();

        app.MapPut("/api/config", AdminInfo.UpdateAdminInfo)
            .RequireAuthorization()
            .WithName("Update app config")
            .WithOpenApi();
    }
}
