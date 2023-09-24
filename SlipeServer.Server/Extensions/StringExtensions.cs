using System.Text.RegularExpressions;

namespace SlipeServer.Server.Extensions;

public static partial class StringExtensions
{
    [GeneratedRegex("#[0-9a-fA-F]{6}")]
    private static partial Regex ColorCodeRegex();

    /// <summary>
    /// Removes color codes from a string
    /// </summary>
    public static string StripColorCode(this string value)
    {
        string temp = value;
        while (ColorCodeRegex().IsMatch(temp))
            temp = ColorCodeRegex().Replace(temp, "");
        return temp;
    }

    /// <summary>
    /// Checks if string contain color code
    /// </summary>
    public static bool ContainsColorCode(this string value) => ColorCodeRegex().IsMatch(value);
}
