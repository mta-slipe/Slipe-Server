using System;
using System.Drawing;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;

namespace SlipeServer.Server.Extensions;

public static class StringExtensions
{
    private const string colorCodeRegex = "#[0-9a-fA-F]{6}";

    /// <summary>
    /// Removes color codes from a string
    /// </summary>
    public static string StripColorCode(this string value)
    {
        string temp = value;
        while (Regex.IsMatch(temp, colorCodeRegex))
            temp = Regex.Replace(temp, colorCodeRegex, "");
        return temp;
    }
    /// <summary>
    /// Calculate and return this strings hash
    /// </summary>
    /// <returns>Returns Hash for this string</returns>
    public static nuint GetHash(this string value)
    {
        UIntPtr a, b, c; // Temporary variables
        int len = value.Length; // Length of the string left

        a = b = 0x9e3779b9;
        c = 0xabcdef89;

        int start = 0;
        int end = 12;


        while (len >= 12)
        {
            var _values = value.ToCharArray(start, end);
            a += (_values[0] + ((uint)_values[1] << 8) + (uint)(_values[2] << 16) + (uint)(_values[3] << 24));
            b += (_values[4] + ((uint)_values[5] << 8) + (uint)(_values[6] << 16) + (uint)(_values[7] << 24));
            c += (_values[8] + ((uint)_values[9] << 8) + (uint)(_values[10] << 16) + (uint)(_values[11] << 24));

            // Mix
            a -= b;
            a -= c;
            a ^= (c >> 13);
            b -= c;
            b -= a;
            b ^= (a << 8);
            c -= a;
            c -= b;
            c ^= (b >> 13);
            a -= b;
            a -= c;
            a ^= (c >> 12);
            b -= c;
            b -= a;
            b ^= (a << 16);
            c -= a;
            c -= b;
            c ^= (b >> 5);
            a -= b;
            a -= c;
            a ^= (c >> 3);
            b -= c;
            b -= a;
            b ^= (a << 10);
            c -= a;
            c -= b;
            c ^= (b >> 15);

            start += 12;
            end += 12;
            len -= 12;
        }

        // Handle the last 11 remaining bytes
        // Note: All cases fall through

        c += (nuint)value.Length;            // Lower byte of c gets used for length

        var values = value.ToCharArray(0, 12);

        switch (len)
        {
            case 11:
                c += ((uint)values[10] << 24);
                goto case 10;
            case 10:
                c += ((uint)values[9] << 16);
                goto case 9;
            case 9:
                c += ((uint)values[8] << 8);
                goto case 8;
            case 8:
                b += ((uint)values[7] << 24);
                goto case 7;
            case 7:
                b += ((uint)values[6] << 16);
                goto case 6;
            case 6:
                b += ((uint)values[5] << 8);
                goto case 5;
            case 5:
                b += values[4];
                goto case 4;
            case 4:
                a += ((uint)values[3] << 24);
                goto case 3;
            case 3:
                a += ((uint)values[2] << 16);
                goto case 2;
            case 2:
                a += ((uint)values[1] << 8);
                goto case 1;
            case 1:
                a += values[0];
                goto default;
            default:
                break;
        }


        // Mix
        a -= b;
        a -= c;
        a ^= (c >> 13);
        b -= c;
        b -= a;
        b ^= (a << 8);
        c -= a;
        c -= b;
        c ^= (b >> 13);
        a -= b;
        a -= c;
        a ^= (c >> 12);
        b -= c;
        b -= a;
        b ^= (a << 16);
        c -= a;
        c -= b;
        c ^= (b >> 5);
        a -= b;
        a -= c;
        a ^= (c >> 3);
        b -= c;
        b -= a;
        b ^= (a << 10);
        c -= a;
        c -= b;
        c ^= (b >> 15);

        return c;
    }
}
