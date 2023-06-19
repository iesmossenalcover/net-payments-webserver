using System.Globalization;
using System.Text;
using Domain.Entities.People;

namespace Application.Common.Helpers;

public class Email
{


    public static string NormalizeText(string text)
    {
        text = text.Replace("ñ", "ny");
        text = text.Replace("l·l", "ll");
        text = text.Replace(".", "");
        text = text.Replace("ç", "c");
        text = text.Replace(" ", "");

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