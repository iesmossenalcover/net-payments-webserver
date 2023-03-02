namespace Application.Common.Models;

public record RedsysRequest(string MerchantParamenters, string SignatureVersion, string Signature);
public record RedsysResponse(string MerchantParamenters, string SignatureVersion, string Signature);