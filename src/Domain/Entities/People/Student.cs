namespace Domain.Entities.People;

public class Student
{
    public long Id { get; set; }
    public long PersonId { get; set; }
    public Person Person { get; set; } = default!;
    public long AcademicRecordNumber { get; set; }
    public string? SubjectsInfo { get; set; } = default!;
    public bool Amipa { get; set; } = false;
    public bool PreEnrollment { get; set; } = false;
}