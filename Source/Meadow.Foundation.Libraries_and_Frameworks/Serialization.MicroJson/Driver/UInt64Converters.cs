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

        // Skip leading white space.
        int length = input.Length;
        int currentIndex = 0;
        while (currentIndex < length && char.IsWhiteSpace(input[currentIndex]))
        {
            currentIndex++;
        }

        // Check for leading sign information.
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

        // If the string is empty
        if (currentIndex >= length)
        {
            return false;
        }

        uint low = 0;
        uint high = 0;
        uint digit;
        ulong tempa, tempb;

        // Parse the value based on the selected format
        do
        {
            character = input[currentIndex];
            if ((character >= '0' && character <= '9') || (parseHex && ((character >= 'A' && character <= 'F') || (character >= 'a' && character <= 'f'))))
            {
                digit = parseHex ?
                    (uint)(char.IsDigit(character) ? character - '0' : char.ToUpper(character) - 'A' + 10) :
                    (uint)(character - '0');

                // Combine the digit with the result, and check for overflow.
                if (!isOverflow)
                {
                    tempa = ((ulong)low) * ((ulong)(parseHex ? 16 : 10));
                    tempb = ((ulong)high) * ((ulong)(parseHex ? 16 : 10));
                    tempb += (tempa >> 32);

                    if (tempb > ((ulong)0xFFFFFFFF))
                    {
                        isOverflow = true;
                    }
                    else
                    {
                        tempa = (tempa & 0xFFFFFFFF) + ((ulong)digit);
                        tempb += (tempa >> 32);

                        if (tempb > ((ulong)0xFFFFFFFF))
                        {
                            isOverflow = true;
                        }
                        else
                        {
                            low = unchecked((uint)tempa);
                            high = unchecked((uint)tempb);
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
                    ++currentIndex;
                else
                    break;
            } while (currentIndex < length);

            if (currentIndex < length)
            {
                return false;
            }
        }

        // Return the results to the caller.
        result = (((ulong)high) << 32) | ((ulong)low);
        return !isOverflow;
    }
}