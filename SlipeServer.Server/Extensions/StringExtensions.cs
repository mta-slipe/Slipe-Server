using System.Text.RegularExpressions;

namespace SlipeServer.Server.Extensions;

public static partial class StringExtensions
{
    [GeneratedRegex("#[0-9a-fA-F]{6}")]
    private static partial Regex ColorCodeRegex();
    [GeneratedRegex("^[!-~]{1,22}$")]
    private static partial Regex NickNameRegex();

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

    /// <summary>
    /// Checks if string is valid nickname. Only ASCII characters between 33 and 126 are allowed (basic latin, no spaces) and length between 1 and 22
    /// <a href="https://wiki.multitheftauto.com/wiki/SetPlayerName">See official documentation</a>
    /// </summary>
    public static bool IsValidNickName(this string value) => NickNameRegex().IsMatch(value);
}
