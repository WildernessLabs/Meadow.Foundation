using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Meadow.Foundation.Serialization;

/// <summary>
/// JSON Serialization and Deserialization library for .NET
/// </summary>
public static partial class MicroJson
{
    /// <summary>
    /// Desrializes a Json string into an object.
    /// </summary>
    /// <param name="json"></param>
    /// <returns>An ArrayList, a Hashtable, a double, a long, a string, null, true, or false</returns>
    public static object? Deserialize(string json)
    {
        return DeserializeString(json);
    }

    /// <summary>
    /// Deserializes a Json string into an object.
    /// </summary>
    /// <param name="json"></param>
    /// <returns>An ArrayList, a Hashtable, a double, a long, a string, null, true, or false</returns>
    public static object? DeserializeString(string json)
    {
        return Parser.JsonDecode(json);
    }

    /// <summary>
    /// Escapes special characters in a string to ensure it is JSON-compliant.
    /// </summary>
    /// <param name="value">The string to escape.</param>
    /// <returns>The escaped string with special characters properly encoded.</returns>
    /// <remarks>
    /// This method handles the following special characters:
    /// - Double quotes (") are escaped as \".
    /// - Backslashes (\) are escaped as \\.
    /// - Newlines (\n) are escaped as \\n.
    /// - Carriage returns (\r) are escaped as \\r.
    /// - Tabs (\t) are escaped as \\t.
    /// - Backspaces (\b) are escaped as \\b.
    /// - Form feeds (\f) are escaped as \\f.
    /// </remarks>
    public static string EscapeString(string value)
    {
        return "\"" + value.Replace("\\", "\\\\")
                        .Replace("\"", "\\\"")
                        .Replace("\n", "\\n")
                        .Replace("\r", "\\r")
                        .Replace("\t", "\\t")
                        .Replace("\b", "\\b")
                        .Replace("\f", "\\f") + "\"";
    }

    /// <summary>
    /// Converts an object to a JSON string.
    /// </summary>
    /// <param name="o">The value to convert.</param>
    /// <param name="dateTimeFormat">The format to use for DateTime values. Defaults to ISO 8601 format.</param>
    /// <param name="convertNamesToCamelCase">True to convert all properties to camel case during serialization</param>
    /// <returns>The JSON object as a string or null when the value type is not supported.</returns>
    /// <remarks>For objects, only public properties with getters are converted.</remarks>
    public static string? Serialize(object o, DateTimeFormat dateTimeFormat = DateTimeFormat.ISO8601, bool convertNamesToCamelCase = true)
    {
        if (o == null)
        {
            return "null";
        }

        Type type = o.GetType();

        if (type.IsEnum)
        {
            // Serialize enum values by converting them to integers
            return $"{(int)o}";
        }

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Boolean:
                return (bool)o ? "true" : "false";
            case TypeCode.String:
                return EscapeString((string)o);
            case TypeCode.Char:
                return EscapeString(o.ToString());
            case TypeCode.Single:
            case TypeCode.Double:
            case TypeCode.Decimal:
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.Int32:
            case TypeCode.UInt32:
            case TypeCode.Int64:
            case TypeCode.UInt64:
                if (o is IFormattable formattable)
                {
                    return formattable.ToString(null, System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    return o.ToString();
                }
            case TypeCode.DateTime:
                return dateTimeFormat switch
                {
                    DateTimeFormat.Ajax => $"\"{DateTimeConverters.ToASPNetAjax((DateTime)o)}\"",
                    _ => $"\"{DateTimeConverters.ToIso8601((DateTime)o)}\"",
                };
            default:
                if (type == typeof(DateTimeOffset))
                {
                    return dateTimeFormat switch
                    {
                        DateTimeFormat.Ajax => $"\"{DateTimeConverters.ToASPNetAjax((DateTimeOffset)o)}\"",
                        _ => $"\"{DateTimeConverters.ToIso8601((DateTimeOffset)o)}\"",
                    };
                }
                if (type == typeof(Guid))
                {
                    return $"\"{o}\"";
                }
                else if (type == typeof(Single) || type == typeof(Double) || type == typeof(Decimal) || type == typeof(float))
                {
                    return o.ToString();
                }
                break;
        }

        if (o is IDictionary dictionary && !type.IsArray)
        {
            return SerializeIDictionary(dictionary, dateTimeFormat);
        }

        if (o is IEnumerable enumerable)
        {
            return SerializeIEnumerable(enumerable, dateTimeFormat);
        }

        if (o is DictionaryEntry entry)
        {
            var hashtable = new Hashtable
            {
                { entry.Key, entry.Value }
            };
            return SerializeIDictionary(hashtable, dateTimeFormat);
        }

        if (type.IsClass)
        {
            var hashtable = new Hashtable();

            // Use PropertyInfo instead of MethodInfo for better performance
            var properties = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttributes(typeof(JsonIgnoreAttribute), true).Length == 0);

            foreach (PropertyInfo property in properties)
            {
                object returnObject = property.GetValue(o);
                var name = convertNamesToCamelCase
                    ? char.ToLowerInvariant(property.Name[0]) + property.Name[1..]
                    : property.Name;

                // camel case the name
                hashtable.Add(name, returnObject);
            }

            return SerializeIDictionary(hashtable, dateTimeFormat);
        }

        throw new NotSupportedException($"Serialization of type {type.Name} is not supported");
    }

    /// <summary>
    /// Converts an IEnumerable to a JSON string.
    /// </summary>
    /// <param name="enumerable">The IEnumerable to convert.</param>
    /// <param name="dateTimeFormat">The format to use for DateTime values. Defaults to ISO 8601 format.</param>
    /// <returns>The JSON array as a string or null when the value type is not supported.</returns>
    private static string SerializeIEnumerable(IEnumerable enumerable, DateTimeFormat dateTimeFormat = DateTimeFormat.ISO8601)
    {
        var result = new StringBuilder("[");

        foreach (object current in enumerable)
        {
            if (result.Length > 1)
            {
                result.Append(",");
            }

            result.Append(Serialize(current, dateTimeFormat));
        }

        result.Append("]");
        return result.ToString();
    }

    /// <summary>
    /// Converts an IDictionary to a JSON string.
    /// </summary>
    /// <param name="dictionary">The IDictionary to convert.</param>
    /// <param name="dateTimeFormat">The format to use for DateTime values. Defaults to ISO 8601 format.</param>
    /// <returns>The JSON object as a string or null when the value type is not supported.</returns>
    private static string SerializeIDictionary(IDictionary dictionary, DateTimeFormat dateTimeFormat = DateTimeFormat.ISO8601)
    {
        var result = new StringBuilder("{");

        foreach (DictionaryEntry entry in dictionary)
        {
            if (result.Length > 1)
            {
                result.Append(",");
            }

            result.Append($"\"{entry.Key}\":{Serialize(entry.Value, dateTimeFormat)}");
        }

        result.Append("}");
        return result.ToString();
    }


    /// <summary>
    /// Safely serialize a String into a JSON string value, escaping all backslash and quote characters.
    /// </summary>
    /// <param name="input">The string to serialize.</param>
    /// <returns>The serialized JSON string.</returns>
    public static string SerializeString(string input)
    {
        if (input.IndexOfAny(new[] { '\\', '\"' }) < 0)
        {
            return input;
        }

        var result = new StringBuilder(input.Length + 1); // we know there is at least 1 char to escape
        foreach (char ch in input)
        {
            if (ch == '\\' || ch == '\"')
            {
                result.Append('\\');
            }
            result.Append(ch);
        }
        return result.ToString();
    }
}