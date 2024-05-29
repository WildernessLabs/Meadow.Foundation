using Meadow.Foundation.Serialization;
using System;
using System.Text.Json;
using Xunit;

namespace Unit.Tests;

public class BasicTests
{
    [Fact]
    public void DateTimeSerializationTest()
    {
        var input = new DateTimeClass
        {
            DTField = DateTime.Now,
            DTOField = DateTimeOffset.UtcNow
        };

        var json = MicroJson.Serialize(input);

        Assert.NotNull(json);
        var test = JsonSerializer.Deserialize<DateTimeClass>(json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        Assert.NotNull(test);
        // the fraction of a second will be lost, so equality won't work
        Assert.True(Math.Abs((input.DTField - test.DTField).TotalSeconds) < 1, "DateTime failed");
        Assert.True(Math.Abs((input.DTOField - test.DTOField).TotalSeconds) < 1, "DateTimeOffset failed");
    }

    [Fact]
    public void DateTimeDeserializationTest()
    {
        var input = new DateTimeClass
        {
            DTField = DateTime.Now,
            DTOField = DateTimeOffset.UtcNow
        };

        var json = JsonSerializer.Serialize(input);

        var test = MicroJson.Deserialize<DateTimeClass>(json);

        Assert.NotNull(test);
        // the fraction of a second will be lost, so equality won't work
        Assert.True(Math.Abs((input.DTField - test.DTField).TotalSeconds) < 1, "DateTime failed");
        Assert.True(Math.Abs((input.DTOField - test.DTOField).TotalSeconds) < 1, "DateTimeOffset failed");
    }

    [Fact]
    public void SimpleIntegerPropertyTest()
    {
        var input = """
            {
                "Value": 23
            }
            """;

        var result = MicroJson.Deserialize<IntegerClass>(input);

        Assert.Equal(23, result.Value);
    }

    [Fact]
    public void SimpleStringArrayTest()
    {
        var input = """
            [
                "Value1",
                "Value2",
                "Value3"
            ]
            """;

        var result = MicroJson.Deserialize<string[]>(input);

        Assert.Equal(3, result.Length);
    }

    [Fact]
    public void SerializeToCamelCaseTest()
    {
        var item = new IntegerClass { Value = 23 };
        var json = MicroJson.Serialize(item);

        Assert.Contains("value", json);
        Assert.DoesNotContain("Value", json);
    }

    [Fact]
    public void SerializeToNonCamelCaseTest()
    {
        var item = new IntegerClass { Value = 23 };
        var json = MicroJson.Serialize(item, convertNamesToCamelCase: false);

        Assert.Contains("Value", json);
        Assert.DoesNotContain("value", json);
    }

    [Fact]
    public void SerializeDataWithSpecialStringCharactersTest()
    {
        var testStrings = new string[]
            {
                "Hello\nThere",
                "Hello\r\nThere",
                "Hello{There}"
            };

        foreach (var s in testStrings)
        {
            var item = new StringFieldClass
            {
                FieldA = s
            };

            var json = MicroJson.Serialize(item);

            var opts = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            var testResult = JsonSerializer.Deserialize<StringFieldClass>(json, opts);

            Assert.NotNull(testResult);
            Assert.Equal(s, testResult.FieldA);
        }
    }

    [Fact]
    public void DeserializeDataWithContainsCarriageReturnsTest()
    {
        var input = """
            {
                "StringArg": "hello
                there
                data"
            }
            """;

        var result = MicroJson.Deserialize<SimpleCommand>(input);

        Assert.NotNull(result);
    }

    [Fact]
    public void DeserializeJsonWithExtraField()
    {
        var result = MicroJson.Deserialize<IntegerClass>("{\"stringArg\":\"my string\",\"value\":23}");
        Assert.Equal(23, result.Value);
    }
}
