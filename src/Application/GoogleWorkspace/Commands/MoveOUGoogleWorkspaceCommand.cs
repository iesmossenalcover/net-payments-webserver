using Application.Common;
using Application.Common.Models;
using Domain.Services;
using Domain.Entities.People;
using MediatR;
using Domain.Entities.GoogleApi;

namespace Application.GoogleWorkspace.Commands;

// Model we receive
public record MoveOUGoogleWorkspaceCommand(long Id) : IRequest<Response<MoveOUGoogleWorkspaceCommandVm>>;

// Validator for the model

// Optionally define a view model
public record MoveOUGoogleWorkspaceCommandVm();

// Handler
public class MoveOUGoogleWorkspaceCommandHandler : IRequestHandler<MoveOUGoogleWorkspaceCommand, Response<MoveOUGoogleWorkspaceCommandVm>>
{
    #region props

    private readonly IGoogleAdminApi _googleAdminApi;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepository;
    private readonly IOUGroupRelationsRepository _oUGroupRelationsRepository;

    public MoveOUGoogleWorkspaceCommandHandler(IGoogleAdminApi googleAdminApi, IPersonGroupCourseRepository personGroupCourseRepository, IOUGroupRelationsRepository oUGroupRelationsRepository)
    {
        _googleAdminApi = googleAdminApi;
        _personGroupCourseRepository = personGroupCourseRepository;
        _oUGroupRelationsRepository = oUGroupRelationsRepository;
    }


    #endregion

    public async Task<Response<MoveOUGoogleWorkspaceCommandVm>> Handle(MoveOUGoogleWorkspaceCommand request, CancellationToken ct)
    {

        

        IEnumerable<PersonGroupCourse> personGroupCourses = await _personGroupCourseRepository.GetPersonGroupCoursesByPersonIdAsync(request.Id, ct);
        PersonGroupCourse? pgc = personGroupCourses.FirstOrDefault(x => x.Course.Active == true);
        
        if (pgc == null) return Response<MoveOUGoogleWorkspaceCommandVm>.Error(ResponseCode.NotFound, "Aquesta persona no està matriculada al curs actual");

        Person p = pgc.Person;

        UoGroupRelation? oug = await _oUGroupRelationsRepository.GetByGroupIdAsync(pgc.GroupId, CancellationToken.None);
        if (oug == null) return Response<MoveOUGoogleWorkspaceCommandVm>.Error(ResponseCode.NotFound, "No s'ha configurat la OU per aquest grup");

        if (string.IsNullOrEmpty(p.ContactMail)) return Response<MoveOUGoogleWorkspaceCommandVm>.Error(ResponseCode.BadRequest, "L'usuari no té correu.");

        GoogleApiResult<bool> result = await _googleAdminApi.MoveUserToOU(p.ContactMail, oug.ActiveOU);
        if (!result.Success) return Response<MoveOUGoogleWorkspaceCommandVm>.Error(ResponseCode.InternalError, result.ErrorMessage ?? "Error cridant api google.");

        result = await _googleAdminApi.AddUserToGroup(p.ContactMail, oug.GroupMail);
        if (!result.Success) return Response<MoveOUGoogleWorkspaceCommandVm>.Error(ResponseCode.InternalError, result.ErrorMessage ?? "Error cridant api google.");

        return Response<MoveOUGoogleWorkspaceCommandVm>.Ok(new MoveOUGoogleWorkspaceCommandVm());
    }
}