using Domain.Entities.Authentication;

namespace Domain.Services;

public interface IOAuthRepository
{
    public Task<Dictionary<string, string>?> TokenInfoValidation(string token, CancellationToken ct);
}