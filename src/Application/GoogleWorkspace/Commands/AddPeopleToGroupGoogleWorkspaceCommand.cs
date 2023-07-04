using Application.Common;
using Application.Common.Models;
using Domain.Services;
using Domain.Entities.GoogleApi;
using Domain.Entities.People;
using MediatR;

namespace Application.GoogleWorkspace.Commands;

// Model we receive
public record AddPeopleToGroupGoogleWorkspaceCommand() : IRequest<Response<AddPeopleToGroupGoogleWorkspaceCommandVm>>;

// Validator for the model

// Optionally define a view model
public record AddPeopleToGroupGoogleWorkspaceCommandVm(bool ok);

// Handler
public class AddPeopleToGroupGoogleWorkspaceCommandHandler : IRequestHandler<AddPeopleToGroupGoogleWorkspaceCommand, Response<AddPeopleToGroupGoogleWorkspaceCommandVm>>
{
    #region props
    private readonly IOUGroupRelationsRepository _oUGroupRelationsRepository;
    private readonly IGoogleAdminApi _googleAdminApi;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepository;
    private readonly ICoursesRepository _courseRepository;


    private readonly string emailDomain;
    private readonly string[] excludeEmails;

    public AddPeopleToGroupGoogleWorkspaceCommandHandler(ICoursesRepository courseRepository, IPersonGroupCourseRepository personGroupCourseRepository, IOUGroupRelationsRepository oUGroupRelationsRepository, IGoogleAdminApi googleAdminApi, IConfiguration configuration)
    {
        _googleAdminApi = googleAdminApi;
        _oUGroupRelationsRepository = oUGroupRelationsRepository;
        _personGroupCourseRepository = personGroupCourseRepository;
        _courseRepository = courseRepository;

        emailDomain = configuration.GetValue<string>("GoogleApiDomain") ?? throw new Exception("GoogleApiDomain");
        excludeEmails = configuration.GetValue<string>("GoogleApiExcludeAccounts")?.Split(" ") ?? throw new Exception("GoogleApiExcludeAccounts");
    }
    #endregion

    public async Task<Response<AddPeopleToGroupGoogleWorkspaceCommandVm>> Handle(AddPeopleToGroupGoogleWorkspaceCommand request, CancellationToken ct)
    {

        Course course = await _courseRepository.GetCurrentCoursAsync(ct);
        IEnumerable<UoGroupRelation> ouRelations = await _oUGroupRelationsRepository.GetAllAsync(ct);

        if (ouRelations == null)
        {
            return Response<AddPeopleToGroupGoogleWorkspaceCommandVm>.Error(ResponseCode.InternalError, "Error extract ou_relation from BBDD");
        }

        foreach (var ou in ouRelations)
        {
            GoogleApiResult<bool> groupResult = await _googleAdminApi.ClearGroupMembers(ou.GroupMail);
            if (!groupResult.Success)
            {
                return Response<AddPeopleToGroupGoogleWorkspaceCommandVm>.Error(ResponseCode.InternalError, $"Error ClearGroupMembers from Group Mail: {ou.GroupMail}");
            }


            IEnumerable<PersonGroupCourse> pgcs = await _personGroupCourseRepository.GetPeopleGroupByGroupIdAndCourseIdAsync(course.Id, ou.GroupId, ct);

            foreach (var pgc in pgcs)
            {
                Person p = pgc.Person;
                // IMPORTANT: Exclude members
                if (excludeEmails.Contains(p.ContactMail)) continue;

                if (!string.IsNullOrEmpty(p.ContactMail))
                {
                    var result = await _googleAdminApi.AddUserToGroup(p.ContactMail, ou.GroupMail);
                    if (!result.Success)
                    {
                        return Response<AddPeopleToGroupGoogleWorkspaceCommandVm>.Error(ResponseCode.InternalError, $"Error function AddUserToGroup, Grup: {ou.GroupMail} USER: {p.ContactMail}");
                    }
                }
            }

        }

        return Response<AddPeopleToGroupGoogleWorkspaceCommandVm>.Ok(new AddPeopleToGroupGoogleWorkspaceCommandVm(true));
    }
}