using Application.Common;
using Domain.Services;
using FluentValidation;
using MediatR;
using Domain.Entities.Orders;
using Application.Common.Models;
using Domain.Entities.Events;
using Domain.Entities.People;
using Domain.Behaviours;

namespace Application.Orders.Commands;

public record ConfirmOrderCommandVm();

public record ConfirmOrderCommand : IRequest<Response<ConfirmOrderCommandVm?>>
{
    public string MerchantParamenters { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
}

public class ConfirmOrderCommandHandler : IRequestHandler<ConfirmOrderCommand, Response<ConfirmOrderCommandVm?>>
{
    #region IOC

    private readonly IOrdersRepository _ordersRepository;
    private readonly IRedsys _redsys;
    private readonly OrderBehaviours _orderBehaviours;

    public ConfirmOrderCommandHandler(IOrdersRepository ordersRepository, IRedsys redsys,
        OrderBehaviours orderBehaviours)
    {
        _ordersRepository = ordersRepository;
        _redsys = redsys;
        _orderBehaviours = orderBehaviours;
    }

    #endregion

    public async Task<Response<ConfirmOrderCommandVm?>> Handle(ConfirmOrderCommand request, CancellationToken ct)
    {
        ct = CancellationToken.None;

        bool isValid = _redsys.Validate(request.MerchantParamenters, request.Signature);
        if (!isValid)
        {
            return Response<ConfirmOrderCommandVm?>.Error(ResponseCode.BadRequest, "Firma invàlida");
        }

        RedsysResult result = _redsys.GetResult(request.MerchantParamenters);
        Order? order = await _ordersRepository.GetByCodeAsync(result.OrderCode, ct);
        if (order == null)
        {
            return Response<ConfirmOrderCommandVm?>.Error(ResponseCode.BadRequest, "Error, l'ordre no existeix");
        }

        // If already paid then return ok.
        if (order.Status == OrderStatus.Paid && result.Success)
        {
            return Response<ConfirmOrderCommandVm?>.Ok(new ConfirmOrderCommandVm());
        }

        if (!result.Success)
        {
            order.Status = OrderStatus.Error;
            await _ordersRepository.UpdateAsync(order, ct);
            return Response<ConfirmOrderCommandVm?>.Error(ResponseCode.BadRequest, result.ErrorMessage ?? string.Empty);
        }

        string? error = (await _orderBehaviours.PayOrder(order, ct)).ErrorMessage();
        if (error != null)
        {
            return Response<ConfirmOrderCommandVm?>.Error(ResponseCode.BadRequest, error);
        }

        return Response<ConfirmOrderCommandVm?>.Ok(new ConfirmOrderCommandVm());
    }
}