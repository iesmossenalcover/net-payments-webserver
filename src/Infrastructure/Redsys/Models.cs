using System.Text.Json.Serialization;

namespace Infrastructure.Redsys;

public record MerchantParametersRequest
{

    [JsonPropertyName("DS_MERCHANT_AMOUNT")]
    public long Amount { get; set; }

    [JsonPropertyName("DS_MERCHANT_ORDER")]
    public string Code { get; set; } = default!;

    [JsonPropertyName("DS_MERCHANT_MERCHANTCODE")]
    public long ClientCode { get; set; }

    [JsonPropertyName("DS_MERCHANT_CURRENCY")]
    public short CurrencyCode { get; set; }

    [JsonPropertyName("DS_MERCHANT_TRANSACTIONTYPE")]
    public short TransactionType { get; set; }

    [JsonPropertyName("DS_MERCHANT_TERMINAL")]
    public string TerminalNumber { get; set; } = default!;

    [JsonPropertyName("DS_MERCHANT_MERCHANTURL")]
    public string? UrlApiResponse { get; set; }

    [JsonPropertyName("DS_MERCHANT_URLOK")]
    public string? UrlRedirectOk { get; set; }
    [JsonPropertyName("DS_MERCHANT_URLKO")]
    public string? UrlRedirectKo { get; set; }

    [JsonPropertyName("DS_MERCHANT_CONSUMERLANGUAGE")]
    public short LanguageCode { get; set; }
}

public record MerchantParametersResponse
{

    [JsonPropertyName("Ds_Order")]
    public string Code { get; set; } = default!;

    [JsonPropertyName("Ds_Response")]
    public string Response { get; set; } = default!;
}