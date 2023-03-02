using Application.Common.Models;
using Application.Common.Services;
using Domain.Entities.Orders;

namespace Infrastructure.Redsys;

public class RedsysApi : IRedsys
{
    private readonly string MERCHANT_KEY = "sq7HjrUOBfKmC576ILgskD5srU870gJ7";

    public RedsysRequest CreateRedsysRequest(Order order)
    {
        var merchantParameters = new MerchantParametersRequest()
        {
            Code = order.Code,
            Amount = (long) (order.Amount * 100),
            CurrencyCode = 978,
            LanguageCode = 3,
            TerminalNumber = "001",
            TransactionType = 0,
            UrlApiResponse = "http://localhost:5185/",
            UrlRedirectOk = "http://localhost:5185/Ok",
            UrlRedirectKo = "http://localhost:5185/Ko",
            ClientCode = 999008881,
        };
        const string signatureVersion = "HMAC_SHA256_V1";
        string encodedMerchantParameters = Helpers.MerchantParametersBase64Encoded(merchantParameters);
        string signature = Helpers.CreateSignature(MERCHANT_KEY, merchantParameters.Code, encodedMerchantParameters);
        return new RedsysRequest(encodedMerchantParameters, signatureVersion, signature);
    }

    public bool Validate(RedsysResponse response)
    {
        if (response == null) return false;

        MerchantParametersResponse? merchantParametersResponse = Helpers.MerchantParametersResponseBase64Decode(response.MerchantParamenters);
        if (merchantParametersResponse == null) return false;

        string signature = Helpers.CreateSignature(MERCHANT_KEY, merchantParametersResponse.Code, response.MerchantParamenters);

        return response.Signature == signature;
    }
}