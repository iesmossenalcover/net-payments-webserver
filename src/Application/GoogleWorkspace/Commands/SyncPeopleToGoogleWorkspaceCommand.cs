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
    private readonly ICsvParser _csvParser;
    private readonly string emailDomain;
    private readonly string tempFolderPath;
    private readonly string[] excludeEmails;

    public SyncPeopleToGoogleWorkspaceCommandHandler(
        IOUGroupRelationsRepository oUGroupRelationsRepository,
        IGoogleAdminApi googleAdminApi,
        ICoursesRepository courseRepository,
        IPersonGroupCourseRepository personGroupCourseRepository,
        IPeopleRepository peopleRepository,
        ITasksRepository tasksRepository,
        ICsvParser csvParser,
        IConfiguration configuration)
    {
        _googleAdminApi = googleAdminApi;
        _courseRepository = courseRepository;
        _personGroupCourseRepository = personGroupCourseRepository;
        _peopleRepository = peopleRepository;
        _oUGroupRelationsRepository = oUGroupRelationsRepository;
        _tasksRepository = tasksRepository;
        _csvParser = csvParser;
        emailDomain = configuration.GetValue<string>("GoogleApiDomain") ?? throw new Exception("GoogleApiDomain");
        tempFolderPath = configuration.GetValue<string>("TempFolderPath") ?? throw new Exception("TempFolderPath");
        excludeEmails = configuration.GetValue<string[]>("GoogleApiExcludeAccounts") ?? throw new Exception("GoogleApiExcludeAccounts");
    }

    #endregion

    public async Task<Response<SyncPeopleToGoogleWorkspaceCommandVm>> Handle(SyncPeopleToGoogleWorkspaceCommand request, CancellationToken ct)
    {
        Course course = await _courseRepository.GetCurrentCoursAsync(ct);
        Domain.Entities.Tasks.Task task = new Domain.Entities.Tasks.Task()
        {
            Status = Domain.Entities.Tasks.TaskStatus.RUNNING,
            Type = Domain.Entities.Tasks.TaskType.SYNC_USERS_TO_GOOGLE_WORKSPACE,
            Start = DateTimeOffset.UtcNow,
            End = null,
        };

        bool insertedTask = await _tasksRepository.AtomiInsertTaskAsync(task);
        if (!insertedTask)
        {
            return Response<SyncPeopleToGoogleWorkspaceCommandVm>.Error(ResponseCode.BadRequest, "Ja existeix una tasca del mateix tipus executant-se.");
        }

        var now = DateTimeOffset.UtcNow;
        string filePath = $"{tempFolderPath}export_users_{now.Date.Year}{now.Date.Month}{now.Date.Day}{now.DateTime.Hour}{now.DateTime.Second}.csv";
        string errorsFilePath = $"{tempFolderPath}errors_export_users_{now.Date.Year}{now.Date.Month}{now.Date.Day}{now.DateTime.Hour}{now.DateTime.Second}.csv";

        await _csvParser.WriteHeadersAsync<ErrorRow>(errorsFilePath);
        await _csvParser.WriteHeadersAsync<PersonRow>(filePath);

        IEnumerable<UoGroupRelation> ouRelations = await _oUGroupRelationsRepository.GetAllAsync(ct);

        foreach (var ou in ouRelations)
        {
            var clearGroupMemberResult = await _googleAdminApi.ClearGroupMembers(ou.GroupMail);

            GoogleApiResult<IEnumerable<string>> usersResult = await _googleAdminApi.GetAllUsers(ou.ActiveOU);
            if (!usersResult.Success || usersResult.Data == null)
            {
                await _csvParser.WriteToFileAsync(errorsFilePath, new ErrorRow { Message = $"Error function GetAllUsers, OU: {ou.ActiveOU}" }, false);
                continue;
            }

            //Move to old OU
            foreach (var user in usersResult.Data)
            {
                var result = await _googleAdminApi.MoveUserToOU(user, ou.OldOU);
                if (!result.Success)
                {
                    await _csvParser.WriteToFileAsync(errorsFilePath, new ErrorRow { Message = $"Error function MoveUserToOU, OU: {ou.OldOU}", Email = user }, false);
                    continue;
                }
            }

            IEnumerable<PersonGroupCourse> pgcs = await _personGroupCourseRepository.GetPeopleGroupByGroupIdAndCourseIdAsync(course.Id, ou.GroupId, ct);

            foreach (var pgc in pgcs)
            {

                Person p = pgc.Person;

                // IMPORTANT: Exclude members
                if (excludeEmails.Contains(p.ContactMail)) continue;

                string? password = null;
                bool createUser = string.IsNullOrEmpty(p.ContactMail);

                if (!string.IsNullOrEmpty(p.ContactMail))
                {
                    GoogleApiResult<bool> userExists = await _googleAdminApi.UserExists(p.ContactMail);
                    if (!userExists.Success)
                    {
                        await _csvParser.WriteToFileAsync(errorsFilePath, new ErrorRow { Message = $"Error function UserExists", Email = p.ContactMail }, false);
                        continue;
                    }

                    //Update password if nedded
                    if (userExists.Data && ou.UpdatePassword)
                    {
                        password = Common.Helpers.GenerateString.RandomAlphanumeric(8);
                        GoogleApiResult<bool> result = await _googleAdminApi.SetPassword(p.ContactMail, password, true);
                        if (!result.Success)
                        {
                            await _csvParser.WriteToFileAsync(errorsFilePath, new ErrorRow { Message = $"Error function SetPassword", Email = p.ContactMail }, false);
                            continue;
                        }
                    }
                    createUser = !userExists.Data;
                }

                if (createUser)
                {
                    password = Common.Helpers.GenerateString.RandomAlphanumeric(8);
                    p.ContactMail = SyncPersonToGoogleWorkspaceCommandHandler.GetEmail(p, emailDomain);
                    GoogleApiResult<bool> createUsersResult = await _googleAdminApi.CreateUser(p.ContactMail, p.Name.ToLower(), p.LastName.ToLower(), password, ou.ActiveOU);
                    if (!createUsersResult.Success)
                    {
                        await _csvParser.WriteToFileAsync(errorsFilePath, new ErrorRow { Message = $"Error function CreateUser, OU: {ou.ActiveOU}", Email = p.ContactMail }, false);
                        continue;
                    }

                    //On create user to google api, need time to execute the creation on the google site.
                    await Task.Delay(2000);


                }
                else if (!string.IsNullOrEmpty(p.ContactMail))
                {
                    GoogleApiResult<bool> moveUsersResult = await _googleAdminApi.MoveUserToOU(p.ContactMail, ou.ActiveOU);
                    if (!moveUsersResult.Success)
                    {
                        await _csvParser.WriteToFileAsync(errorsFilePath, new ErrorRow { Message = $"Error function MoveUserToOU, OU: {ou.ActiveOU}", Email = p.ContactMail }, false);
                        continue;
                    }
                }

                //Set user to new group
                var setGroupResult = await _googleAdminApi.AddUserToGroup(p.ContactMail ?? "", ou.GroupMail);
                if (!setGroupResult.Success)
                {
                    await _csvParser.WriteToFileAsync(errorsFilePath, new ErrorRow { Message = $"Error function AddUserToGroup, OU: {ou.GroupMail}", Email = p.ContactMail }, false);
                    continue;
                }
                await _peopleRepository.UpdateAsync(p, ct);

                var pr = new PersonRow()
                {
                    Email = p.ContactMail ?? "",
                    FirstName = p.Name,
                    LastName = p.LastName,
                    GroupName = pgc.Group.Name,
                    TempPassword = password ?? "****",
                };

                await _csvParser.WriteManyToFileAsync(filePath, new List<PersonRow>() { pr }, false);

            }
        }

        // Finish task
        task.End = DateTimeOffset.UtcNow;
        task.Status = Domain.Entities.Tasks.TaskStatus.SUCCESS;
        await _tasksRepository.UpdateAsync(task, CancellationToken.None);
        return Response<SyncPeopleToGoogleWorkspaceCommandVm>.Ok(new SyncPeopleToGoogleWorkspaceCommandVm());
    }
}

public class PersonRow
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TempPassword { get; set; } = string.Empty;
}

public class ErrorRow
{
    public string? Email { get; set; }
    public string Message { get; set; } = string.Empty;
}