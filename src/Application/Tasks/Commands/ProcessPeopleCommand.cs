using System.Data;
using Application.Common;
using Application.Common.Models;
using Application.Common.Services;
using Domain.Entities.People;
using FluentValidation;
using MediatR;

namespace Application.Tasks.Commands;

// Model we receive
public record ProcessPeopleCommand() : IRequest<Response<ProcessPeopleCommandVm>>;

// Validator for the model

// Optionally define a view model
public record ProcessPeopleCommandVm();

// Handler
public class ProcessPeopleCommandHandler : IRequestHandler<ProcessPeopleCommand, Response<ProcessPeopleCommandVm>>
{
    #region props

    private readonly IGoogleAdminApi _googleAdminApi;

    public ProcessPeopleCommandHandler(IGoogleAdminApi googleAdminApi)
    {
        _googleAdminApi = googleAdminApi;
    }

    public async Task<Response<ProcessPeopleCommandVm>> Handle(ProcessPeopleCommand request, CancellationToken ct)
    {
        await _googleAdminApi.Test(ct);

        return Response<ProcessPeopleCommandVm>.Ok(new ProcessPeopleCommandVm());
    }
    #endregion


}