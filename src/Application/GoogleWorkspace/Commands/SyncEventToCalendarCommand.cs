using Application.Common;
using Application.Common.Models;
using Domain.Services;
using Domain.Entities.GoogleApi;
using Domain.Entities.People;
using MediatR;

namespace Application.GoogleWorkspace.Commands;

// Model we receive
public record SyncEventToCalendarCommand(long Id) : IRequest<Response<SyncEventToCalendarCommandVm>>;

// Validator for the model

// Optionally define a view model
public record SyncEventToCalendarCommandVm(string Email, string? Password);

// Handler
public class SyncEventToCalendarCommandHandler : IRequestHandler<SyncEventToCalendarCommand, Response<SyncEventToCalendarCommandVm>>
{
    #region props

    private readonly IGoogleAdminApi _googleAdminApi;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepository;
    private readonly IPeopleRepository _peopleRepository;
    private readonly IOUGroupRelationsRepository _oUGroupRelationsRepository;
    private readonly string emailDomain;

    public SyncEventToCalendarCommandHandler(IOUGroupRelationsRepository oUGroupRelationsRepository, IGoogleAdminApi googleAdminApi, IPersonGroupCourseRepository personGroupCourseRepository, IPeopleRepository peopleRepository, IConfiguration configuration)
    {
        _googleAdminApi = googleAdminApi;
        _personGroupCourseRepository = personGroupCourseRepository;
        _peopleRepository = peopleRepository;
        _oUGroupRelationsRepository = oUGroupRelationsRepository;
        emailDomain = configuration.GetValue<string>("GoogleApiDomain") ?? throw new Exception("GoogleApiDomain");
    }
    #endregion

    public async Task<Response<SyncEventToCalendarCommandVm>> Handle(SyncEventToCalendarCommand request, CancellationToken ct)
    {
        IEnumerable<PersonGroupCourse> personGroupCourses = await _personGroupCourseRepository.GetPersonGroupCoursesByPersonIdAsync(request.Id, ct);
        PersonGroupCourse? pgc = personGroupCourses.FirstOrDefault(x => x.Course.Active == true);
        if (pgc == null) return Response<SyncEventToCalendarCommandVm>.Error(ResponseCode.NotFound, "Aquesta persona no està matriculada al curs actual");

        Person p = pgc.Person;

        OuGroupRelation? oug = await _oUGroupRelationsRepository.GetByGroupIdAsync(pgc.GroupId, CancellationToken.None);
        if (oug == null) return Response<SyncEventToCalendarCommandVm>.Error(ResponseCode.NotFound, "No s'ha configurat la OU per aquest grup");
        string? password = null;

        bool createUser = string.IsNullOrEmpty(p.ContactMail);
        if (!string.IsNullOrEmpty(p.ContactMail))
        {
            GoogleApiResult<bool> userExists = await _googleAdminApi.UserExists(p.ContactMail);
            if (!userExists.Success) return Response<SyncEventToCalendarCommandVm>.Error(ResponseCode.BadRequest, userExists.ErrorMessage ?? "Error recuperant l'usuari");
            if (userExists.Data) return Response<SyncEventToCalendarCommandVm>.Error(ResponseCode.BadRequest, "Ja existeix un compte amb aquest correu");
            createUser = !userExists.Data;
        }

        if (createUser)
        {
            password = Common.Helpers.GenerateString.RandomAlphanumeric(8);
            p.ContactMail = string.IsNullOrEmpty(p.ContactMail) ? GetEmail(p, emailDomain) : p.ContactMail;
            GoogleApiResult<bool> createUsersResult = await _googleAdminApi.CreateUser(p.ContactMail, p.Name.ToLower(), p.LastName.ToLower(), password, oug.ActiveOU);
            if (!createUsersResult.Success) return Response<SyncEventToCalendarCommandVm>.Error(ResponseCode.BadRequest, createUsersResult.ErrorMessage ?? "Error al crear l'usuari");

            //On create user to google api, need time to execute the creation on the google site.
            await Task.Delay(2000);

            createUsersResult = await _googleAdminApi.AddUserToGroup(p.ContactMail, oug.GroupMail);
            if (!createUsersResult.Success) return Response<SyncEventToCalendarCommandVm>.Error(ResponseCode.BadRequest, createUsersResult.ErrorMessage ?? "Error a l'assignar l'usuari al grup");

            await _peopleRepository.UpdateAsync(p, ct);
        }
        else if (!string.IsNullOrEmpty(p.ContactMail)) // If user has an email
        {
            GoogleApiResult<bool> moveUsersResult = await _googleAdminApi.MoveUserToOU(p.ContactMail, oug.ActiveOU);
            if (!moveUsersResult.Success) return Response<SyncEventToCalendarCommandVm>.Error(ResponseCode.BadRequest, moveUsersResult.ErrorMessage ?? "Error movent d'OU");

            GoogleApiResult<bool> changeGroup = await _googleAdminApi.AddUserToGroup(p.ContactMail, oug.GroupMail);
            if (!changeGroup.Success) return Response<SyncEventToCalendarCommandVm>.Error(ResponseCode.BadRequest, moveUsersResult.ErrorMessage ?? "Error change user to group");

        }

        if (!string.IsNullOrEmpty(p.ContactMail))
        {
            return Response<SyncEventToCalendarCommandVm>.Ok(new SyncEventToCalendarCommandVm(p.ContactMail, password));
        }
        else
        {
            return Response<SyncEventToCalendarCommandVm>.Error(ResponseCode.InternalError, "No s'ha pogut creal l'email");
        }


    }

    public static string GetEmail(Person p, string emailDomain)
    {
        if (p.IsStudent)
        {
            return $"{Common.Helpers.Email.NormalizeText($"{p.Surname1}{p.Name}{p.AcademicRecordNumber}")}@{emailDomain}".ToLower();
        }
        return $"{Common.Helpers.Email.NormalizeText($"{p.Name}{p.Surname1}")}@{emailDomain}".ToLower();
    }

}
