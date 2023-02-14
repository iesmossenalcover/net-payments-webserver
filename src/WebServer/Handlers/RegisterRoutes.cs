namespace WebServer.Handlers;
public static class RegisterRoutes
{
    public static void MapRoutes(this WebApplication app)
    {
        // auth related actions are in WebServer layer dueto related session storage.
        app.MapPost("/api/signin", Authentication.Signin.Post)
            .WithName("signin")
            .WithOpenApi();

        app.MapPost("/api/signup", Authentication.Signup.Post)
            .WithName("signup")
            .WithOpenApi();

        app.MapGet("/api/identity", Authentication.Identity.Get)
            .RequireAuthorization()
            .WithName("identity")
            .WithOpenApi();

        // Tasks
        app.MapPost("/api/tasks/people", WebServer.Handlers.Tasks.UploadPeople)
            .WithName("Upload people")
            .WithOpenApi();

        // People
        app.MapGet("/api/people/{id}", WebServer.Handlers.People.GetPerson)
            .WithName("Get person by id")
            .WithOpenApi();
            
        app.MapGet("/api/people", WebServer.Handlers.People.ListPeople)
            .WithName("List people by group")
            .WithOpenApi();

        app.MapPost("/api/people", WebServer.Handlers.People.CreatePerson)
            .WithName("Create person")
            .WithOpenApi();

        app.MapPut("/api/people", WebServer.Handlers.People.UpdatePerson)
            .WithName("Update person")
            .WithOpenApi();
    }
}