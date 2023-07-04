using Application.Common;
using Application.Common.Models;
using Domain.Services;
using Domain.Entities.GoogleApi;
using Domain.Entities.People;
using MediatR;

namespace Application.GoogleWorkspace.Commands;

// Model we receive
public record MovePeopleGoogleWorkspaceCommand() : IRequest<Response<MovePeopleGoogleWorkspaceCommandVm>>;

// Validator for the model

// Optionally define a view model
public record MovePeopleGoogleWorkspaceCommandVm(bool ok);

// Handler
public class MovePeopleGoogleWorkspaceCommandHandler : IRequestHandler<MovePeopleGoogleWorkspaceCommand, Response<MovePeopleGoogleWorkspaceCommandVm>>
{
    #region props
    private readonly IOUGroupRelationsRepository _oUGroupRelationsRepository;
    private readonly IGoogleAdminApi _googleAdminApi;
    private readonly string emailDomain;
    private readonly string[] excludeEmails;

    public MovePeopleGoogleWorkspaceCommandHandler(IOUGroupRelationsRepository oUGroupRelationsRepository, IGoogleAdminApi googleAdminApi, IConfiguration configuration)
    {
        _googleAdminApi = googleAdminApi;
        _oUGroupRelationsRepository = oUGroupRelationsRepository;
        emailDomain = configuration.GetValue<string>("GoogleApiDomain") ?? throw new Exception("GoogleApiDomain");
       excludeEmails = configuration.GetValue<string>("GoogleApiExcludeAccounts")?.Split(" ") ?? throw new Exception("GoogleApiExcludeAccounts");
    }
    #endregion

    public async Task<Response<MovePeopleGoogleWorkspaceCommandVm>> Handle(MovePeopleGoogleWorkspaceCommand request, CancellationToken ct)
    {

        IEnumerable<UoGroupRelation> ouRelations = await _oUGroupRelationsRepository.GetAllAsync(ct);

        foreach (var ou in ouRelations)
        {
            GoogleApiResult<IEnumerable<string>> usersResult = await _googleAdminApi.GetAllUsers(ou.ActiveOU);
            if (!usersResult.Success || usersResult.Data == null)
            {
                return Response<MovePeopleGoogleWorkspaceCommandVm>.Error(ResponseCode.InternalError, $"Error function GetAllUsers, OU: {ou.GroupMail}");
            }

            foreach (var user in usersResult.Data)
            {
                // IMPORTANT: Exclude members
                if (excludeEmails.Contains(user)) continue;

                var result = await _googleAdminApi.MoveUserToOU(user, ou.OldOU);
                if (!result.Success)
                {
                    return Response<MovePeopleGoogleWorkspaceCommandVm>.Error(ResponseCode.InternalError, $"Error function MoveUserToOU, OU: {ou.GroupMail} USER: {user}");
                }
            }
        }

        return Response<MovePeopleGoogleWorkspaceCommandVm>.Ok(new MovePeopleGoogleWorkspaceCommandVm(true));
    }
}