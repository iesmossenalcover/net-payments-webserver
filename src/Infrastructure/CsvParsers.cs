using System.Data;
using System.Globalization;
using Application.Common.Services;
using CsvHelper;

namespace Infrastructure;

public class CsvParser : ICsvParser
{
    public CsvParserResult<T> Parse<T>(Stream stream)
    {
        var result = new CsvParserResult<T>();
		try
		{
            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                result.Values = csv.GetRecords<T>().ToList();
                result.Ok = true;
            }
        }
		catch (CsvHelperException e)
		{
            result.Ok = false;
            result.ErrorMessage = e.Message;
		}
        return result;
    }
}