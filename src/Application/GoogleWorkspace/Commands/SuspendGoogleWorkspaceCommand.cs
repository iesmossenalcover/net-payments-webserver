using Application.Common;
using Application.Common.Models;
using Application.Common.Services;
using Domain.Entities.GoogleApi;
using Domain.Entities.People;
using MediatR;

namespace Application.GoogleWorkspace.Commands;

// Model we receive
public record SuspendGoogleWorkspaceCommand() : IRequest<Response<SuspendGoogleWorkspaceCommandVm>>;

// Validator for the model

// Optionally define a view model
public record SuspendGoogleWorkspaceCommandVm(bool ok);

// Handler
public class SuspendGoogleWorkspaceCommandHandler : IRequestHandler<SuspendGoogleWorkspaceCommand, Response<SuspendGoogleWorkspaceCommandVm>>
{
    #region props
    private readonly IOUGroupRelationsRepository _oUGroupRelationsRepository;
    private readonly IGoogleAdminApi _googleAdminApi;

    public SuspendGoogleWorkspaceCommandHandler(IOUGroupRelationsRepository oUGroupRelationsRepository, IGoogleAdminApi googleAdminApi, IConfiguration configuration)
    {
        _googleAdminApi = googleAdminApi;
        _oUGroupRelationsRepository = oUGroupRelationsRepository;
    }
    #endregion

    public async Task<Response<SuspendGoogleWorkspaceCommandVm>> Handle(SuspendGoogleWorkspaceCommand request, CancellationToken ct)
    {

        IEnumerable<UoGroupRelation> ouRelations = await _oUGroupRelationsRepository.GetAllAsync(ct);
        IEnumerable<string> pendings = ouRelations.Select(x => x.OldOU).Distinct();


        foreach (var ou in pendings)
        {
            GoogleApiResult<bool> result = await _googleAdminApi.SetSuspendByOU(ou, true, false);
            if (!result.Success) return Response<SuspendGoogleWorkspaceCommandVm>.Error(ResponseCode.InternalError, result.ErrorMessage ?? $"Error cridant api google. per la OU {ou}");
        }

        return Response<SuspendGoogleWorkspaceCommandVm>.Ok(new SuspendGoogleWorkspaceCommandVm(true));
    }
}