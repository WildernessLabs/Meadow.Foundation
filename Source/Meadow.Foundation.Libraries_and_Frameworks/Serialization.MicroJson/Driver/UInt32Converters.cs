using System;

namespace Meadow.Foundation.Serialization;

internal enum NumberStyle
{
    Decimal = 1,
    Hexadecimal
}

internal static class UInt32Converters
{
    public static bool TryParse(string str, NumberStyle style, out UInt32 result)
    {
        bool bresult = UInt64Converters.TryParse(str, style == NumberStyle.Hexadecimal, out ulong tmp, out bool sign);
        result = (UInt32)tmp;

        return bresult && !sign;
    }

    /// <summary>
    /// Converts a Unicode character to a string of its ASCII equivalent.
    /// Very simple, it works only on ordinary characters.
    /// </summary>
    /// <param name="codePoint">The Unicode code point.</param>
    /// <returns>The string representation of the ASCII character.</returns>
    public static string ConvertUnicodeToAsciiString(int codePoint)
    {
        return new string((char)codePoint, 1);
    }
}