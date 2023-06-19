using System.Data;
using Application.Common;
using Application.Common.Models;
using Application.Common.Services;
using Domain.Entities.GoogleApi;
using Domain.Entities.People;
using FluentValidation;
using MediatR;

namespace Application.GoogleWorkspace.Commands;

// Model we receive
public record SyncPeopleToGoogleWorkspaceCommand() : IRequest<Response<SyncPeopleToGoogleWorkspaceCommandVm>>;

// Validator for the model

// Optionally define a view model
public record SyncPeopleToGoogleWorkspaceCommandVm();

// Handler
public class SyncPeopleToGoogleWorkspaceCommandHandler : IRequestHandler<SyncPeopleToGoogleWorkspaceCommand, Response<SyncPeopleToGoogleWorkspaceCommandVm>>
{
    #region IOC

    private readonly IGoogleAdminApi _googleAdminApi;
    private readonly ICoursesRepository _courseRepository;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepository;
    private readonly IPeopleRepository _peopleRepository;
    private readonly IOUGroupRelationsRepository _oUGroupRelationsRepository;
    private readonly ITasksRepository _tasksRepository;
    private readonly string emailDomain;

    public SyncPeopleToGoogleWorkspaceCommandHandler(
        IOUGroupRelationsRepository oUGroupRelationsRepository,
        IGoogleAdminApi googleAdminApi,
        ICoursesRepository courseRepository,
        IPersonGroupCourseRepository personGroupCourseRepository,
        IPeopleRepository peopleRepository,
        ITasksRepository tasksRepository,
        IConfiguration configuration)
    {
        _googleAdminApi = googleAdminApi;
        _courseRepository = courseRepository;
        _personGroupCourseRepository = personGroupCourseRepository;
        _peopleRepository = peopleRepository;
        _oUGroupRelationsRepository = oUGroupRelationsRepository;
        _tasksRepository = tasksRepository;
        emailDomain = configuration.GetValue<string>("GoogleApiDomain") ?? throw new Exception("GoogleApiDomain");
    }

    #endregion

    public async Task<Response<SyncPeopleToGoogleWorkspaceCommandVm>> Handle(SyncPeopleToGoogleWorkspaceCommand request, CancellationToken ct)
    {
        Course course = await _courseRepository.GetCurrentCoursAsync(ct);
        Domain.Entities.Tasks.Task task = new Domain.Entities.Tasks.Task()
        {
            Status = Domain.Entities.Tasks.TaskStatus.PENDING,
            Type = Domain.Entities.Tasks.TaskType.SYNC_USERS_TO_GOOGLE_WORKSPACE,
            Start = DateTimeOffset.UtcNow,
            End = null,
        };

        bool insertedTask = await _tasksRepository.AtomiInsertTaskAsync(task);
        if (!insertedTask)
        {
            return Response<SyncPeopleToGoogleWorkspaceCommandVm>.Error(ResponseCode.BadRequest, "Ja existeix una tasca del mateix tipus executant-se.");
        }

        IEnumerable<UoGroupRelation> ouRelations = await _oUGroupRelationsRepository.GetAllAsync(ct);

        foreach (var ou in ouRelations)
        {
            var clearGroupMemberResult = await _googleAdminApi.ClearGroupMembers(ou.GroupMail);
            if (!clearGroupMemberResult.Success) throw new Exception("Enregistrar error");

            GoogleApiResult<IEnumerable<string>> usersResult = await _googleAdminApi.GetAllUsers(ou.ActiveOU);
            if (!usersResult.Success || usersResult.Data == null) throw new Exception("Enregistrar error");

            //Move to old OU
            foreach (var user in usersResult.Data)
            {
                var result = await _googleAdminApi.MoveUserToOU(user, ou.OldOU);
                if (!result.Success) throw new Exception("Enregistrar error");
            }

            IEnumerable<PersonGroupCourse> pgcs = await _personGroupCourseRepository.GetPeopleGroupByGroupIdAndCourseIdAsync(course.Id, ou.GroupId, ct);

            foreach (var pgc in pgcs)
            {

                Person p = pgc.Person;
                string? password = null;
                bool createUser = string.IsNullOrEmpty(p.ContactMail);

                if (!string.IsNullOrEmpty(p.ContactMail))
                {
                    GoogleApiResult<bool> userExists = await _googleAdminApi.UserExists(p.ContactMail);
                    if (!userExists.Success) throw new Exception("Enregistrar error");

                    //Update password if nedded
                    if (userExists.Data && ou.UpdatePassword)
                    {
                        password = Common.Helpers.GenerateString.RandomAlphanumeric(8);
                        GoogleApiResult<bool> result = await _googleAdminApi.SetPassword(p.ContactMail, password, true);
                        if (!result.Success) throw new Exception("Enregistrar error");
                    }
                    createUser = !userExists.Data;
                }

                if (createUser)
                {
                    password = Common.Helpers.GenerateString.RandomAlphanumeric(8);
                    p.ContactMail = SyncPersonToGoogleWorkspaceCommandHandler.GetEmail(p, emailDomain);
                    GoogleApiResult<bool> createUsersResult = await _googleAdminApi.CreateUser(p.ContactMail, p.Name.ToLower(), p.LastName.ToLower(), password, ou.ActiveOU);
                    if (!createUsersResult.Success) throw new Exception("Enregistrar error");

                    //On create user to google api, need time to execute the creation on the google site.
                    await Task.Delay(2000);


                }
                else if (!string.IsNullOrEmpty(p.ContactMail))
                {
                    GoogleApiResult<bool> moveUsersResult = await _googleAdminApi.MoveUserToOU(p.ContactMail, ou.ActiveOU);
                    if (!moveUsersResult.Success) throw new Exception("Enregistrar error");
                }

                //Set user to new group
                var setGroupResult = await _googleAdminApi.AddUserToGroup(p.ContactMail ?? "", ou.GroupMail);
                if (!setGroupResult.Success) throw new Exception("Enregistrar error");
                await _peopleRepository.UpdateAsync(p, ct);

            }
        }


        return Response<SyncPeopleToGoogleWorkspaceCommandVm>.Ok(new SyncPeopleToGoogleWorkspaceCommandVm());
    }
}