namespace Application.Common.Models;

public class BatchUploadRow
{
    public long? Expedient { get; set; }
    public string Identitat { get; set; } = default!;
    public string Nom { get; set; } = default!;
    public string Llinatge1 { get; set; } = default!;
    public string? Llinatge2 { get; set; }
    public string? TelContacte { get; set; }
    public string? Grup { get; set; }
    public string? Assignatures { get; set; }
}

public class WifiAccountRow
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}

public class AccountRow
{
    public string First { get; set; } = string.Empty;
    public string Last { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Org { get; set; } = string.Empty;
    public string New { get; set; } = string.Empty;
    public string Recovery { get; set; } = string.Empty;
    public string Home { get; set; } = string.Empty;
    public string Work { get; set; } = string.Empty;
    public string RecoveryPhone { get; set; } = string.Empty;
    public string WorkPhone { get; set; } = string.Empty;
    public string HomePhone { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string WorkAddress { get; set; } = string.Empty;
    public string HomeAddress { get; set; } = string.Empty;
    public string Employee { get; set; } = string.Empty;
    public string EmployeeType { get; set; } = string.Empty;
    public string EmployeeTitle { get; set; } = string.Empty;
    public string Manager { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Cost { get; set; } = string.Empty;
    public string Building { get; set; } = string.Empty;
    public string Floor { get; set; } = string.Empty;
    public string FloorSection { get; set; } = string.Empty;
    public string Change { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string Advanced { get; set; } = string.Empty;
}

public class PersonRow
{
    public string Name { get; set; } = string.Empty;
    public long? AcademicRecordNumber { get; set; }
    public string DocumentId { get; set; } = string.Empty;
    public string Surname1 { get; set; } = string.Empty;
    public string? Surname2 { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public bool Amipla { get; set; }
    public bool Enrolled { get; set; }
}