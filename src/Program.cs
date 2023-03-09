using WebServer.Handlers;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using MediatR;
using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Payments API", Version = "v1" });
});

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
});

// Auth services
builder.Services
    .AddAuthentication()
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o =>
    {
        o.Cookie.Name = "SID";
        o.Cookie.HttpOnly = true; // Must be true for security reasons.
        // TODO: Check in production
        o.Cookie.SameSite = SameSiteMode.None;
        o.Cookie.SecurePolicy = CookieSecurePolicy.Always;

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
        p.WithOrigins(
            builder.Configuration.GetValue<string>("CORSOriginUrl") ?? throw new Exception("Configure CORSOriginUrl"),
            "http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Add MediatR service
// The first method call will scan our Application 
/// assembly and add all our Commands, Queries, and their respective handlers to the DI container.
builder.Services.AddMediatR(Assembly.GetExecutingAssembly());
// Register validation pipeline for mediatR
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(Application.Common.Behaviors.RequestValidationBehavior<,>));
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// App Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<IPasswordHasher<Domain.Entities.Authentication.User>, PasswordHasher<Domain.Entities.Authentication.User>>();
builder.Services.AddScoped<Application.Common.Services.ICurrentRequestService, WebServer.Services.CurrentRequestService>();

var app = builder.Build();

// Configure the HTTP middleware request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api pagamanets IES Mossèn Alcover v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapRoutes();

app.Run();