using System.Data;
using System.Globalization;
using Application.Common.Services;
using CsvHelper;

namespace Infrastructure;

public class CsvParser : ICsvParser
{
    public DataTable? Parse(Stream stream, IDictionary<string, Type> columns)
    {
        using (var reader = new StreamReader(stream))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            // Do any configuration to `CsvReader` before creating CsvDataReader.
            using (var dr = new CsvDataReader(csv))
            {
                var dt = new DataTable();                
                foreach (var c in columns)
                {
                    dt.Columns.Add(c.Key, c.Value);
                }
                dt.Load(dr);
                return dt;
            }
        }
    }
}