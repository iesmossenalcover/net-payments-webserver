namespace Domain.Entities.Authentication;

public class GoogleGroupClaimRelation : Entity
{
    public int Proprity { get; set; }
    public string GroupEmail { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}