using Application.Common;
using Application.Common.Models;
using Application.Common.Services;
using Domain.Entities.People;
using MediatR;

namespace Application.GoogleWorkspace.Commands;

// Model we receive
public record SuspendGoogleWorkspaceCommand(string Path) : IRequest<Response<SuspendGoogleWorkspaceCommandVm>>;

// Validator for the model

// Optionally define a view model
public record SuspendGoogleWorkspaceCommandVm(bool ok);

// Handler
public class SuspendGoogleWorkspaceCommandHandler : IRequestHandler<SuspendGoogleWorkspaceCommand, Response<SuspendGoogleWorkspaceCommandVm>>
{
    #region props

    private readonly IGoogleAdminApi _googleAdminApi;
    private readonly string emailDomain;

    public SuspendGoogleWorkspaceCommandHandler(IGoogleAdminApi googleAdminApi, IConfiguration configuration)
    {
        _googleAdminApi = googleAdminApi;
        emailDomain = configuration.GetValue<string>("GoogleApiDomain") ?? throw new Exception("GoogleApiDomain");
    }
    #endregion

    public async Task<Response<SuspendGoogleWorkspaceCommandVm>> Handle(SuspendGoogleWorkspaceCommand request, CancellationToken ct)
    {

        string path = request.Path;
        if (string.IsNullOrEmpty(path)) return Response<SuspendGoogleWorkspaceCommandVm>.Error(ResponseCode.BadRequest, "No s'ha especificat ruta");

        GoogleApiResult<bool> result = await _googleAdminApi.SetSuspendByOU(path, true, false);
        if (!result.Success) return Response<SuspendGoogleWorkspaceCommandVm>.Error(ResponseCode.InternalError, result.ErrorMessage ?? "Error cridant api google.");

        return Response<SuspendGoogleWorkspaceCommandVm>.Ok(new SuspendGoogleWorkspaceCommandVm(true));
    }
}