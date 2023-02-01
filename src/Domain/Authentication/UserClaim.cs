namespace Domain.Authentication;

public class UserClaim
{
    public long Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public long UserId { get; set; }
    public virtual User User { get; set; } = default!;
}