using Domain.Entities.People;

namespace Domain.Services;

public interface IPeopleRepository : IRepository<Person>
{
    public Task<IEnumerable<Person>> GetPeopleByDocumentIdsAsync(IEnumerable<string> documentIds, CancellationToken ct);
    public Task<Person?> GetPersonByDocumentIdAsync(string documentId, CancellationToken ct);
    public Task<Person?> GetPersonByEmailAsync(string email, CancellationToken ct);
    public Task<Person?> GetPersonByAcademicRecordAsync(long academicRecord, CancellationToken ct);
    public Task<IEnumerable<Person>> GetPeopleByAcademicRecordAsync(IEnumerable<long> academicRecords, CancellationToken ct);
    public Task<IEnumerable<Person>> FilterPeople(string q, IEnumerable<long> excludeIds, int maxResults, CancellationToken ct);
}