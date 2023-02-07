using System.Data;
using System.Globalization;
using Application.Common.Services;
using CsvHelper;

namespace Infrastructure;

public class CsvParser : ICsvParser
{
    public IList<T>? Parse<T>(Stream stream)
    {
        using (var reader = new StreamReader(stream))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            return csv.GetRecords<T>().ToList();
        }
    }
}