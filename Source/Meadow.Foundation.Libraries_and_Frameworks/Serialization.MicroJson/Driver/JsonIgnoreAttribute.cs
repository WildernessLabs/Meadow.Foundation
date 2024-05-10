using System;

namespace Meadow.Foundation.Serialization;

/// <summary>
/// Specifies that a property should be ignored when serializing an object to JSON.
/// </summary>
/// <remarks>
/// This attribute can be applied to properties in a class to indicate that the property
/// should not be included in the JSON representation of the object. This can be useful
/// for properties that do not contain information relevant to the purpose of the serialization,
/// or for properties that might contain sensitive information.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public class JsonIgnoreAttribute : Attribute
{
}