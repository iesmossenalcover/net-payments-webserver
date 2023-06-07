using System.Data;
using Application.Common;
using Application.Common.Models;
using Application.Common.Services;
using Domain.Entities.People;
using FluentValidation;
using MediatR;

namespace Application.Tasks.Commands;

// Model we receive
public record ProcessUsersCommand() : IRequest<Response<ProcessUsersCommandVm>>;

// Validator for the model

// Optionally define a view model
public record ProcessUsersCommandVm();

// Handler
public class ProcessUsersCommandHandler : IRequestHandler<ProcessUsersCommand, Response<ProcessUsersCommandVm>>
{
    #region props

    private readonly IGoogleAdminApi _googleAdminApi;

    public ProcessUsersCommandHandler(IGoogleAdminApi googleAdminApi)
    {
        _googleAdminApi = googleAdminApi;
    }

    public async Task<Response<ProcessUsersCommandVm>> Handle(ProcessUsersCommand request, CancellationToken ct)
    {
        await _googleAdminApi.Test(ct);

        return Response<ProcessUsersCommandVm>.Ok(new ProcessUsersCommandVm());
    }
    #endregion


}