using WebServer.Handlers;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Auth services
builder.Services
    .AddAuthentication()
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o => {
        o.Cookie.Name = "SID";
        o.Cookie.HttpOnly = true; // Must be true for security reasons.

        // // If we want to send only a id and store data on a container like redis.
        // // usefull when we store many claims.
        // // by default session is stored as encrypted into the app
        // // o.SessionStore = ...

        // IMPORTANT: Disable redirect for js clients
        o.Events.OnRedirectToAccessDenied = o.Events.OnRedirectToLogin = c =>
        {
            c.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

// CORS service
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(p =>
    {
        // TODO check for production
        p.WithOrigins("http://localhost:3000", "http://localhost:3001")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// App Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IPasswordHasher<Domain.Authentication.User>, PasswordHasher<Domain.Authentication.User>>();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<Application.Services.ICurrentRequestService, WebServer.Services.CurrentRequestService>();

var app = builder.Build();

// Configure the HTTP middleware request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapRoutes();

app.Run();