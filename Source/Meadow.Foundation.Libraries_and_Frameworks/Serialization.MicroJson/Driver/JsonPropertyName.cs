using System;

namespace Meadow.Foundation.Serialization;

/// <summary>
/// Specifies the property name that is present in the JSON when serializing and deserializing.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public class JsonPropertyName : Attribute
{
    /// <summary>
    /// Gets the name of the property.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Initializes a new instance of JsonPropertyNameAttribute with the specified property name.
    /// </summary>
    /// <param name="propertyName">The name of the property</param>
    public JsonPropertyName(string propertyName)
    {
        PropertyName = propertyName;
    }
}