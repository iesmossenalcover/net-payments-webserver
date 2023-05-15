namespace Domain.Entities.Authentication;

public class OAuthUser : Entity
{
    public string OAuthProviderCode { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public long UserId { get; set; }
    public User User { get; set; } = default!;
}