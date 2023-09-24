﻿using System.Drawing;

namespace SlipeServer.Server.Extensions;

public static class ColorExtensions
{
    /// <summary>
    /// Returns the HTML color code for a certain color, for example #ff0000 for red
    /// </summary>
    public static string ToColorCode(this Color? color)
    {
        if (color == null)
            return "#ffffff";

        return $"#{color.Value.R:X2}{color.Value.G:X2}{color.Value.B:X2}";
    }

    /// <summary>
    /// Returns the retrieves the hex number of a specified color, useful for the dx functions. Equivalent of lua tocolor function
    /// </summary>
    public static double ToLuaColor(this Color color) => color.B + color.G * 256 + color.R * 256 * 256 + color.A * 256 * 256 * 256;
}
