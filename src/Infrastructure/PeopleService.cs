using Application.Common.Services;
using Domain.Entities.People;

namespace Infrastructure;

public class PeopleService : IPeopleService
{
    public Task<IEnumerable<Student>> GetManyStudentsAsync(IEnumerable<long> expidients, bool loadPeople, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}