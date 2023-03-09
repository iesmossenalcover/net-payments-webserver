namespace Domain.Entities.People;

public class Student : Person
{
    public long AcademicRecordNumber { get; set; }
    public string? SubjectsInfo { get; set; } = default!;
}