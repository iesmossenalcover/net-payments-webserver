using System.Security.Cryptography;
using System.Text.Json;
using System.Text;

namespace Infrastructure.Redsys
{
    public class Helpers
    {
        public static string MerchantParametersBase64Encoded(MerchantParametersRequest merchantParameters)
        {
            string jsonStr = JsonSerializer.Serialize(merchantParameters);
            return Base64UrlEncode(jsonStr);
        }

        public static MerchantParametersResponse? MerchantParametersResponseBase64Decode(string txt)
        {
            string decoded = Base64UrlDecode(txt);
            return JsonSerializer.Deserialize<MerchantParametersResponse>(decoded);
        }

        public static string CreateSignature(string merchantKey, string orderCode, string encodedMerchantParameters)
        {
            byte[] kk = Encrypt3DES(orderCode, merchantKey);
            byte[] hashedParams = GetHMACSHA256(encodedMerchantParameters, kk);
            return Base64Encode(hashedParams);
        }

        public static string Base64UrlEncode(string txt)
        {
            byte[] encodeAsBytes = Encoding.UTF8.GetBytes(txt);
            return Base64Encode(encodeAsBytes);
        }

        public static string Base64Encode(byte[] data)
        {
            return Convert.ToBase64String(data)
                .Replace('+', '-').Replace('/', '_'); // base64url encode
        }

        public static string Base64UrlDecode(string txt)
        {
            txt = txt.Replace('-', '+').Replace('_', '/'); // base64url decode
            byte[] data = Convert.FromBase64String(txt);
            return Encoding.UTF8.GetString(data);
        }

        public static byte[] Encrypt3DES(string txt, string key)
        {
            byte[] data = Encoding.UTF8.GetBytes(txt);
            byte[] SALT = new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
            using TripleDES tripleDes = TripleDES.Create();
            tripleDes.BlockSize = 64;
            tripleDes.KeySize = 192;
            tripleDes.Mode = CipherMode.CBC;
            tripleDes.Padding = PaddingMode.Zeros;
            tripleDes.IV = SALT;
            tripleDes.Key = Convert.FromBase64String(key);
            using ICryptoTransform transformation = tripleDes.CreateEncryptor();
            return transformation.TransformFinalBlock(data, 0, data.Length);
        }

        public static byte[] GetHMACSHA256(string txt, byte[] key)
        {
            byte[] txtData = Encoding.UTF8.GetBytes(txt);
            using HMACSHA256 hmac = new HMACSHA256(key);
            return hmac.ComputeHash(txtData, 0, txtData.Length);
        }
    }
}
