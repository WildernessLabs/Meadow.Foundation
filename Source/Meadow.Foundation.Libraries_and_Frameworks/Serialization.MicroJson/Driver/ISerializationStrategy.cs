using System;

namespace Meadow.Foundation.Serialization;

/// <summary>
/// Interface for Unit type JSON converters that handle serialization and deserialization
/// </summary>
public interface IUnitJsonConverter
{
    /// <summary>
    /// Serializes a Unit object to a JSON string
    /// </summary>
    /// <param name="obj">The object to serialize</param>
    /// <param name="convertNamesToCamelCase">Whether to convert property names to camel case</param>
    /// <returns>A JSON string representing the Unit object, or null if the object is not a supported Unit type</returns>
    string? Serialize(object obj, bool convertNamesToCamelCase = true);

    /// <summary>
    /// Deserializes a JSON string to an object of type T
    /// </summary>
    /// <typeparam name="T">The expected type (Unit type or array of Unit type)</typeparam>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>A deserialized object of type T, or default value if deserialization fails</returns>
    T? Deserialize<T>(string json);

    /// <summary>
    /// Deserializes a JSON string to an object of the specified type
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="targetType">The expected object type</param>
    /// <returns>A deserialized object, or null if deserialization fails</returns>
    object? Deserialize(string json, Type targetType);
}