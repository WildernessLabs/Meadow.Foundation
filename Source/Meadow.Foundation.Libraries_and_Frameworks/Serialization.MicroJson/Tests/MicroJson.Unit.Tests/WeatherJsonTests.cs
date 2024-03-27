using Meadow.Foundation.Serialization;
using Xunit;

namespace Unit.Tests;

public class WeatherJsonTests
{
    [Fact]
    public void MatchedCaseWeatherDeserializationTest()
    {
        var json = Inputs.GetInputResource("weather.json");
        var result = MicroJson.Deserialize<WeatherReadingDTO>(json);

        Assert.NotNull(result);
        Assert.NotNull(result.clouds);
        Assert.NotNull(result.wind);
        Assert.NotNull(result.main);
        Assert.NotNull(result.weather);
        Assert.NotNull(result.coord);
        Assert.NotNull(result.sys);
    }

    [Fact]
    public void CamelCasedWeatherDeserializationTest()
    {
        var json = Inputs.GetInputResource("weather.json");
        var result = MicroJson.Deserialize<WeatherReadingDTOCamelCase>(json);

        Assert.NotNull(result);
        Assert.NotNull(result.Clouds);
        Assert.NotNull(result.Wind);
        Assert.NotNull(result.Main);
        Assert.NotNull(result.Weather);
        Assert.NotNull(result.Coord);
        Assert.NotNull(result.Sys);
    }
}
