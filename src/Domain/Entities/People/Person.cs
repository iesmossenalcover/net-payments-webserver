using Domain.Entities.Events;

namespace Domain.Entities.People;

public class Person : Entity
{
    public string DocumentId { get; set; } = default!;
    public string Name  { get; set; }  = default!;
    public string LastName  { get; set; }  = default!;
    public string? ContactPhone { get; set; }
    public string? ContactMail  { get; set; }
    public long? AcademicRecordNumber { get; set; }

    public bool IsStudent => AcademicRecordNumber.HasValue;
    public string FullName => $"{LastName}, {Name}";
}