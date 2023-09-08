namespace Domain.Entities.GoogleApi;

using Domain;
using Domain.Entities.People;

public class UoGroupRelation : Entity
{
    public long GroupId { get; set; }
    public Group Group { get; set; } = default!;

    public string GroupMail { get; set; } = default!;

    public string OldOU { get; set; } = default!;
    public string ActiveOU { get; set; } = default!;

    public bool UpdatePassword {get; set; } = false;
    public bool ChangePasswordNextSignIn { get; set; } = true;
}