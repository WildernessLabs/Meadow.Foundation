using Meadow.Foundation.Sensors.Location.Gnss;

namespace NmeaProcess.Unit.Tests;

public class RmcTests
{
    [Fact]
    public void TestGPRMC()
    {
        // ID, TIME, STATUS, LATITUDE, N/S, LONGITUDE, E/W, SPEED, COURSE, DATE, MAGNETIC VARIATION, MODE, CHECKSUM
        var testSentences = new string[]
        {
            "$GPRMC,123519,A,4807.038,N,01131.000,E,022.4,084.4,230394,003.1,W*6A",
            "$GPRMC,223759,A,3751.65,S,14507.36,E,000.0,360.0,130694,011.3,E*6D",
            "$GPRMC,085120,A,4050.995,N,07359.965,W,005.5,054.7,040302,002.1,W*7E",
            "$GPRMC,000000,A,4807.000,N,01131.000,E,000.0,000.0,010203,000.0,W*6D",
            "$GPRMC,192045,A,3723.2475,N,12158.3416,W,000.0,360.0,080301,015.0,E*6A"
        };

        var decoder = new RmcDecoder();
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
