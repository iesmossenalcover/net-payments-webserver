using System.Data;

namespace Application.Common.Services;

public interface ICsvParser
{
    CsvParserResult<T> Parse<T>(Stream stream);
}

public class CsvParserResult<T>
{
    public bool Ok { get; set; }
    public string? ErrorMessage { get; set; }
    public IList<T>? Values { get; set; }
}