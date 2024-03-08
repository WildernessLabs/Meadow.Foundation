using System.Globalization;

namespace Meadow.Foundation.Serialization;

internal static class UInt64Converters
{
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
        if (input.Length >= 2 && input[..2].ToLower() == "0x")
        {
            input = input[2..];
            parseHex = true;
        }

        char character;
        bool noOverflow = true;
        result = 0;

        // Skip leading white space.
        int len = input.Length;
        int posn = 0;
        while (posn < len && char.IsWhiteSpace(input[posn]))
        {
            posn++;
        }

        // Check for leading sign information.
        NumberFormatInfo nfi = CultureInfo.CurrentUICulture.NumberFormat;
        string posSign = nfi.PositiveSign;
        string negSign = nfi.NegativeSign;
        sign = false;
        while (posn < len)
        {
            character = input[posn];
            if (!parseHex && character == negSign[0])
            {
                sign = true;
                ++posn;
            }
            else if (!parseHex && character == posSign[0])
            {
                sign = false;
                ++posn;
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

        //if the string is empty
        if (posn >= len)
        {
            return false;
        }

        uint low = 0;
        uint high = 0;
        uint digit;
        ulong tempa, tempb;
        if (parseHex)
        {
            #region Parse a hexadecimal value.
            do
            {
                // Get the next digit from the string.
                character = input[posn];
                if (character >= '0' && character <= '9')
                {
                    digit = (uint)(character - '0');
                }
                else if (character >= 'A' && character <= 'F')
                {
                    digit = (uint)(character - 'A' + 10);
                }
                else if (character >= 'a' && character <= 'f')
                {
                    digit = (uint)(character - 'a' + 10);
                }
                else
                {
                    break;
                }

                // Combine the digit with the result, and check for overflow.
                if (noOverflow)
                {
                    tempa = ((ulong)low) * ((ulong)16);
                    tempb = ((ulong)high) * ((ulong)16);
                    tempb += (tempa >> 32);
                    if (tempb > ((ulong)0xFFFFFFFF))
                    {
                        // Overflow has occurred.
                        noOverflow = false;
                    }
                    else
                    {
                        tempa = (tempa & 0xFFFFFFFF) + ((ulong)digit);
                        tempb += (tempa >> 32);
                        if (tempb > ((ulong)0xFFFFFFFF))
                        {
                            // Overflow has occurred.
                            noOverflow = false;
                        }
                        else
                        {
                            low = unchecked((uint)tempa);
                            high = unchecked((uint)tempb);
                        }
                    }
                }
                ++posn; // Advance to the next character.
            } while (posn < len);
            #endregion
        }
        else
        {
            #region Parse a decimal value.
            do
            {
                // Get the next digit from the string.
                character = input[posn];
                if (character >= '0' && character <= '9')
                {
                    digit = (uint)(character - '0');
                }
                else
                {
                    break;
                }

                // Combine the digit with the result, and check for overflow.
                if (noOverflow)
                {
                    tempa = ((ulong)low) * ((ulong)10);
                    tempb = ((ulong)high) * ((ulong)10);
                    tempb += (tempa >> 32);
                    if (tempb > ((ulong)0xFFFFFFFF))
                    {
                        // Overflow has occurred.
                        noOverflow = false;
                    }
                    else
                    {
                        tempa = (tempa & 0xFFFFFFFF) + ((ulong)digit);
                        tempb += (tempa >> 32);
                        if (tempb > ((ulong)0xFFFFFFFF))
                        {
                            // Overflow has occurred.
                            noOverflow = false;
                        }
                        else
                        {
                            low = unchecked((uint)tempa);
                            high = unchecked((uint)tempb);
                        }
                    }
                }
                ++posn;// Advance to the next character.
            } while (posn < len);
            #endregion
        }

        // Process trailing white space.
        if (posn < len)
        {
            do
            {
                character = input[posn];
                if (char.IsWhiteSpace(character))
                    ++posn;
                else
                    break;
            } while (posn < len);

            if (posn < len)
            {
                return false;
            }
        }

        // Return the results to the caller.
        result = (((ulong)high) << 32) | ((ulong)low);
        return noOverflow;
    }
}