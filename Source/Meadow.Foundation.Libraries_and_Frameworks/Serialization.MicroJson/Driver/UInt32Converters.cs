using System;

namespace Meadow.Foundation.Serialization;

/// <summary>
/// Provides utility methods for converting and parsing UInt32 values.
/// </summary>
internal static class UInt32Converters
{
    /// <summary>
    /// Tries to parse a string representation of a UInt32 value with the specified number style.
    /// </summary>
    /// <param name="str">The string to parse.</param>
    /// <param name="style">The number style to use during parsing.</param>
    /// <param name="result">When the method returns, contains the parsed UInt32 value.</param>
    /// <returns><c>true</c> if the parsing was successful; otherwise, <c>false</c>.</returns>
    public static bool TryParse(string str, NumberStyle style, out UInt32 result)
    {
        bool parsingResult = UInt64Converters.TryParse(str, style == NumberStyle.Hexadecimal, out ulong tmp, out bool sign);
        result = (UInt32)tmp;

        return parsingResult && !sign;
    }

    /// <summary>
    /// Converts a Unicode character to a string of its ASCII equivalent.
    /// This method is suitable for ordinary characters.
    /// </summary>
    /// <param name="codePoint">The Unicode code point.</param>
    /// <returns>The string representation of the ASCII character.</returns>
    public static string ConvertUnicodeToAsciiString(int codePoint)
    {
        return new string((char)codePoint, 1);
    }
}