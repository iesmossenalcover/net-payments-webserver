using System.Globalization;
using System.Text;
using Domain.Entities.People;

namespace Application.Common.Helpers;

public class Email
{


    public static string NormalizeText(string text)
    {
        // string final = text.Replace('à','a');
        // final = text.Replace('á','a');
        // final = text.Replace('è','e');
        // final = text.Replace('é','e');
        // final = text.Replace('ì','i');
        // final = text.Replace('í','i');
        // final = text.Replace('ó','o');
        // final = text.Replace('ò','o');
        // final = text.Replace('ù','u');
        // final = text.Replace('ú','u');

        text = text.Replace("ñ", "ny");
        text = text.Replace("l·l", "ll");
        text = text.Replace("ç", "c");

        var normalizedText = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedText)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString();
    }
}