using Domain.Entities.Events;

namespace Domain.Entities.People;

public class Person : Entity
{
    public string DocumentId { get; set; } = default!;
    public string Name  { get; set; }  = default!;
    public string Surname1  { get; set; }  = default!;
    public string? Surname2  { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactMail  { get; set; }
    public long? AcademicRecordNumber { get; set; }
    public string? SubjectsInfo { get; set; } = default!;

    public bool IsStudent => AcademicRecordNumber.HasValue;
}