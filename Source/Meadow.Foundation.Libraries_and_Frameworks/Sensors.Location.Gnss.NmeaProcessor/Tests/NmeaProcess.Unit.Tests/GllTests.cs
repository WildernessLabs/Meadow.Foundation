using Meadow.Foundation.Sensors.Location.Gnss;

namespace NmeaProcess.Unit.Tests;

public class GllTests
{
    [Fact]
    public void TestGPGLL()
    {
        var testSentences = new string[]
        {
            "$GPGLL,4916.45,N,12311.12,W,225444,A*31",
            "$GPGLL,5303.15,N,00920.56,E,092204,A*2E",
            "$GPGLL,5530.1234,N,03736.1234,E,082706,A*23",
            "$GPGLL,3751.65,S,14507.36,E,232905,A*39",
            "$GPGLL,3723.2475,N,12158.3416,W,174530,A*32",
            "$GNGLL,,,,,,V,N*7A"
        };

        var decoder = new GllDecoder();
        var processed = false;

        decoder.PositionReceived += (s, m) =>
        {
            processed = true;
        };

        foreach (var sentence in testSentences)
        {
            processed = false;
            decoder.Process(sentence);
            Assert.True(processed);
        }
    }
}
