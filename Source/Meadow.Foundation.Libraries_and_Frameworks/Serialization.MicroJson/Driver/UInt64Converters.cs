using System;
using System.Globalization;

namespace Meadow.Foundation.Serialization;

internal static class UInt64Converters
{
    /// <summary>
    /// The maximum number of digits supported for parsing double values.
    /// </summary>
    public const int MaxDoubleDigits = 16;

    /// <summary>
    /// Parse integer values using localized number format information.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="parseHex"></param>
    /// <param name="result"></param>
    /// <param name="sign"></param>
    /// <returns>true if succesful</returns>
    public static bool TryParse(string input, bool parseHex, out ulong result, out bool sign)
    {
        // Check for hexadecimal prefix
        if (input.Length >= 2 && input.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            input = input[2..];
            parseHex = true;
        }

        char character;
        bool isOverflow = false;
        result = 0;

        int length = input.Length;
        int currentIndex = 0;
        while (currentIndex < length && char.IsWhiteSpace(input[currentIndex]))
        {
            currentIndex++;
        }

        NumberFormatInfo nfi = CultureInfo.CurrentUICulture.NumberFormat;
        string posSign = nfi.PositiveSign;
        string negSign = nfi.NegativeSign;
        sign = false;

        while (currentIndex < length)
        {
            character = input[currentIndex];
            if (!parseHex && (character == negSign[0] || character == posSign[0]))
            {
                sign = character == negSign[0];
                ++currentIndex;
            }
            else if ((parseHex && ((character >= 'A' && character <= 'F') || (character >= 'a' && character <= 'f'))) ||
                     (character >= '0' && character <= '9'))
            {
                break;
            }
            else
            {
                return false;
            }
        }

        if (currentIndex >= length)
        {
            return false;
        }

        uint low = 0;
        uint high = 0;
        uint digit;
        ulong lowPart, highPart;

        // Parse the value based on the selected format
        do
        {
            character = input[currentIndex];
            if ((character >= '0' && character <= '9') || (parseHex && ((character >= 'A' && character <= 'F') || (character >= 'a' && character <= 'f'))))
            {
                digit = parseHex ?
                    (uint)(char.IsDigit(character) ? character - '0' : char.ToUpper(character) - 'A' + 10) :
                    (uint)(character - '0');

                if (!isOverflow)
                {
                    lowPart = low * ((ulong)(parseHex ? 16 : 10));
                    highPart = high * ((ulong)(parseHex ? 16 : 10));
                    highPart += lowPart >> 32;

                    if (highPart > 0xFFFFFFFF)
                    {
                        isOverflow = true;
                    }
                    else
                    {
                        lowPart = (lowPart & 0xFFFFFFFF) + digit;
                        highPart += (lowPart >> 32);

                        if (highPart > 0xFFFFFFFF)
                        {
                            isOverflow = true;
                        }
                        else
                        {
                            low = unchecked((uint)lowPart);
                            high = unchecked((uint)highPart);
                        }
                    }
                }
                currentIndex++;
            }
            else
            {
                break;
            }
        } while (currentIndex < length);

        if (currentIndex < length)
        {
            do
            {
                character = input[currentIndex];
                if (char.IsWhiteSpace(character))
                {
                    ++currentIndex;
                }
                else
                {
                    break;
                }
            } while (currentIndex < length);

            if (currentIndex < length)
            {
                return false;
            }
        }

        result = (((ulong)high) << 32) | low;
        return !isOverflow;
    }
}