using Application.Common.Models;
using Application.Common.Services;
using Domain.Entities.Orders;

namespace Infrastructure.Redsys;

public class RedsysApi : IRedsys
{
    private const string SignatureVersion = "HMAC_SHA256_V1";
    private readonly string TerminalNumber;
    private readonly string UrlApiResponse;
    private readonly string UrlRedirectOk;
    private readonly string UrlRedirectKo;
    private readonly string MerchantKey;
    private readonly string RedsysUrl;
    private readonly long MerchantCode;

    public RedsysApi(IConfiguration configuration)
    {
        TerminalNumber = configuration.GetValue<string>("RedsysTerminalNumber") ?? throw new Exception("RedsysTerminalNumber");
        UrlApiResponse = configuration.GetValue<string>("RedsysUrlApiResponse") ?? throw new Exception("UrlApiResponse");
        UrlRedirectOk = configuration.GetValue<string>("RedsysUrlRedirectOk") ?? throw new Exception("UrlRedirectOk");
        UrlRedirectKo = configuration.GetValue<string>("RedsysUrlRedirectKo") ?? throw new Exception("UrlRedirectKo");
        MerchantKey = configuration.GetValue<string>("RedsysMerchantKey") ?? throw new Exception("MerchantKey");
        MerchantCode = configuration.GetValue<long>("RedsysMerchantCode");
        RedsysUrl = configuration.GetValue<string>("RedsysUrl") ?? throw new Exception("RedsysUrl");
    }

    public RedsysRequest CreateRedsysRequest(Order order)
    {
        var merchantParameters = new MerchantParametersRequest()
        {
            Code = order.Code,
            Amount = (long) (order.Amount * 100),
            CurrencyCode = 978, //eur
            LanguageCode = 3,
            TransactionType = 0,
            TerminalNumber = TerminalNumber,
            UrlApiResponse = UrlApiResponse,
            UrlRedirectOk = UrlRedirectOk,
            UrlRedirectKo = UrlRedirectKo,
            MerchantCode = MerchantCode,
        };
        string encodedMerchantParameters = Helpers.MerchantParametersBase64Encoded(merchantParameters);
        string signature = Helpers.CreateSignature(MerchantKey, merchantParameters.Code, encodedMerchantParameters);
        return new RedsysRequest(RedsysUrl, encodedMerchantParameters, SignatureVersion, signature);
    }

    public RedsysResult GetResult(string merchantParameters)
    {
        MerchantParametersResponse? merchantParametersResponse = Helpers.MerchantParametersResponseBase64Decode(merchantParameters);
        if (merchantParametersResponse == null) return new RedsysResult(false, string.Empty, "No s'ha pogut decodificar la resposta");

        int code = 0;
        if (!int.TryParse(merchantParametersResponse.StatusCode, out code)) return new RedsysResult(false, string.Empty, "No s'ha pogut obtenir el codi de resposta");

        if (code >= 0 && code < 100)
        {
            return new RedsysResult(true, merchantParametersResponse.OrderCode);
        }

        return new RedsysResult(false, string.Empty, merchantParametersResponse.StatusCode);
    }

    public bool Validate(string merchantParameters, string signature)
    {
        try
        {
            MerchantParametersResponse? merchantParametersResponse = Helpers.MerchantParametersResponseBase64Decode(merchantParameters);
            if (merchantParametersResponse == null) return false;
            string ownSignature = Helpers.CreateSignature(MerchantKey, merchantParametersResponse.OrderCode, merchantParameters);
            return signature == ownSignature;
        }
        catch (System.Exception)
        {
            return false;
        }
    }
}