using System.Data;

namespace Application.Common.Services;

public interface ICsvParser
{
    DataTable? Parse(Stream stream, IDictionary<string, Type> columns);
}