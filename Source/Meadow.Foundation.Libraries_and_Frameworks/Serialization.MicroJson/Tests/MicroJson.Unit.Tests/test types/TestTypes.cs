using Meadow.Foundation.Serialization;

namespace Unit.Tests;

internal class IntegerClass
{
    public int Value { get; set; }
}

internal class IgnorableContainerClass
{
    public int ValueA { get; set; }
    [JsonIgnore]
    public string? ValueB { get; set; }
    public bool ValueC { get; set; }
}
