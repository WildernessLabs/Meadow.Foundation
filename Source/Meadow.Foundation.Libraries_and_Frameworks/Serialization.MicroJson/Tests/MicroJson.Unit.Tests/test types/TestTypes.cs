using Meadow.Foundation.Serialization;
using System;
using System.Collections.Generic;

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

public class StringListClass
{
    public List<string> ListProp { get; set; } = new();
}

public class TestResultMessage2
{
    public Guid? TestID { get; set; }
}

public class TestResultMessage
{
    public Guid? ResultID { get; set; }
    public Guid? TestID { get; set; }
    public string? TestName { get; set; }
    public DateTime? StartedTimestamp { get; set; }
    public DateTime? CompletedTimestamp { get; set; }
    public string? State { get; set; }
    public string? TargetPlatform { get; set; }
    public string? MeadowOSVersion { get; set; }
    public string? TargetInfo { get; set; }
    public string? TestRunBy { get; set; }
    public List<string>? Output { get; set; } = new List<string>();
}
