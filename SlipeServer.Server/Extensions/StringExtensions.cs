using SlipeServer.Packets;
using SlipeServer.Server.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SlipeServer.Server.Extensions
{
    public static class StringExtensions
    {
        const string colorCodeRegex = "#[0-9a-fA-F]{6}";

        public static string StripColorCode(this string value)
        {
            string temp = value;
            while (Regex.IsMatch(temp, colorCodeRegex))
                temp = Regex.Replace(temp, colorCodeRegex, "");
            return temp;
        }
    }
}
