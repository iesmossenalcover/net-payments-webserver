using Domain.People;

namespace Application.Common.Services;

public interface IPeopleService
{
    public Task<IEnumerable<Student>> GetManyStudentsAsync(IEnumerable<long> expidients, bool loadPeople, CancellationToken ct);
}