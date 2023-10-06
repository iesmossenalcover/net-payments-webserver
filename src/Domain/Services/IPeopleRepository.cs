using Domain.Entities.People;

namespace Domain.Services;

public interface IPeopleRepository : IRepository<Person>
{
    public Task<IEnumerable<Person>> GetPeopleByDocumentIdsAsync(IEnumerable<string> documentIds, bool readOnly,
        CancellationToken ct);

    public Task<Person?> GetPersonByDocumentIdAsync(string documentId, bool readOnly, CancellationToken ct);
    public Task<Person?> GetPersonByEmailAsync(string email, bool readOnly, CancellationToken ct);
    public Task<Person?> GetPersonByAcademicRecordAsync(long academicRecord, bool readOnly, CancellationToken ct);

    public Task<IEnumerable<Person>> GetPeopleByAcademicRecordAsync(IEnumerable<long> academicRecords, bool readOnly,
        CancellationToken ct);

    public Task<IEnumerable<Person>> FilterPeople(string q, IEnumerable<long> excludeIds, int maxResults, bool readOnly,
        CancellationToken ct);
}