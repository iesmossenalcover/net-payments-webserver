namespace Application.Common.Models;

public record FileVm(MemoryStream Stream, string FileType, string FileName);