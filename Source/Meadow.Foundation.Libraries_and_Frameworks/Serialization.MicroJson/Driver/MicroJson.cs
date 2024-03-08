using System;
using System.Collections;
using System.Reflection;
using System.Text;

namespace Meadow.Foundation.Serialization;

/// <summary>
/// JSON Serialization and Deserialization library for .NET
/// </summary>
public static partial class MicroJson
{
    /// <summary>
    /// Converts an object to a JSON string.
    /// </summary>
    /// <param name="o">The value to convert. Supported types are: Boolean, String, byte, sbyte, (U)Int16, (U)Int32, Float, Double, Decimal, Array, IDictionary, IEnumerable, Guid, DateTime, DictionaryEntry, Object, and null.</param>
    /// <param name="dateTimeFormat">The format to use for DateTime values. Defaults to ISO 8601 format.</param>
    /// <returns>The JSON object as a string or null when the value type is not supported.</returns>
    /// <remarks>For objects, only public properties with getters are converted.</remarks>
    public static string? Serialize(object o, DateTimeFormat dateTimeFormat = DateTimeFormat.ISO8601)
    {
        return SerializeObject(o, dateTimeFormat);
    }

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
    /// Convert an object to a JSON string.
    /// </summary>
    /// <param name="o">The value to convert. Supported types are: Boolean, String, Byte, (U)Int16, (U)Int32, Float, Double, Decimal, Array, IDictionary, IEnumerable, Guid, Datetime, DictionaryEntry, Object and null.</param>
    /// <param name="dateTimeFormat">The format to use for DateTime values. Defaults to ISO 8601 format.</param>
    /// <returns>The JSON object as a string or null when the value type is not supported.</returns>
    /// <remarks>For objects, only public properties with getters are converted.</remarks>
    public static string? SerializeObject(object o, DateTimeFormat dateTimeFormat = DateTimeFormat.ISO8601)
    {
        if (o == null)
        {
            return "null";
        }

        Type type = o.GetType();

        if (type.IsEnum)
        {   // Serialize enum values by converting them integers
            return $"{(int)o}";
        }

        switch (type.Name)
        {
            case "Boolean":
                return (bool)o ? "true" : "false";
            case "String":
            case "Char":
            case "Guid":
                return "\"" + o.ToString() + "\"";
            case "Single":
            case "Double":
            case "Decimal":
            case "Float":
            case "Byte":
            case "SByte":
            case "Int16":
            case "UInt16":
            case "Int32":
            case "UInt32":
            case "Int64":
            case "UInt64":
                return o.ToString();
            case "DateTime":
                return dateTimeFormat switch
                {   // This MSDN page describes the problem with JSON dates: http://msdn.microsoft.com/en-us/library/bb299886.aspx
                    DateTimeFormat.Ajax => "\"" + DateTimeConverters.ToASPNetAjax((DateTime)o) + "\"",
                    _ => "\"" + DateTimeConverters.ToIso8601((DateTime)o) + "\"",
                };
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

            // Iterate through all of the methods, looking for public GET properties
            MethodInfo[] methods = type.GetMethods();
            foreach (MethodInfo method in methods)
            {
                // We care only about property getters when serializing
                if (method.Name.StartsWith("get_"))
                {
                    // Ignore abstract and virtual objects
                    if (method.IsAbstract)
                    {
                        continue;
                    }

                    // Ignore delegates and MethodInfos
                    if ((method.ReturnType == typeof(Delegate)) ||
                        (method.ReturnType == typeof(MulticastDelegate)) ||
                        (method.ReturnType == typeof(MethodInfo)))
                    {
                        continue;
                    }
                    // Ditto for DeclaringType
                    if ((method.DeclaringType == typeof(Delegate)) ||
                        (method.DeclaringType == typeof(MulticastDelegate)))
                    {
                        continue;
                    }

                    object returnObject = method.Invoke(o, null);
                    hashtable.Add(method.Name.Substring(4), returnObject);
                }
            }
            return SerializeIDictionary(hashtable, dateTimeFormat);
        }

        return null;
    }

    /// <summary>
    /// Convert an IEnumerable to a JSON string.
    /// </summary>
    /// <param name="enumerable">The value to convert.</param>
    /// <param name="dateTimeFormat">The format to use for DateTime values. Defaults to ISO 8601 format.</param>
    /// <returns>The JSON object as a string or null when the value type is not supported.</returns>
    private static string SerializeIEnumerable(IEnumerable enumerable, DateTimeFormat dateTimeFormat = DateTimeFormat.ISO8601)
    {
        var result = new StringBuilder("[");

        foreach (object current in enumerable)
        {
            if (result.Length > 1)
            {
                result.Append(",");
            }

            result.Append(SerializeObject(current, dateTimeFormat));
        }

        result.Append("]");
        return result.ToString();
    }

    /// <summary>
    /// Convert an IDictionary to a JSON string.
    /// </summary>
    /// <param name="dictionary">The value to convert.</param>
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

            result.Append("\"" + entry.Key + "\"");
            result.Append(":");
            result.Append(SerializeObject(entry.Value, dateTimeFormat));
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
        // If the string is just fine (most are) then make a quick exit for improved performance
        if (input.IndexOf('\\') < 0 && input.IndexOf('\"') < 0)
        {
            return input;
        }

        // Build a new string
        var result = new StringBuilder(input.Length + 1); // we know there is at least 1 char to escape
        foreach (char ch in input.ToCharArray())
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