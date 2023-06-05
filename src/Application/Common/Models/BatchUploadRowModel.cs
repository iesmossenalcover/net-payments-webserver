namespace Application.Common.Models
{
    public class BatchUploadRowModel
    {
        public long? Expedient { get; set; }
        public string Identitat { get; set; } = default!;
        public string Nom { get; set; } = default!;
        public string Llinatges { get; set; } = default!;
        public string? EmailContacte { get; set; }
        public string? TelContacte { get; set; }
        public string? Grup { get; set; }
        public string? Amipa { get; set; }
        public string? Assignatures { get; set; }
    }
}
