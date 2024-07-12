using Meadow.Foundation.Serialization;
using Meadow.Hardware;
using Xunit;

namespace Unit.Tests;

public class TouchCalibrationTests
{
    [Fact]
    public void BidirectionalCalibrationPointTest()
    {
        var points = new CalibrationPoint[]
            {
                new CalibrationPoint(10, 20, 100, 200),
                new CalibrationPoint(15, 25, 150, 250)
            };

        var json = MicroJson.Serialize(points);
        Assert.NotNull(json);

        var testArray = MicroJson.Deserialize<CalibrationPoint[]>(json);
        Assert.Equal(points.Length, testArray.Length);
    }
}
