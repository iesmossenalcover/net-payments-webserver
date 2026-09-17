namespace WebServer.Handlers.Authentication;

// Noms de les polítiques d'autorització. Es registren a Program.cs.
public static class AuthorizationPolicies
{
    public const string SUPER_USER = "Superuser";
    public const string ADVANCED_ADMIN = "AdvancedAdmin";
    public const string ADMIN = "Admin";
    public const string READER = "Reader";
}
