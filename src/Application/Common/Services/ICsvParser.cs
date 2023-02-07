using System.Data;

namespace Application.Common.Services;

public interface ICsvParser
{
    IList<T>? Parse<T>(Stream stream);
}