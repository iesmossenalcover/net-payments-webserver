using Domain.Entities.People;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repos;

public class PeopleRepository : Repository<Person>, Domain.Services.IPeopleRepository
{
    public PeopleRepository(AppDbContext dbContext) : base(dbContext, dbContext.People)
    {
    }

    public async Task<IEnumerable<Person>> FilterPeople(string q,
        IEnumerable<long> excludeIds,
        int maxResults,
        bool readOnly,
        CancellationToken ct)
    {
        return await DbSet(readOnly)
            .Where(x => !excludeIds.Contains(x.Id) && (
                EF.Functions.Like(x.DocumentId, $"%{q}%") ||
                EF.Functions.ILike(EF.Functions.Unaccent(x.Surname1), $"%{q}%") ||
                EF.Functions.ILike(EF.Functions.Unaccent(x.Name), $"%{q}%") ||
                (x.Surname2 != null && EF.Functions.ILike(EF.Functions.Unaccent(x.Surname2), $"%{q}%")) ||
                (x.AcademicRecordNumber.HasValue &&
                 EF.Functions.ILike(EF.Functions.Unaccent(x.AcademicRecordNumber.Value.ToString()), $"%{q}%")))
            )
            .Take(maxResults)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Person>> GetPeopleByDocumentIdsAsync(IEnumerable<string> documentIds,
        bool readOnly,
        CancellationToken ct)
    {
        documentIds = documentIds.Distinct().Select(x => x.ToUpperInvariant());
        return await DbSet(readOnly).Where(x => documentIds.Distinct().Contains(x.DocumentId)).ToListAsync(ct);
    }

    public async Task<Person?> GetPersonByDocumentIdAsync(string documentId, bool readOnly, CancellationToken ct)
    {
        return await DbSet(readOnly).FirstOrDefaultAsync(x => x.DocumentId == documentId.ToUpperInvariant(), ct);
    }

    public async Task<Person?> GetPersonByIdAsync(long id, bool readOnly, CancellationToken ct)
    {
        return await DbSet(readOnly).FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<Person?> GetPersonByAcademicRecordAsync(long academicRecord, bool readOnly, CancellationToken ct)
    {
        return await DbSet(readOnly).FirstOrDefaultAsync(x => x.AcademicRecordNumber == academicRecord, ct);
    }

    public async Task<IEnumerable<Person>> GetPeopleByAcademicRecordAsync(IEnumerable<long> academicRecords,
        bool readOnly,
        CancellationToken ct)
    {
        return await DbSet(readOnly)
            .Where(x => x.AcademicRecordNumber.HasValue &&
                        academicRecords.Distinct().Contains(x.AcademicRecordNumber.Value))
            .ToListAsync(ct);
    }

    public async Task<Person?> GetPersonByEmailAsync(string email, bool readOnly, CancellationToken ct)
    {
        return await DbSet(readOnly).FirstOrDefaultAsync(x => x.ContactMail == email, ct);
    }
}