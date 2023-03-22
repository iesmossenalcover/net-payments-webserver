using Application.Common;
using Application.Common.Services;
using Domain.Entities.Configuration;
using FluentValidation;
using MediatR;

namespace Application.AdminInfo.Commands;

public record UpdateAppConfigCommand : IRequest<Response<long?>>
{
    public bool DisplayEnrollment {get;set;}
}

public class UpdateAppConfigCommandValidator : AbstractValidator<UpdateAppConfigCommand>
{
	public UpdateAppConfigCommandValidator()
	{
        
    }
}

public class UpdateAppConfigCommandHandler : IRequestHandler<UpdateAppConfigCommand, Response<long?>>
{
    #region IOC
    private readonly IAppConfigRepository _appConfigRepository;

    public UpdateAppConfigCommandHandler(IAppConfigRepository appConfigRepository)
    {
        _appConfigRepository = appConfigRepository;
    }

    #endregion

    public async Task<Response<long?>> Handle(UpdateAppConfigCommand request, CancellationToken ct)
    {

        AppConfig config = await _appConfigRepository.GetAsync(ct);
        config.DisplayEnrollment = request.DisplayEnrollment;

         await _appConfigRepository.UpdateAsync(config, CancellationToken.None);

        return Response<long?>.Ok(config.Id);

    }


}