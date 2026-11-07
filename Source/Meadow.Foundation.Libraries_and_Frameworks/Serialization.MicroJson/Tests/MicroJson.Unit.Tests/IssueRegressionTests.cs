using Meadow.Foundation.Serialization;
using System;
using System.Collections.Generic;
using Xunit;

namespace Unit.Tests;

public class IssueRegressionTests
{
    public class ListHolder
    {
        public string Name { get; set; } = "";
        public List<ListItem>? Items { get; set; }
        public List<string>? Tags { get; set; }
        public Dictionary<string, string>? Map { get; set; }
    }

    public class ListItem
    {
        public string Name { get; set; } = "";
    }

    public class DateHolder
    {
        public DateTime When { get; set; }
        public DateTime? MaybeWhen { get; set; }
    }

    [Fact]
    public void NullListsDeserializeAsNull()
    {
        var json = "{\"Name\":\"Item 1\",\"Items\":null,\"Tags\":null,\"Map\":null}";

        var result = MicroJson.Deserialize<ListHolder>(json);

        Assert.Equal("Item 1", result.Name);
        Assert.Null(result.Items);
        Assert.Null(result.Tags);
        Assert.Null(result.Map);
    }

    [Fact]
    public void UtcDateTimeRoundTrips()
    {
        var original = new DateHolder { When = new DateTime(2026, 11, 7, 20, 15, 30, 123, DateTimeKind.Utc) };
        original.MaybeWhen = original.When;

        var json = MicroJson.Serialize(original)!;
        var result = MicroJson.Deserialize<DateHolder>(json);

        Assert.Equal(original.When, result.When);
        Assert.Equal(DateTimeKind.Utc, result.When.Kind);
        Assert.Equal(original.When, result.MaybeWhen);
    }

    [Fact]
    public void OffsetDateTimeDeserializesToSameInstant()
    {
        var json = "{\"When\":\"2026-11-07T12:15:30.0000000-08:00\"}";

        var result = MicroJson.Deserialize<DateHolder>(json);

        Assert.Equal(new DateTime(2026, 11, 7, 20, 15, 30, DateTimeKind.Utc), result.When.ToUniversalTime());
    }
}
