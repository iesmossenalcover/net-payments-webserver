using Domain.Entities.Authentication;

namespace Application.Common.Services;

public interface IOAuthRepository
{
    public Task<Dictionary<string, string>?> TokenInfoValidation(string token, CancellationToken ct);
}