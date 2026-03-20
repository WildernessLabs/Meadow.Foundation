using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using static Meadow.Foundation.Serialization.MicroJson;

namespace Meadow.Foundation.Serialization;

/// <summary>
/// Options for controlling JSON serialization behavior.
/// </summary>
public class SerializerOptions
{
    /// <summary>
    /// Gets or sets whether to omit properties with null values from the JSON output.
    /// Default is false.
    /// </summary>
    public bool OmitNulls { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to format the JSON output with indentation and line breaks.
    /// Default is false (compact output).
    /// </summary>
    public bool WriteIndented { get; set; } = false;

    /// <summary>
    /// Gets or sets the string used for indentation when WriteIndented is true.
    /// Default is two spaces.
    /// </summary>
    public string IndentString { get; set; } = "  ";

    /// <summary>
    /// Gets or sets whether to convert property names to camelCase during serialization.
    /// Default is true.
    /// </summary>
    public bool ConvertNamesToCamelCase { get; set; } = true;

    /// <summary>
    /// Gets or sets the format to use for DateTime values.
    /// Default is ISO8601.
    /// </summary>
    public DateTimeFormat DateTimeFormat { get; set; } = DateTimeFormat.ISO8601;

    /// <summary>
    /// Gets the default serializer options with standard settings.
    /// </summary>
    public static SerializerOptions Default => new SerializerOptions();
}

/// <summary>
/// JSON Serialization and Deserialization library for .NET
/// </summary>
public static partial class MicroJson
{
    private static string[] ExplicitlyUnsupportedTypes =
    {
        "System.Text.Json.JsonElement"
    };

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
    /// Converts an object to a JSON string using the specified options.
    /// </summary>
    /// <param name="o">The value to convert.</param>
    /// <param name="options">The serialization options to use.</param>
    /// <returns>The JSON object as a string or null when the value type is not supported.</returns>
    public static string? Serialize(object o, SerializerOptions? options = null)
    {
        return Serialize(o, options, null);
    }

    /// <summary>
    /// Converts an object to a JSON string using the specified options.
    /// </summary>
    /// <param name="o">The value to convert.</param>
    /// <param name="options">The serialization options to use.</param>
    /// <param name="unitJsonConverter">An optional serialization converter used for Meadow.Units</param>
    /// <returns>The JSON object as a string or null when the value type is not supported.</returns>
    /// <remarks>For objects, only public properties with getters are converted.</remarks>
    public static string? Serialize(object o, SerializerOptions? options, IUnitJsonConverter? unitJsonConverter = null)
    {
        options ??= SerializerOptions.Default;
        return SerializeInternal(o, options, unitJsonConverter, 0);
    }

    /// <summary>
    /// Internal serialization method that handles the actual serialization logic.
    /// </summary>
    private static string? SerializeInternal(object o, SerializerOptions options, IUnitJsonConverter? unitJsonConverter, int indentLevel)
    {
        if (o == null)
        {
            return "null";
        }

        if (unitJsonConverter != null)
        {
            var result = unitJsonConverter.Serialize(o, options.ConvertNamesToCamelCase);
            if (result != null)
            {
                return result;
            }
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
                return options.DateTimeFormat switch
                {
                    DateTimeFormat.Ajax => $"\"{DateTimeConverters.ToASPNetAjax((DateTime)o)}\"",
                    _ => $"\"{DateTimeConverters.ToIso8601((DateTime)o)}\"",
                };
            default:
                if (type == typeof(DateTimeOffset))
                {
                    return options.DateTimeFormat switch
                    {
                        DateTimeFormat.Ajax => $"\"{DateTimeConverters.ToASPNetAjax((DateTimeOffset)o)}\"",
                        _ => $"\"{DateTimeConverters.ToIso8601((DateTimeOffset)o)}\"",
                    };
                }
                if (type == typeof(Guid) || type == typeof(TimeSpan))
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
            return SerializeIDictionary(dictionary, options, indentLevel);
        }

        if (o is IEnumerable enumerable)
        {
            return SerializeIEnumerable(enumerable, options, indentLevel);
        }

        if (o is DictionaryEntry entry)
        {
            var hashtable = new Hashtable
            {
                { entry.Key, entry.Value }
            };
            return SerializeIDictionary(hashtable, options, indentLevel);
        }

        if (type.IsClass || type.IsValueType && !ExplicitlyUnsupportedTypes.Any(e => e == type.FullName))
        {

            var hashtable = new Hashtable();

            // Use PropertyInfo instead of MethodInfo for better performance
            var properties = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttributes(typeof(JsonIgnoreAttribute), true).Length == 0);

            foreach (PropertyInfo property in properties)
            {
                object returnObject = property.GetValue(o);

                // Skip null properties if OmitNulls is enabled
                if (options.OmitNulls && returnObject == null)
                {
                    continue;
                }

                var mappedName = property.GetCustomAttribute<JsonPropertyNameAttribute>(true);

                var name = mappedName != null
                    ? mappedName.PropertyName
                    : options.ConvertNamesToCamelCase
                        ? char.ToLowerInvariant(property.Name[0]) + property.Name[1..]
                        : property.Name;

                hashtable.Add(name, returnObject);
            }

            return SerializeIDictionary(hashtable, options, indentLevel);
        }

