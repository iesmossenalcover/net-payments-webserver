using System.Text;

namespace Application.Common.Helpers;

public class GenerateString
{
    public static string Random(int length)
    {
        const string pool = "abcdefghijklmnopqrstuvwxyz";
        var builder = new StringBuilder(length);
        var random = new Random();
        for (var i = 0; i < length; i++)
        {
            var c = pool[random.Next(0, pool.Length)];
            builder.Append(c);
        }

        return builder.ToString();
    }

    public static string RandomAlphanumeric(int length)
    {
        const string pool = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var builder = new StringBuilder(length);
        var random = new Random();
        for (var i = 0; i < length; i++)
        {
            var c = pool[random.Next(0, pool.Length)];
            builder.Append(c);
        }

        return builder.ToString();
    }

    public static string RandomNumeric(int length)
    {
        const string pool = "0123456789";
        var builder = new StringBuilder(length);
        var random = new Random();
        for (var i = 0; i < length; i++)
        {
            var c = pool[random.Next(0, pool.Length)];
            builder.Append(c);
        }

        return builder.ToString();
    }

    public static string CurrentDateAsCode()
    {
        return DateTime.UtcNow.ToString("yyyyMMdd");
    }
}