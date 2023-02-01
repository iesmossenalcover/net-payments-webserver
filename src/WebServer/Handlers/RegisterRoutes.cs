namespace WebServer.Handlers
{
    public static class RegisterRoutes
    {
        public static void MapRoutes(this WebApplication app)
        {
            app.MapPost("/api/signin", Authentication.Signin.Post).WithName("signin").WithOpenApi();
            app.MapPost("/api/signup", Authentication.Signup.Post).WithName("signup").WithOpenApi();
            app.MapGet("/api/identity", Authentication.Identity.Get)
                .RequireAuthorization()
                .WithName("identity")
                .WithOpenApi();
        }
    }
}