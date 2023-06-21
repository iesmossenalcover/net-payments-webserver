using Application.Common.Models;
using System.Data;

namespace Application.Common.Services;

public interface ICsvParser
{
    CsvParseResult<T> Parse(Stream stream);
    Task WriteManyToFileAsync<T>(string path, IEnumerable<T> records, bool overrite);
    Task WriteToFileAsync<T>(string path, T record, bool overrite);
    Task WriteHeadersAsync<T>(string path);
}

public class CsvParseResult<T>
{
    public bool Ok { get; set; }
    public string? ErrorMessage { get; set; }
    public IList<T>? Values { get; set; }
}