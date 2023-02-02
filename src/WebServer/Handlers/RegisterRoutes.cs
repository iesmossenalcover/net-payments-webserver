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


        // Examples, remove in production
        app.MapGet("/api/items", WebServer.Handlers.Items.GetItems)
            .WithName("getitems")
            .WithOpenApi();

        app.MapPost("/api/items", WebServer.Handlers.Items.AddItem)
            .WithName("setitem")
            .WithOpenApi();
    }
}