        throw new NotSupportedException($"Serialization of type {type.Name} is not supported");
    }

    /// <summary>
    /// Converts an IEnumerable to a JSON string.
    /// </summary>
    /// <param name="enumerable">The IEnumerable to convert.</param>
    /// <param name="options">The serialization options to use.</param>
    /// <param name="indentLevel">The current indentation level.</param>
    /// <returns>The JSON array as a string or null when the value type is not supported.</returns>
    private static string SerializeIEnumerable(IEnumerable enumerable, SerializerOptions options, int indentLevel)
    {
        var result = new StringBuilder("[");
        var elements = new List<string>();

        // Collect all elements first to avoid comma issues
        foreach (object current in enumerable)
        {
            var serializedValue = SerializeInternal(current, options, null, indentLevel + 1);
            
            var elementString = new StringBuilder();
            
            if (options.WriteIndented)
            {
                elementString.Append(GetIndentString(options.IndentString, indentLevel + 1));
            }
            
            elementString.Append(serializedValue);
            
            elements.Add(elementString.ToString());
        }

        // Join elements with commas
        if (elements.Count > 0)
        {
            if (options.WriteIndented)
            {
                result.AppendLine();
                result.Append(string.Join(",\n", elements));
                result.AppendLine();
                result.Append(GetIndentString(options.IndentString, indentLevel));
            }
            else
            {
                result.Append(string.Join(",", elements));
            }
        }

        result.Append("]");
        return result.ToString();
    }

    /// <summary>
    /// Converts an IDictionary to a JSON string.
    /// </summary>
    /// <param name="dictionary">The IDictionary to convert.</param>
    /// <param name="options">The serialization options to use.</param>
    /// <param name="indentLevel">The current indentation level.</param>
    /// <returns>The JSON object as a string or null when the value type is not supported.</returns>
    private static string SerializeIDictionary(IDictionary dictionary, SerializerOptions options, int indentLevel)
    {
        var result = new StringBuilder("{");
        var entries = new List<string>();

        // Collect all valid entries first to avoid comma issues
        foreach (DictionaryEntry entry in dictionary)
        {
            var serializedValue = SerializeInternal(entry.Value, options, null, indentLevel + 1);
            
            // Skip null values if OmitNulls is enabled
            if (options.OmitNulls && serializedValue == "null")
            {
                continue;
            }

            var entryString = new StringBuilder();
            
            if (options.WriteIndented)
            {
                entryString.Append(GetIndentString(options.IndentString, indentLevel + 1));
            }

            entryString.Append($"\"{entry.Key}\":");
            
            if (options.WriteIndented)
            {
                entryString.Append(" ");
            }
            
            entryString.Append(serializedValue);
            
            entries.Add(entryString.ToString());
        }

        // Join entries with commas
        if (entries.Count > 0)
        {
            if (options.WriteIndented)
            {
                result.AppendLine();
                result.Append(string.Join(",\n", entries));
                result.AppendLine();
                result.Append(GetIndentString(options.IndentString, indentLevel));
            }
            else
            {
                result.Append(string.Join(",", entries));
            }
        }

        result.Append("}");
        return result.ToString();
    }

    /// <summary>
    /// Gets the indentation string for the specified level.
    /// </summary>
    /// <param name="indentString">The string to use for each indentation level.</param>
    /// <param name="level">The indentation level.</param>
    /// <returns>The indentation string.</returns>
    private static string GetIndentString(string indentString, int level)
    {
        if (level <= 0) return string.Empty;

        var result = new StringBuilder();
        for (int i = 0; i < level; i++)
        {
            result.Append(indentString);
        }
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