using Application.Common.Models;
using System.Data;

namespace Application.Common.Services;

public interface ICsvParser
{
    CsvParserResult<BatchUploadRowModel> ParseBatchUpload(Stream stream);
}

public class CsvParserResult<T>
{
    public bool Ok { get; set; }
    public string? ErrorMessage { get; set; }
    public IList<T>? Values { get; set; }
}