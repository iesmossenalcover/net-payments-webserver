using Application.Common.Services;
using Domain.Entities.People;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class PeopleService : IPeopleService
{
    #region properties

        private readonly ApplicationDbContext _dbContext;

        public PeopleService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        #endregion

    public async Task<IEnumerable<Student>> GetManyStudentsAsync(IEnumerable<long> expidients, bool loadPeople, CancellationToken ct)
    {
        var query = _dbContext
                    .Students
                    .Where(x => expidients.Contains(x.AcademicRecordNumber));

        if (loadPeople)
        {
            query = query.Include(x => x.Person);
        }

        return await query.ToListAsync();
    }

    public async Task InsertOrUpdateManyStudentsAsync(IEnumerable<Student> students, CancellationToken ct)
    {
        _dbContext.Students.UpdateRange(students.Where(x => x.Id > 0));
        _dbContext.Students.AddRange(students.Where(x => x.Id == 0));
        await _dbContext.SaveChangesAsync();
    }
}