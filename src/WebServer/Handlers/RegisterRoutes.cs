using WebServer.Handlers.Authentication;

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

        var signup = app.MapPost("/api/signup", global::WebServer.Handlers.Authentication.Auth.SignupPost)
            .WithName("signup");
        if (!app.Environment.IsDevelopment())
        {
            signup.RequireAuthorization(AuthorizationPolicies.ADMIN);
        }

        // Jobs
        app.MapGet("/api/processes", Processes.GetProcessess)
            .WithName("Get Last Process")
            .RequireAuthorization(AuthorizationPolicies.SUPER_USER);

        app.MapGet("/api/processes/logs/{id}", Processes.GetLog)
            .WithName("Get Log")
            .RequireAuthorization(AuthorizationPolicies.SUPER_USER);

        app.MapPost("/api/processes", Processes.StartProcess)
            .WithName("Start process")
            .RequireAuthorization(AuthorizationPolicies.SUPER_USER);

        // Tasks
        app.MapGet("/api/tasks/people", Tasks.GetPeopleBatchUploadTemplate)
            .RequireAuthorization(AuthorizationPolicies.ADMIN)
            .WithName("Get people batch upload template");

        app.MapPost("/api/tasks/people", Tasks.PeopleBatchUpload)
            .RequireAuthorization(AuthorizationPolicies.ADMIN)
            .WithName("Upload people");

        app.MapGet("/api/googleworkspace/people/export", GoogleWorkspace.ExportPeopleGoogleWorkspace)
            .WithName("Export people")
            .RequireAuthorization(AuthorizationPolicies.SUPER_USER);

        app.MapPost("/api/googleworkspace/people/sync/{id}", GoogleWorkspace.SyncPersonToGoogleWorkspace)
            .WithName("Sync person")
            .RequireAuthorization(AuthorizationPolicies.ADMIN);

        app.MapPost("/api/googleworkspace/people/{id}/password", GoogleWorkspace.UpdatePasswordGoogleWorkspace)
            .WithName("Set password person")
            .RequireAuthorization(AuthorizationPolicies.ADMIN);

        app.MapPost("/api/googleworkspace/people/{id}/ou", GoogleWorkspace.MoveOUGoogleWorkspace)
            .WithName("Update ou person")
            .RequireAuthorization(AuthorizationPolicies.ADMIN);

        app.MapPost("/api/googleworkspace/events/{id}/sync", GoogleWorkspace.SyncEventToCalendar)
            .WithName("Sync event with calendar")
            .RequireAuthorization(AuthorizationPolicies.ADVANCED_ADMIN);

        app.MapDelete("/api/googleworkspace/events/{id}", GoogleWorkspace.RemoveEventFromCalendar)
            .WithName("Remove calendar event from google calendar")
            .RequireAuthorization(AuthorizationPolicies.ADVANCED_ADMIN);

        //Wifi
        app.MapGet("/api/wifi/export", Wifi.ExportWifiUsers)
            .WithName("Export wifi users")
            .RequireAuthorization(AuthorizationPolicies.SUPER_USER);


        // People
        app.MapGet("/api/people/export", People.ExportPeople)
            .RequireAuthorization(AuthorizationPolicies.ADMIN)
            .WithName("Export people csv");

        app.MapGet("/api/people/{id}", People.GetPerson)
            .RequireAuthorization(AuthorizationPolicies.ADMIN)
            .WithName("Get person by id");

        app.MapGet("/api/people", People.ListPeople)
            .RequireAuthorization(AuthorizationPolicies.ADMIN)
            .WithName("List people by course");

        app.MapGet("/api/people/filter", People.FilterPeople)
            .RequireAuthorization(AuthorizationPolicies.ADMIN)
            .WithName("Filter people");

        app.MapPost("/api/people", People.CreatePerson)
            .RequireAuthorization(AuthorizationPolicies.ADMIN)
            .WithName("Create person");

        app.MapPut("/api/people/{id}", People.UpdatePerson)
            .RequireAuthorization(AuthorizationPolicies.ADMIN)
            .WithName("Update person");

        app.MapDelete("/api/people/{id}", People.DeletePerson)
            .RequireAuthorization(AuthorizationPolicies.ADMIN)
            .WithName("Delete person");

        app.MapGet("/api/people/{id}/payments", People.PersonPayments)
            .RequireAuthorization(AuthorizationPolicies.ADMIN)
            .WithName("Person payments");

        // Courses
        app.MapGet("/api/courses/{id}", Courses.GetCourse)
            .RequireAuthorization(AuthorizationPolicies.SUPER_USER)
            .WithName("Get course by id");

        app.MapGet("/api/courses", Courses.GetAllCourses)
            .RequireAuthorization(AuthorizationPolicies.SUPER_USER)
            .WithName("Get all courses");

        app.MapGet("/api/courses/selector", Courses.GetCoursesSelector)
            .RequireAuthorization(AuthorizationPolicies.SUPER_USER)
            .WithName("Get courses selector");

        app.MapPost("/api/courses", Courses.CreateCourse)
            .RequireAuthorization(AuthorizationPolicies.SUPER_USER)
            .WithName("Create course");

        app.MapPut("/api/courses/{id}", Courses.UpdateCourse)
            .RequireAuthorization(AuthorizationPolicies.SUPER_USER)
            .WithName("Update course");

        app.MapPut("/api/courses/{id}/active", Courses.SetActiveCourse)
            .RequireAuthorization(AuthorizationPolicies.SUPER_USER)
            .WithName("Set active course");

        //OU Relations
        app.MapGet("/api/ougrouprelations", OuRelations.ListOuRelations)
            .RequireAuthorization(AuthorizationPolicies.SUPER_USER)
            .WithName("List ou relations");

        app.MapGet("/api/ougrouprelations/{id}", OuRelations.GetOuRelation)
            .RequireAuthorization(AuthorizationPolicies.SUPER_USER)
            .WithName("Get ou relation by id");

        app.MapPost("/api/ougrouprelations", OuRelations.CreateOuRelation)
            .RequireAuthorization(AuthorizationPolicies.SUPER_USER)
            .WithName("Create ou relation");

        app.MapPut("/api/ougrouprelations/{id}", OuRelations.UpdateOuRelation)
            .RequireAuthorization(AuthorizationPolicies.SUPER_USER)
            .WithName("Update ou relation");

        app.MapDelete("/api/ougrouprelations/{id}", OuRelations.DeleteOuRelation)
            .RequireAuthorization(AuthorizationPolicies.SUPER_USER)
            .WithName("Delete ou relation");

        // Groups
        app.MapGet("/api/groups/selector", Groups.GetGroupsSelector)
            .RequireAuthorization(AuthorizationPolicies.ADMIN)
            .WithName("Get groups selector");

        app.MapGet("/api/groups", Groups.ListGroups)
            .RequireAuthorization(AuthorizationPolicies.ADMIN)
            .WithName("List groups");

        app.MapGet("/api/groups/{id}", Groups.GetGroup)
            .RequireAuthorization(AuthorizationPolicies.ADMIN)
            .WithName("Get group by id");

        app.MapPost("/api/groups", Groups.CreateGroup)
            .RequireAuthorization(AuthorizationPolicies.ADMIN)
            .WithName("Create group");

        app.MapPut("/api/groups/{id}", Groups.UpdateGroup)
            .RequireAuthorization(AuthorizationPolicies.ADMIN)
            .WithName("Update group");

        // Events
        app.MapGet("/api/events", Events.ListCourseEvents)
            .RequireAuthorization(AuthorizationPolicies.ADMIN)
            .WithName("List current course events");

        app.MapGet("/api/events/{id}", Events.GetEvent)
            .RequireAuthorization(AuthorizationPolicies.ADMIN)
            .WithName("Get event by id");

        app.MapPost("/api/events", Events.CreateEvent)
            .RequireAuthorization(AuthorizationPolicies.ADMIN)
            .WithName("Create event");

        app.MapPut("/api/events/{id}", Events.UpdateEvent)
            .RequireAuthorization(AuthorizationPolicies.ADMIN)
            .WithName("Update event");

        app.MapDelete("/api/events/{id}", Events.DeleteEvent)
            .RequireAuthorization(AuthorizationPolicies.ADMIN)
            .WithName("Delete event");

        app.MapPost("/api/events/active", Events.ListActivePersonEvents)
            .WithName("Get active events by person document Id");

        app.MapGet("/api/events/export", Events.ExportEvents)
            .RequireAuthorization(AuthorizationPolicies.ADMIN)
            .WithName("Export events info");

        // Events People
        app.MapPost("/api/events/{eventCode}/people", Events.SetPeopleToEvent)
            .RequireAuthorization(AuthorizationPolicies.ADMIN)
            .WithName("Set people to event");

        app.MapGet("/api/events/{eventCode}/people", Events.GetPeopleEvent)
            .RequireAuthorization(AuthorizationPolicies.ADMIN)
            .WithName("Get people in event");

        app.MapGet("/api/events/{eventCode}/payments", Events.ListEventPayments)
            .RequireAuthorization(AuthorizationPolicies.ADMIN)
            .WithName("List event payments");

        app.MapPut("/api/events/{eventPersonId}/payment", Events.SetPersonEventPaid)
            .RequireAuthorization(AuthorizationPolicies.ADMIN)
            .WithName("Set person event paid/not paid");

        app.MapGet("/api/events/{eventCode}/summary", Events.ListEventSummary)
            .RequireAuthorization(AuthorizationPolicies.READER)
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
            .RequireAuthorization(AuthorizationPolicies.ADMIN)
            .WithName("Get admin info");

        app.MapPut("/api/config", AdminInfo.UpdateAdminInfo)
            .RequireAuthorization(AuthorizationPolicies.ADMIN)
            .WithName("Update app config");
    }
}
