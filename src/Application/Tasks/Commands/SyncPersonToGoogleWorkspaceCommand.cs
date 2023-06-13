using System.Data;
using Application.Common;
using Application.Common.Models;
using Application.Common.Services;
using Domain.Entities.GoogleApi;
using Domain.Entities.People;
using FluentValidation;
using MediatR;

namespace Application.Tasks.Commands;

// Model we receive
public record SyncPersonToGoogleWorkspaceCommand(long Id) : IRequest<Response<SyncPersonToGoogleWorkspaceCommandVm>>;

// Validator for the model

// Optionally define a view model
public record SyncPersonToGoogleWorkspaceCommandVm(string Email, string Password);

// Handler
public class SyncPersonToGoogleWorkspaceCommandHandler : IRequestHandler<SyncPersonToGoogleWorkspaceCommand, Response<SyncPersonToGoogleWorkspaceCommandVm>>
{
    #region props

    private readonly IGoogleAdminApi _googleAdminApi;
    private readonly ICoursesRepository _courseRepository;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepository;
    private readonly IPeopleRepository _peopleRepository;
    private readonly IOUGroupRelationsRepository _oUGroupRelationsRepository;
    private readonly string emailDomain;

    public SyncPersonToGoogleWorkspaceCommandHandler(IOUGroupRelationsRepository oUGroupRelationsRepository, IGoogleAdminApi googleAdminApi, ICoursesRepository courseRepository, IPersonGroupCourseRepository personGroupCourseRepository, IPeopleRepository peopleRepository, IConfiguration configuration)
    {
        _googleAdminApi = googleAdminApi;
        _courseRepository = courseRepository;
        _personGroupCourseRepository = personGroupCourseRepository;
        _peopleRepository = peopleRepository;
        _oUGroupRelationsRepository = oUGroupRelationsRepository;
        emailDomain = configuration.GetValue<string>("GoogleApiDomain") ?? throw new Exception("GoogleApiDomain");
    }
    #endregion

    public async Task<Response<SyncPersonToGoogleWorkspaceCommandVm>> Handle(SyncPersonToGoogleWorkspaceCommand request, CancellationToken ct)
    {

        // GoogleApiResult<bool> getUsersResult = await _googleAdminApi.DeleteUserInGroup("test1234@iesmossenalcover.cat", "payment.superuser@iesmossenalcover.cat");

        // if (!getUsersResult.Success)
        // {
        //     return Response<SyncPersonToGoogleWorkspaceCommandVm>.Error(ResponseCode.BadRequest, getUsersResult.ErrorMessage ?? "");

        // }
        // return Response<SyncPersonToGoogleWorkspaceCommandVm>.Ok(new SyncPersonToGoogleWorkspaceCommandVm());

        IEnumerable<PersonGroupCourse> personGroupCourses = await _personGroupCourseRepository.GetPersonGroupCoursesByPersonIdAsync(request.Id, ct);
        PersonGroupCourse? pgc = personGroupCourses.FirstOrDefault(x => x.Course.Active == true);
        if (pgc == null) return Response<SyncPersonToGoogleWorkspaceCommandVm>.Error(ResponseCode.NotFound, "Aquesta persona no està matriculada al curs actual");

        Person p = pgc.Person;

        UoGroupRelation? oug = await _oUGroupRelationsRepository.GetByGroupIdAsync(pgc.GroupId, CancellationToken.None);
        if (oug == null) return Response<SyncPersonToGoogleWorkspaceCommandVm>.Error(ResponseCode.NotFound, "No s'ha configurat la OU per aquest grup");
        string password = "****";

        bool createUser = string.IsNullOrEmpty(p.ContactMail);
        if (!string.IsNullOrEmpty(p.ContactMail))
        {
            GoogleApiResult<bool> userExists = await _googleAdminApi.UserExists(p.ContactMail);
            if (!userExists.Success) return Response<SyncPersonToGoogleWorkspaceCommandVm>.Error(ResponseCode.BadRequest, userExists.ErrorMessage ?? "Error recuperant l'usuari");
            createUser = !userExists.Data;
        }

        if (createUser)
        {
            password = Common.Helpers.GenerateString.RandomAlphanumeric(8);
            p.ContactMail = $"{Common.Helpers.Email.NormalizeText($"{p.Name}{p.Surname1}{p.AcademicRecordNumber}")}@{emailDomain}".ToLower();

            GoogleApiResult<bool> createUsersResult = await _googleAdminApi.CreateUser(p.ContactMail, p.Name, p.LastName, password, oug.ActiveOU);
            if (!createUsersResult.Success) return Response<SyncPersonToGoogleWorkspaceCommandVm>.Error(ResponseCode.BadRequest, createUsersResult.ErrorMessage ?? "Error al crear l'usuari");

            createUsersResult = await _googleAdminApi.AddUserToGroup(p.ContactMail, oug.GroupMail);
            if (!createUsersResult.Success) return Response<SyncPersonToGoogleWorkspaceCommandVm>.Error(ResponseCode.BadRequest, createUsersResult.ErrorMessage ?? "Error a l'assignar l'usuari al grup");

            await _peopleRepository.UpdateAsync(p, ct);
        }
        else if(!string.IsNullOrEmpty(p.ContactMail))
        {
            GoogleApiResult<bool> moveUsersResult = await _googleAdminApi.MoveUserToOU(p.ContactMail, oug.ActiveOU);
        }

        if (!string.IsNullOrEmpty(p.ContactMail))
        {
            return Response<SyncPersonToGoogleWorkspaceCommandVm>.Ok(new SyncPersonToGoogleWorkspaceCommandVm(p.ContactMail, password));
        }
        else
        {
            return Response<SyncPersonToGoogleWorkspaceCommandVm>.Error(ResponseCode.InternalError, "No s'ha pogut creal l'email");
        }

    }



}