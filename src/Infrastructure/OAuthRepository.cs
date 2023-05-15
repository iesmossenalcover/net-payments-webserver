using System.Text.Json;

namespace Infrastructure;

public class OAuthRepository : Application.Common.Services.IOAuthRepository
{
    public async Task<Dictionary<string, string>?> TokenInfoValidation(string token, CancellationToken ct)
    {
        // Request user info using received access token.
        // TODO: Hardcoded for google...
        var httpClient = new HttpClient();
        var response = await httpClient.GetAsync($"https://www.googleapis.com/oauth2/v3/tokeninfo?id_token={token}");
        var json = await response.Content.ReadAsStringAsync();
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        }
        catch (System.Exception)
        {
            return null;
        }
    }
}