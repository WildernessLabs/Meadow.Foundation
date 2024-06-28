using Meadow.Foundation.Serialization;
using System;

namespace Unit.Tests;

// {"stringArg":"my string","value":23}
internal class SimpleCommand
{
    public string StringArg { get; set; }
    public int Value { get; set; }
}

internal class StringFieldClass
{
    public string FieldA { get; set; } = string.Empty;
    public string? FieldB { get; set; }
}

internal class DateTimeClass
{
    public DateTime DTField { get; set; }
    public DateTimeOffset DTOField { get; set; }
}

internal class TimeSpanClass
{
    public TimeSpan TSField { get; set; }
}

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

internal class RenamedPropertyClass
{
    [JsonPropertyName("prop_name")]
    public string? Name { get; set; }
    public string? OtherProp { get; set; }
}

public class AuthResponseMessage
{
    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }
    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
}
