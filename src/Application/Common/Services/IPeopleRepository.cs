using Domain.Entities.People;

namespace Application.Common.Services;

public interface IPeopleRepository : IRepository<Person>
{
    public Task<IEnumerable<Person>> GetPeopleByDocumentIdsAsync(IEnumerable<string> documentIds, CancellationToken ct);
    public Task<Person?> GetPersonByDocumentIdAsync(string documentId, CancellationToken ct);
    public Task<IEnumerable<PersonGroupCourse>> GetCurrentCourseGroupByPeopleIdsAsync(IEnumerable<long> peopleIds, CancellationToken ct);
}