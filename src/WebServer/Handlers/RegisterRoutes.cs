using WebServer.Handlers;

namespace net_payments_webserver.WebServer.Handlers;

public static class RegisterRoutes
{
    public static void MapRoutes(this WebApplication app)
    {
        app.MapGet("/api/health", () => "up")
            .WithName("Health")
            .WithOpenApi();

        app.MapPost("/api/signin", global::WebServer.Handlers.Authentication.Auth.SigninPost)
            .WithName("signin")
            .WithOpenApi();

        app.MapPost("/api/oauth/", global::WebServer.Handlers.Authentication.Auth.SigninOAuth)
            .WithName("External OAuth Signin")
            .WithOpenApi();

        app.MapGet("/api/identity", global::WebServer.Handlers.Authentication.Auth.GetIdentity)
            .RequireAuthorization()
            .WithName("identity")
            .WithOpenApi();

        app.MapPost("/api/signup", global::WebServer.Handlers.Authentication.Auth.SignupPost)
            .RequireAuthorization("Admin")
            .WithName("signup")
            .WithOpenApi();

        // Jobs
        app.MapGet("/api/processes", Processes.GetProcessess)
            .WithName("Get Last Process")
            .RequireAuthorization("Superuser")
            .WithOpenApi();

        app.MapGet("/api/processes/logs/{id}", Processes.GetLog)
            .WithName("Get Log")
            .RequireAuthorization("Superuser")
            .WithOpenApi();

        app.MapPost("/api/processes", Processes.StartProcess)
            .WithName("Start process")
            .RequireAuthorization("Superuser")
            .WithOpenApi();

        // Tasks
        app.MapGet("/api/tasks/people", Tasks.GetPeopleBatchUploadTemplate)
            .RequireAuthorization("Admin")
            .WithName("Get people batch upload template")
            .WithOpenApi();

        app.MapPost("/api/tasks/people", Tasks.PeopleBatchUpload)
            .RequireAuthorization("Admin")
            .WithName("Upload people")
            .WithOpenApi();

        app.MapGet("/api/googleworkspace/people/export", GoogleWorkspace.ExportPeopleGoogleWorkspace)
            .WithName("Export people")
            .RequireAuthorization("Superuser")
            .WithOpenApi();

        app.MapPost("/api/googleworkspace/people/sync/{id}", GoogleWorkspace.SyncPersonToGoogleWorkspace)
            .WithName("Sync person")
            .RequireAuthorization("Admin")
            .WithOpenApi();

        app.MapPost("/api/googleworkspace/people/{id}/password", GoogleWorkspace.UpdatePasswordGoogleWorkspace)
            .WithName("Set password person")
            .RequireAuthorization("Admin")
            .WithOpenApi();

        app.MapPost("/api/googleworkspace/people/{id}/ou", GoogleWorkspace.MoveOUGoogleWorkspace)
            .WithName("Update ou person")
            .RequireAuthorization("Admin")
            .WithOpenApi();

        //Wifi
        app.MapGet("/api/wifi/export", Wifi.ExportWifiUsers)
            .WithName("Export wifi users")
            .RequireAuthorization("Superuser")
            .WithOpenApi();


        // People
        app.MapGet("/api/people/export", People.ExportPeople)
            .RequireAuthorization("Admin")
            .WithName("Export people csv")
            .WithOpenApi();

        app.MapGet("/api/people/{id}", People.GetPerson)
            .RequireAuthorization("Admin")
            .WithName("Get person by id")
            .WithOpenApi();

        app.MapGet("/api/people", People.ListPeople)
            .RequireAuthorization("Admin")
            .WithName("List people by course")
            .WithOpenApi();

        app.MapGet("/api/people/filter", People.FilterPeople)
            .RequireAuthorization("Admin")
            .WithName("Filter people")
            .WithOpenApi();

        app.MapPost("/api/people", People.CreatePerson)
            .RequireAuthorization("Admin")
            .WithName("Create person")
            .WithOpenApi();

        app.MapPut("/api/people/{id}", People.UpdatePerson)
            .RequireAuthorization("Admin")
            .WithName("Update person")
            .WithOpenApi();

        app.MapDelete("/api/people/{id}", People.DeletePerson)
            .RequireAuthorization("Admin")
            .WithName("Delete person")
            .WithOpenApi();

        app.MapGet("/api/people/{id}/payments", People.PersonPayments)
            .RequireAuthorization("Admin")
            .WithName("Person payments")
            .WithOpenApi();

        // Courses
        app.MapGet("/api/courses/{id}", Courses.GetCourse)
            .RequireAuthorization("Admin")
            .WithName("Get course by id")
            .WithOpenApi();

        app.MapGet("/api/courses", Courses.GetAllCourses)
            .RequireAuthorization("Admin")
            .WithName("Get all courses")
            .WithOpenApi();

        app.MapGet("/api/courses/selector", Courses.GetCoursesSelector)
            .RequireAuthorization("Admin")
            .WithName("Get courses selector")
            .WithOpenApi();

        app.MapPost("/api/courses", Courses.CreateCourse)
            .RequireAuthorization("Admin")
            .WithName("Create course")
            .WithOpenApi();

        app.MapPut("/api/courses/{id}", Courses.UpdateCourse)
            .RequireAuthorization("Admin")
            .WithName("Update course")
            .WithOpenApi();

        app.MapPut("/api/courses/{id}/active", Courses.SetActiveCourse)
            .RequireAuthorization("Admin")
            .WithName("Set active course")
            .WithOpenApi();

        //OU Relations
        app.MapGet("/api/ougrouprelations", OuRelations.ListOuRelations)
            .RequireAuthorization("Superuser")
            .WithName("List ou relations")
            .WithOpenApi();

        app.MapGet("/api/ougrouprelations/{id}", OuRelations.GetOuRelation)
            .RequireAuthorization("Superuser")
            .WithName("Get ou relation by id")
            .WithOpenApi();

        app.MapPost("/api/ougrouprelations", OuRelations.CreateOuRelation)
            .RequireAuthorization("Superuser")
            .WithName("Create ou relation")
            .WithOpenApi();

        app.MapPut("/api/ougrouprelations/{id}", OuRelations.UpdateOuRelation)
            .RequireAuthorization("Superuser")
            .WithName("Update ou relation")
            .WithOpenApi();

        app.MapDelete("/api/ougrouprelations/{id}", OuRelations.DeleteOuRelation)
            .RequireAuthorization("Superuser")
            .WithName("Delete ou relation")
            .WithOpenApi();

        // Groups
        app.MapGet("/api/groups/selector", Groups.GetGroupsSelector)
            .RequireAuthorization("Admin")
            .WithName("Get groups selector")
            .WithOpenApi();

        app.MapGet("/api/groups", Groups.ListGroups)
            .RequireAuthorization("Admin")
            .WithName("List groups")
            .WithOpenApi();

        app.MapGet("/api/groups/{id}", Groups.GetGroup)
            .RequireAuthorization("Admin")
            .WithName("Get group by id")
            .WithOpenApi();

        app.MapPost("/api/groups", Groups.CreateGroup)
            .RequireAuthorization("Admin")
            .WithName("Create group")
            .WithOpenApi();

        app.MapPut("/api/groups/{id}", Groups.UpdateGroup)
            .RequireAuthorization("Admin")
            .WithName("Update group")
            .WithOpenApi();

        // Events
        app.MapGet("/api/events", Events.ListCourseEvents)
            .RequireAuthorization("Admin")
            .WithName("List current course events")
            .WithOpenApi();

        app.MapGet("/api/events/{id}", Events.GetEvent)
            .RequireAuthorization("Admin")
            .WithName("Get event by id")
            .WithOpenApi();

        app.MapPost("/api/events", Events.CreateEvent)
            .RequireAuthorization("Admin")
            .WithName("Create event")
            .WithOpenApi();

        app.MapPut("/api/events/{id}", Events.UpdateEvent)
            .RequireAuthorization("Admin")
            .WithName("Update event")
            .WithOpenApi();

        app.MapDelete("/api/events/{id}", Events.DeleteEvent)
            .RequireAuthorization("Admin")
            .WithName("Delete event")
            .WithOpenApi();

        app.MapPost("/api/events/active", Events.ListActivePersonEvents)
            .WithName("Get active events by person document Id")
            .WithOpenApi();

        app.MapGet("/api/events/export", Events.ExportEvents)
            .RequireAuthorization("Admin")
            .WithName("Export events info")
            .WithOpenApi();

        // Events People
        app.MapPost("/api/events/{eventCode}/people", Events.SetPeopleToEvent)
            .RequireAuthorization("Admin")
            .WithName("Set people to event")
            .WithOpenApi();

        app.MapGet("/api/events/{eventCode}/people", Events.GetPeopleEvent)
            .RequireAuthorization("Admin")
            .WithName("Get people in event")
            .WithOpenApi();

        app.MapGet("/api/events/{eventCode}/payments", Events.ListEventPayments)
            .RequireAuthorization("Admin")
            .WithName("List event payments")
            .WithOpenApi();

        app.MapPut("/api/events/{eventPersonId}/payment", Events.SetPersonEventPaid)
            .RequireAuthorization("Admin")
            .WithName("Set person event paid/not paid")
            .WithOpenApi();

        app.MapGet("/api/events/{eventCode}/summary", Events.ListEventSummary)
            .RequireAuthorization("Reader")
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
            .RequireAuthorization("Admin")
            .WithName("Get admin info")
            .WithOpenApi();

        app.MapPut("/api/config", AdminInfo.UpdateAdminInfo)
            .RequireAuthorization("Admin")
            .WithName("Update app config")
            .WithOpenApi();
    }
}