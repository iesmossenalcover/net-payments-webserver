namespace Application.Common.Models
{
    public class BatchUploadRowModel
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
}
