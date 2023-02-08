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

    public Task<Course> GetCurrentCoursAsync(CancellationToken ct)
    {
        return _dbContext
            .Courses
            .FirstAsync(x => x.Active == true);
    }

    public async Task<IEnumerable<PersonGroupCourse>> GetCurrentCoursePersonGroupByPeopleIdsAsync(IEnumerable<long> peopleIds, CancellationToken ct)
    {
        return await _dbContext.PersonGroupCourses
                        .Include(x => x.Person)
                        .Include(x => x.Group)
                        .Include(x => x.Course)
                        .Where(x => x.Course.Active == true && peopleIds.Contains(x.PersonId)).ToListAsync();
    }

    public async Task<IEnumerable<Group>> GetGroupsByNameAsync(IEnumerable<string> names, CancellationToken ct)
    {
        return await _dbContext.Groups.Where(x => names.Contains(x.Name)).ToListAsync(ct);
    }

    public async Task<IEnumerable<Person>> GetPeopleAsync(IEnumerable<string> documents, CancellationToken ct)
    {
        return await _dbContext.People.Where(x => documents.Contains(x.DocumentId)).ToListAsync(ct);
    }

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

    public async Task InsertAndUpdateTransactionAsync(
        IEnumerable<Group> groups,
        IEnumerable<Person> people,
        IEnumerable<Student> students,
        IEnumerable<PersonGroupCourse> personGroupCourses,
        CancellationToken ct)
    {
        _dbContext.Groups.AddRange(groups.Where(x => x.Id == 0));
        _dbContext.Groups.UpdateRange(groups.Where(x => x.Id > 0));
        _dbContext.People.AddRange(people.Where(x => x.Id == 0));
        _dbContext.People.UpdateRange(people.Where(x => x.Id >= 0));
        _dbContext.PersonGroupCourses.AddRange(personGroupCourses.Where(x => x.Id == 0));
        _dbContext.PersonGroupCourses.UpdateRange(personGroupCourses.Where(x => x.Id > 0));
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Student> InsertStudentAsync(Student student)
    {
        _dbContext.Students.Add(student);
        await _dbContext.SaveChangesAsync();
        return student;
    }
}