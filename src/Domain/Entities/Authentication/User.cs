namespace Domain.Entities.Authentication;

public class User
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string HashedPassword { get; set; } = string.Empty;
    public string Firstname { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
    public virtual IList<UserClaim> UserClaims { get; set; } = new List<UserClaim>();
}