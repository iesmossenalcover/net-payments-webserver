namespace Application.Common.Models;

public record RedsysRequest(string Url, string MerchantParamenters, string SignatureVersion, string Signature);
public record RedsysResult(bool Success, string OrderCode, string? ErrorMessage = null);