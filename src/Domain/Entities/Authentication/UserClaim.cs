namespace Domain.Entities.Authentication;

public class RoleClaimValues
{
    public const string ADMIN = "admin";
    public const string READER = "reader";
}

public class UserClaim : Entity
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public long UserId { get; set; }
    public virtual User User { get; set; } = default!;
}