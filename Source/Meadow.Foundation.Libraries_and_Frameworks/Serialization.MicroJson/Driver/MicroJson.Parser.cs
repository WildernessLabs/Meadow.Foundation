using System;
using System.Collections;
using System.Globalization;
using System.Text;

namespace Meadow.Foundation.Serialization;

/// <summary>
/// Parses JSON strings into a Hashtable, mapping property names to their values. 
/// Values can be Hashtables (for nested objects), ArrayLists (for collections), 
/// or value types (e.g., string, int, bool, Guid, DateTime).
/// </summary>
internal class Parser
{
    protected enum Token
    {
        None = 0,
        ObjectBegin,                // {
        ObjectEnd,                  // }
        ArrayBegin,                 // [
        ArrayEnd,                   // ]
        PropertySeparator,          // :
        ItemsSeparator,             // ,
        StringType,                 // "  <-- string of characters
        NumberType,                 // 0-9  <-- number, fixed or floating point
        BooleanTrue,                // true
        BooleanFalse,               // false
        NullType                    // null
    }

    /// <summary>
    /// Parses the string json into a value
    /// </summary>
    /// <param name="json">A JSON string.</param>
    /// <returns>An ArrayList, a Hashtable, a double, long, a string, null, true, or false</returns>
    public static object? JsonDecode(string json)
    {
        bool success = true;

        return JsonDecode(json, ref success);
    }

    /// <summary>
    /// Parses the string json into a value
    /// </summary>
    /// <param name="json">A JSON string.</param>
    /// <param name="success">true if successfu;</param>
    /// <returns>The decoded object</returns>
    public static object? JsonDecode(string json, ref bool success)
    {
        success = false;

        if (json != null)
        {
            char[] charArray = json.ToCharArray();
            int index = 0;
            object? value = ParseValue(charArray, ref index, ref success);
            return value;
        }
        return null;
    }

    protected static Hashtable? ParseObject(char[] json, ref int index, ref bool success)
    {
        var table = new Hashtable();
        Token token;

        // {
        NextToken(json, ref index);

        bool done = false;
        while (!done)
        {
            token = LookAhead(json, index);
            if (token == Token.None)
            {
                success = false;
                return null;
            }
            else if (token == Token.ItemsSeparator)
            {
                NextToken(json, ref index);
            }
            else if (token == Token.ObjectEnd)
            {
                NextToken(json, ref index);
                return table;
            }
            else
            {   // name
                string? name = ParseString(json, ref index, ref success);
                if (!success)
                {
                    success = false;
                    return null;
                }

                // :
                token = NextToken(json, ref index);
                if (token != Token.PropertySeparator)
                {
                    success = false;
                    return null;
                }

                // value
                object? value = ParseValue(json, ref index, ref success);
                if (!success)
                {
                    success = false;
                    return null;
                }

                table[name] = value;
            }
        }

        return table;
    }

    protected static ArrayList? ParseArray(char[] json, ref int index, ref bool success)
    {
        var array = new ArrayList();

        // [
        NextToken(json, ref index);

        bool done = false;
        while (!done)
        {
            Token token = LookAhead(json, index);
            if (token == Token.None)
            {
                success = false;
                return null;
            }
            else if (token == Token.ItemsSeparator)
            {
                NextToken(json, ref index);
            }
            else if (token == Token.ArrayEnd)
            {
                NextToken(json, ref index);
                break;
            }
            else
            {
                object? value = ParseValue(json, ref index, ref success);
                if (!success)
                {
                    return null;
                }

                array.Add(value);
            }
        }

        return array;
    }

    protected static object? ParseValue(char[] json, ref int index, ref bool success)
    {
        switch (LookAhead(json, index))
        {
            case Token.StringType:
                return ParseString(json, ref index, ref success);
            case Token.NumberType:
                return ParseNumber(json, ref index, ref success);
            case Token.ObjectBegin:
                return ParseObject(json, ref index, ref success);
            case Token.ArrayBegin:
                return ParseArray(json, ref index, ref success);
            case Token.BooleanTrue:
                NextToken(json, ref index);
                return true;
            case Token.BooleanFalse:
                NextToken(json, ref index);
                return false;
            case Token.NullType:
                NextToken(json, ref index);
                return null;
            case Token.None:
                break;
        }

        success = false;
        return null;
    }

