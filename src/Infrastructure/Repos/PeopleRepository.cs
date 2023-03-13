using Domain.Entities.People;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repos;

public class PeopleRepository : Repository<Person>, Application.Common.Services.IPeopleRepository
{
    public PeopleRepository(AppDbContext dbContext) : base(dbContext, dbContext.People) {}

    public async Task<IEnumerable<Person>> GetPeopleByDocumentIdsAsync(IEnumerable<string> documentIds, CancellationToken ct)
    {
        documentIds = documentIds.Distinct().Select(x => x.ToUpperInvariant());
        return await _dbSet.Where(x => documentIds.Distinct().Contains(x.DocumentId)).ToListAsync(ct);
    }

    public async Task<Person?> GetPersonByDocumentIdAsync(string documentId, CancellationToken ct)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.DocumentId == documentId.ToUpperInvariant(), ct);
    }

    public async Task<Person?> GetPersonByIdAsync(long id)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Person?> GetPersonByAcademicRecordAsync(long academicRecord, CancellationToken ct)
    {
        return await _dbSet
            .FirstOrDefaultAsync(x => x.AcademicRecordNumber == academicRecord, ct);
    }

    public async Task<IEnumerable<Person>> GetPeopleByAcademicRecordAsync(IEnumerable<long> academicRecords, CancellationToken ct)
    {
        return await _dbSet
            .Where(x => x.AcademicRecordNumber.HasValue && academicRecords.Distinct().Contains(x.AcademicRecordNumber.Value))
            .ToListAsync(ct);
    }
}