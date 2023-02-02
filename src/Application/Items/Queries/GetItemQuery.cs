using Application.Common.Services;
using MediatR;

namespace Application.Items.Queries;

// What query returns
public record GetBasketVm(string text);

// What query receives
public record GetItemQuery() : IRequest<GetBasketVm>;

// Handl query logic
public class GetItemQueryHandler : IRequestHandler<GetItemQuery, GetBasketVm>
{
    private readonly IAuthenticationService _authService;

    public GetItemQueryHandler(IAuthenticationService authService)
    {
        _authService = authService;
    }

    public async Task<GetBasketVm> Handle(GetItemQuery request, CancellationToken ct)
    {
        // query logic here
        var _ = await _authService.GetUserAsync("ssanso", ct);
        return new GetBasketVm("ok");
    }
}