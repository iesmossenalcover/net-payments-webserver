namespace WebServer.Handlers;

public static class RegisterRoutes
{
    public static void MapRoutes(this WebApplication app)
    {
        app.MapGet("/api/health", () => "up")
            .WithName("Health");

        app.MapPost("/api/signin", global::WebServer.Handlers.Authentication.Auth.SigninPost)
            .WithName("signin");

        app.MapPost("/api/oauth/", global::WebServer.Handlers.Authentication.Auth.SigninOAuth)
            .WithName("External OAuth Signin");

        app.MapGet("/api/identity", global::WebServer.Handlers.Authentication.Auth.GetIdentity)
            .RequireAuthorization()
            .WithName("identity");

        app.MapPost("/api/signup", global::WebServer.Handlers.Authentication.Auth.SignupPost)
            .RequireAuthorization("Admin")
            .WithName("signup");

        // Jobs
        app.MapGet("/api/processes", Processes.GetProcessess)
            .WithName("Get Last Process")
            .RequireAuthorization("Superuser");

        app.MapGet("/api/processes/logs/{id}", Processes.GetLog)
            .WithName("Get Log")
            .RequireAuthorization("Superuser");

        app.MapPost("/api/processes", Processes.StartProcess)
            .WithName("Start process")
            .RequireAuthorization("Superuser");

        // Tasks
        app.MapGet("/api/tasks/people", Tasks.GetPeopleBatchUploadTemplate)
            .RequireAuthorization("Admin")
            .WithName("Get people batch upload template");

        app.MapPost("/api/tasks/people", Tasks.PeopleBatchUpload)
            .RequireAuthorization("Admin")
            .WithName("Upload people");

        app.MapGet("/api/googleworkspace/people/export", GoogleWorkspace.ExportPeopleGoogleWorkspace)
            .WithName("Export people")
            .RequireAuthorization("Superuser");

        app.MapPost("/api/googleworkspace/people/sync/{id}", GoogleWorkspace.SyncPersonToGoogleWorkspace)
            .WithName("Sync person")
            .RequireAuthorization("Admin");

        app.MapPost("/api/googleworkspace/people/{id}/password", GoogleWorkspace.UpdatePasswordGoogleWorkspace)
            .WithName("Set password person")
            .RequireAuthorization("Admin");

        app.MapPost("/api/googleworkspace/people/{id}/ou", GoogleWorkspace.MoveOUGoogleWorkspace)
            .WithName("Update ou person")
            .RequireAuthorization("Admin");

        app.MapPost("/api/googleworkspace/events/{id}/sync", GoogleWorkspace.SyncEventToCalendar)
            .WithName("Sync event with calendar");
        // .RequireAuthorization("Admin");

        //Wifi
        app.MapGet("/api/wifi/export", Wifi.ExportWifiUsers)
            .WithName("Export wifi users")
            .RequireAuthorization("Superuser");


        // People
        app.MapGet("/api/people/export", People.ExportPeople)
            .RequireAuthorization("Admin")
            .WithName("Export people csv");

        app.MapGet("/api/people/{id}", People.GetPerson)
            .RequireAuthorization("Admin")
            .WithName("Get person by id");

        app.MapGet("/api/people", People.ListPeople)
            .RequireAuthorization("Admin")
            .WithName("List people by course");

        app.MapGet("/api/people/filter", People.FilterPeople)
            .RequireAuthorization("Admin")
            .WithName("Filter people");

        app.MapPost("/api/people", People.CreatePerson)
            .RequireAuthorization("Admin")
            .WithName("Create person");

        app.MapPut("/api/people/{id}", People.UpdatePerson)
            .RequireAuthorization("Admin")
            .WithName("Update person");

        app.MapDelete("/api/people/{id}", People.DeletePerson)
            .RequireAuthorization("Admin")
            .WithName("Delete person");

        app.MapGet("/api/people/{id}/payments", People.PersonPayments)
            .RequireAuthorization("Admin")
            .WithName("Person payments");

        // Courses
        app.MapGet("/api/courses/{id}", Courses.GetCourse)
            .RequireAuthorization("Admin")
            .WithName("Get course by id");

        app.MapGet("/api/courses", Courses.GetAllCourses)
            .RequireAuthorization("Admin")
            .WithName("Get all courses");

        app.MapGet("/api/courses/selector", Courses.GetCoursesSelector)
            .RequireAuthorization("Admin")
            .WithName("Get courses selector");

        app.MapPost("/api/courses", Courses.CreateCourse)
            .RequireAuthorization("Admin")
            .WithName("Create course");

        app.MapPut("/api/courses/{id}", Courses.UpdateCourse)
            .RequireAuthorization("Admin")
            .WithName("Update course");

        app.MapPut("/api/courses/{id}/active", Courses.SetActiveCourse)
            .RequireAuthorization("Admin")
            .WithName("Set active course");

        //OU Relations
        app.MapGet("/api/ougrouprelations", OuRelations.ListOuRelations)
            .RequireAuthorization("Superuser")
            .WithName("List ou relations");

        app.MapGet("/api/ougrouprelations/{id}", OuRelations.GetOuRelation)
            .RequireAuthorization("Superuser")
            .WithName("Get ou relation by id");

        app.MapPost("/api/ougrouprelations", OuRelations.CreateOuRelation)
            .RequireAuthorization("Superuser")
            .WithName("Create ou relation");

        app.MapPut("/api/ougrouprelations/{id}", OuRelations.UpdateOuRelation)
            .RequireAuthorization("Superuser")
            .WithName("Update ou relation");

        app.MapDelete("/api/ougrouprelations/{id}", OuRelations.DeleteOuRelation)
            .RequireAuthorization("Superuser")
            .WithName("Delete ou relation");

        // Groups
        app.MapGet("/api/groups/selector", Groups.GetGroupsSelector)
            .RequireAuthorization("Admin")
            .WithName("Get groups selector");

        app.MapGet("/api/groups", Groups.ListGroups)
            .RequireAuthorization("Admin")
            .WithName("List groups");

        app.MapGet("/api/groups/{id}", Groups.GetGroup)
            .RequireAuthorization("Admin")
            .WithName("Get group by id");

        app.MapPost("/api/groups", Groups.CreateGroup)
            .RequireAuthorization("Admin")
            .WithName("Create group");

        app.MapPut("/api/groups/{id}", Groups.UpdateGroup)
            .RequireAuthorization("Admin")
            .WithName("Update group");

        // Events
        app.MapGet("/api/events", Events.ListCourseEvents)
            .RequireAuthorization("Admin")
            .WithName("List current course events");

        app.MapGet("/api/events/{id}", Events.GetEvent)
            .RequireAuthorization("Admin")
            .WithName("Get event by id");

        app.MapPost("/api/events", Events.CreateEvent)
            .RequireAuthorization("Admin")
            .WithName("Create event");

        app.MapPut("/api/events/{id}", Events.UpdateEvent)
            .RequireAuthorization("Admin")
            .WithName("Update event");

        app.MapDelete("/api/events/{id}", Events.DeleteEvent)
            .RequireAuthorization("Admin")
            .WithName("Delete event");

        app.MapPost("/api/events/active", Events.ListActivePersonEvents)
            .WithName("Get active events by person document Id");

        app.MapGet("/api/events/export", Events.ExportEvents)
            .RequireAuthorization("Admin")
            .WithName("Export events info");

        // Events People
        app.MapPost("/api/events/{eventCode}/people", Events.SetPeopleToEvent)
            .RequireAuthorization("Admin")
            .WithName("Set people to event");

        app.MapGet("/api/events/{eventCode}/people", Events.GetPeopleEvent)
            .RequireAuthorization("Admin")
            .WithName("Get people in event");

        app.MapGet("/api/events/{eventCode}/payments", Events.ListEventPayments)
            .RequireAuthorization("Admin")
            .WithName("List event payments");

        app.MapPut("/api/events/{eventPersonId}/payment", Events.SetPersonEventPaid)
            .RequireAuthorization("Admin")
            .WithName("Set person event paid/not paid");

        app.MapGet("/api/events/{eventCode}/summary", Events.ListEventSummary)
            .RequireAuthorization("Reader")
            .WithName("List event summary");


        // Orders
        app.MapPost("/api/orders", Orders.CreateOrder)
            .WithName("Create order");

        app.MapPost("/api/orders/confirm", Orders.ConfirmOrderPost)
            .WithName("Confirm order post");

        app.MapGet("/api/order/info", Orders.GetOrderInfo)
            .WithName("Get order info");

        // Admin Info

        app.MapGet("/api/admin", AdminInfo.GetAdminInfo)
            .RequireAuthorization("Admin")
            .WithName("Get admin info");

        app.MapPut("/api/config", AdminInfo.UpdateAdminInfo)
            .RequireAuthorization("Admin")
            .WithName("Update app config");
    }
}
