﻿using Meadow.Foundation.Serialization;
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