    protected static string? ParseString(char[] json, ref int index, ref bool success)
    {
        var s = new StringBuilder();

        success = true;

        AdvanceIndexPastWhitespace(json, ref index);

        // "
        char c;
        bool complete = false;

        index++;
        while (!complete)
        {
            if (index == json.Length)
            {
                break;
            }

            c = json[index++];
            if (c == '"')
            {
                complete = true;
                break;
            }
            else if (c == '\\')
            {
                if (index == json.Length)
                {
                    break;
                }

                c = json[index++];
                if (c == '"')
                {
                    s.Append('"');
                }
                else if (c == '\\')
                {
                    s.Append('\\');
                }
                else if (c == '/')
                {
                    s.Append('/');
                }
                else if (c == 'b')
                {
                    s.Append('\b');
                }
                else if (c == 'f')
                {
                    s.Append('\f');
                }
                else if (c == 'n')
                {
                    s.Append('\n');
                }
                else if (c == 'r')
                {
                    s.Append('\r');
                }
                else if (c == 't')
                {
                    s.Append('\t');
                }
                else if (c == 'u')
                {
                    int remainingLength = json.Length - index;
                    if (remainingLength >= 4)
                    {
                        // parse the 32 bit hex into an integer codepoint
                        if (!(success = UInt32Converters.TryParse(new string(json, index, 4), NumberStyle.Hexadecimal, out uint codePoint)))
                        {
                            return string.Empty;
                        }

                        // convert the integer codepoint to a unicode char and add to string
                        s.Append(UInt32Converters.ConvertUnicodeToAsciiString((int)codePoint));

                        // skip 4 chars
                        index += 4;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                s.Append(c);
            }
        }

        if (!complete)
        {
            success = false;
            return null;
        }

        return s.ToString();
    }

    /// <summary>
    /// Determines the type of number (int, double, etc) and returns an object
    /// containing that value.
    /// </summary>
    /// <param name="json"></param>
    /// <param name="index"></param>
    /// <param name="success"></param>
    /// <returns>The object</returns>
    protected static object? ParseNumber(char[] json, ref int index, ref bool success)
    {
        AdvanceIndexPastWhitespace(json, ref index);

        int lastIndex = GetLastIndexOfNumber(json, index);
        int charLength = (lastIndex - index) + 1;
        var value = new string(json, index, charLength);

        index = lastIndex + 1;
        success = true;

        string dot = CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator;
        string comma = CultureInfo.CurrentUICulture.NumberFormat.NumberGroupSeparator;

        // Detecting and parsing the number based on its characteristics
        if (value.Contains(dot) || value.Contains(comma) || value.Contains("e") || value.Contains("E"))
        {
            // Parse as a double for floating-point numbers
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double dblValue))
            {
                return dblValue;
            }
        }
        else if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase) || value.IndexOfAny(new[] { 'a', 'b', 'c', 'd', 'e', 'f', 'A', 'B', 'C', 'D', 'E', 'F' }) >= 0)
        {
            // Parse as a hexadecimal number
            if (long.TryParse(value.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out long hexValue))
            {
                return hexValue;
            }
        }
        else
        {
            // Parse as a long integer for decimal numbers
            if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long intValue))
            {
                return intValue;
            }
        }

        success = false;
        return null;
    }

    protected static int GetLastIndexOfNumber(char[] json, int index)
    {
        int lastIndex;

        for (lastIndex = index; lastIndex < json.Length; lastIndex++)
        {
            if ("0123456789+-.eE".IndexOf(json[lastIndex]) == -1)
            {
                break;
            }
        }
        return lastIndex - 1;
    }

    /// <summary>
    /// Advances the index past any whitespace characters in the JSON data.
    /// </summary>
    /// <param name="json">The JSON data.</param>
    /// <param name="index">The current index in the JSON data.</param>
    protected static void AdvanceIndexPastWhitespace(char[] json, ref int index)
    {
        for (; index < json.Length; index++)
        {
            if (!char.IsWhiteSpace(json[index]))
            {
                break;
            }
        }
    }

    protected static Token LookAhead(char[] json, int index)
    {
        int saveIndex = index;
        return NextToken(json, ref saveIndex);
    }

    protected static Token NextToken(char[] json, ref int index)
    {
        AdvanceIndexPastWhitespace(json, ref index);

        if (index == json.Length)
        {
            return Token.None;
        }

        char nextCharacter = json[index];
        index++;

        switch (nextCharacter)
        {
            case '{':
                return Token.ObjectBegin;
            case '}':
                return Token.ObjectEnd;
            case '[':
                return Token.ArrayBegin;
            case ']':
                return Token.ArrayEnd;
            case ',':
                return Token.ItemsSeparator;
            case '"':
                return Token.StringType;
            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
            case '-':
                return Token.NumberType;
            case ':':
                return Token.PropertySeparator;
        }
        index--;

        int remainingLength = json.Length - index;

        // false
        if (remainingLength >= 5)
        {
            if (json[index] == 'f' &&
                json[index + 1] == 'a' &&
                json[index + 2] == 'l' &&
                json[index + 3] == 's' &&
                json[index + 4] == 'e')
            {
                index += 5;
                return Token.BooleanFalse;
            }
        }

        // true
        if (remainingLength >= 4)
        {
            if (json[index] == 't' &&
                json[index + 1] == 'r' &&
                json[index + 2] == 'u' &&
                json[index + 3] == 'e')
            {
                index += 4;
                return Token.BooleanTrue;
            }
        }

        // null
        if (remainingLength >= 4)
        {
            if (json[index] == 'n' &&
                json[index + 1] == 'u' &&
                json[index + 2] == 'l' &&
                json[index + 3] == 'l')
            {
                index += 4;
                return Token.NullType;
            }
        }

        return Token.None;
    }
}