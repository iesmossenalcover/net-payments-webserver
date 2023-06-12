using System.Data;
using Application.Common;
using Application.Common.Models;
using Application.Common.Services;
using Domain.Entities.People;
using FluentValidation;
using MediatR;

namespace Application.Tasks.Commands;

// Model we receive
public record SyncStudentsCommand() : IRequest<Response<SyncStudentsCommandVm>>;

// Validator for the model

// Optionally define a view model
public record SyncStudentsCommandVm();

// Handler
public class SyncStudentsCommandHandler : IRequestHandler<SyncStudentsCommand, Response<SyncStudentsCommandVm>>
{
    #region props

    private readonly IGoogleAdminApi _googleAdminApi;

    public SyncStudentsCommandHandler(IGoogleAdminApi googleAdminApi)
    {
        _googleAdminApi = googleAdminApi;
    }

    public async Task<Response<SyncStudentsCommandVm>> Handle(SyncStudentsCommand request, CancellationToken ct)
    {
        GoogleApiResult<bool> getUsersResult = await _googleAdminApi.DeleteUserOfGroup("test1234@iesmossenalcover.cat","payments.superuser@iesmossenalcover.cat" );

        return Response<SyncStudentsCommandVm>.Ok(new SyncStudentsCommandVm());
    }
    #endregion


}