using Application.Common;
using Application.Common.Models;
using Domain.Services;
using Domain.Entities.People;
using MediatR;

namespace Application.GoogleWorkspace.Commands;

// Model we receive
public record SetPasswordGoogleWorkspaceCommand(long Id) : IRequest<Response<SetPasswordGoogleWorkspaceCommandVm>>;

// Validator for the model

// Optionally define a view model
public record SetPasswordGoogleWorkspaceCommandVm(string Password);

// Handler
public class SetPasswordGoogleWorkspaceCommandHandler : IRequestHandler<SetPasswordGoogleWorkspaceCommand, Response<SetPasswordGoogleWorkspaceCommandVm>>
{
    #region props

    private readonly IGoogleAdminApi _googleAdminApi;
    private readonly IPeopleRepository _peopleRepository;

    public SetPasswordGoogleWorkspaceCommandHandler(IGoogleAdminApi googleAdminApi, IPeopleRepository peopleRepository)
    {
        _googleAdminApi = googleAdminApi;
        _peopleRepository = peopleRepository;
    }
    #endregion

    public async Task<Response<SetPasswordGoogleWorkspaceCommandVm>> Handle(SetPasswordGoogleWorkspaceCommand request, CancellationToken ct)
    {
        Person? p = await _peopleRepository.GetByIdAsync(request.Id, ct);
        if (p == null) return Response<SetPasswordGoogleWorkspaceCommandVm>.Error(ResponseCode.NotFound, "Usuari no trobat");

        if (string.IsNullOrEmpty(p.ContactMail)) return Response<SetPasswordGoogleWorkspaceCommandVm>.Error(ResponseCode.BadRequest, "L'usuari no té correu.");

        string password = Common.Helpers.GenerateString.RandomAlphanumeric(8);
        GoogleApiResult<bool> result = await _googleAdminApi.SetPassword(p.ContactMail, password, true);
        if (!result.Success) return Response<SetPasswordGoogleWorkspaceCommandVm>.Error(ResponseCode.InternalError, result.ErrorMessage ?? "Error cridant api google.");

        return Response<SetPasswordGoogleWorkspaceCommandVm>.Ok(new SetPasswordGoogleWorkspaceCommandVm(password));
    }
}