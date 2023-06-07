namespace Application.Common.Services;

public interface IGoogleAdminApi
{
    public Task<IEnumerable<string>> GetUserClaims(string email, CancellationToken ct);
    public Task Test(CancellationToken ct);
}