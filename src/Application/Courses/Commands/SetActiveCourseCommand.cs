using Application.Common;
using Application.Common.Services;
using Domain.Entities.People;
using MediatR;

namespace Application.Courses.Commands;

public record SetActiveCourseCommand : IRequest<Response<Unit>>
{
    private long _Id;

    public long GetId => _Id;
    public void SetId(long value) { _Id = value; }
}

public class SetActiveCourseCommandHandler : IRequestHandler<SetActiveCourseCommand, Response<Unit>>
{

    #region  IOC
    private readonly ICoursesRepository _coursesRepository;

    public SetActiveCourseCommandHandler(ICoursesRepository coursesRepository)
    {
        _coursesRepository = coursesRepository;
    }
    #endregion

    public async Task<Response<Unit>> Handle(SetActiveCourseCommand request, CancellationToken ct)
    {

        Course currentCourse = await _coursesRepository.GetCurrentCoursAsync(ct);
        Course? newCurrentCourse = await _coursesRepository.GetByIdAsync(request.GetId, ct);
        if (newCurrentCourse == null) return Response<Unit>.Error(ResponseCode.NotFound, "El curs que es vol activar no existeix.");

        currentCourse.Active = false;
        newCurrentCourse.Active = true;

        await _coursesRepository.UpdateManyAsync(new Course[] { currentCourse, newCurrentCourse }, ct);

        return Response<Unit>.Ok(Unit.Value);
    }
}