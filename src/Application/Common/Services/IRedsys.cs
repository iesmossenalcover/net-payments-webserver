using Application.Common.Models;
using Domain.Entities.Orders;

namespace Application.Common.Services;

public interface IRedsys
{
    public RedsysRequest CreateRedsysRequest(Order order);
    public bool Validate(RedsysResponse response);
}