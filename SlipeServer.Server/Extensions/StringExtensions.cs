using System.Text.RegularExpressions;

namespace SlipeServer.Server.Extensions;

public static class StringExtensions
{
    private const string colorCodeRegex = "#[0-9a-fA-F]{6}";

    public static string StripColorCode(this string value)
    {
        string temp = value;
        while (Regex.IsMatch(temp, colorCodeRegex))
            temp = Regex.Replace(temp, colorCodeRegex, "");
        return temp;
    }
}
