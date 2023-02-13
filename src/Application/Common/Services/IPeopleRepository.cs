using Domain.Entities.People;

namespace Application.Common.Services;

public interface IPeopleRepository : IRepository<Person>
{
    public Task<IEnumerable<Person>> GetPeopleByDocumentIdsAsync(IEnumerable<string> documentIds, CancellationToken ct);
    public Task<IEnumerable<Person>> GetPeopleByCourseAsync(long courseId, int position, int? max, CancellationToken ct);
    public Task<Person?> GetPersonByDocumentIdAsync(string documentId, CancellationToken ct);
}