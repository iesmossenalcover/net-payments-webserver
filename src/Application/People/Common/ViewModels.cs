namespace Application.People.Common.ViewModels;

public record PersonVm
{
    public string Name { get; set; } = string.Empty;
    public string Surname1 { get; set; } = string.Empty;
    public string? Surname2 { get; set; }
    public string DocumentId { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? ContactMail { get; set; }
}

public record StudentVm
{
    public long AcademicRecordNumber { get; set; }
    public bool PreEnrollment { get; set; }
    public bool Amipa { get; set; }
    public string? SubjectsInfo { get; set; }
}

public record PersonGroupCourseVm(long Id, long CourseId, string CourseName, long GroupId, string GroupName);
public record GroupVm(long Id, string Name);
public record CourseVm(long Id, string Name);