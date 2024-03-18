using System;

namespace Meadow.Foundation.Serialization;

[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public class JsonIgnoreAttribute : Attribute
{
}
