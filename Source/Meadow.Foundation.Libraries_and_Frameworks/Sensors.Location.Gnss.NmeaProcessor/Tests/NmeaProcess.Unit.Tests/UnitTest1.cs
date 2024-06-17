using Meadow.Foundation.Sensors.Location.Gnss;

namespace NmeaProcess.Unit.Tests;

public class GgaTests
{
    [Fact]
    public void TestGPGGA()
    {
        var testSentences = new string[]
        {
            "$GPGGA,115739.00,4158.8441367,N,09147.4416929,W,4,13,0.9,255.747,M,-32.00,M,01,0000*6E",
            "$GPGGA,123519,4807.038,N,01131.000,E,1,08,0.9,545.4,M,46.9,M,,*47",
            "$GPGGA,225446,4916.45,N,12311.12,W,1,05,1.5,280.2,M,-34.0,M,,*78",
            "$GPGGA,092750,5321.6802,N,00630.3372,W,1,08,0.95,59.0,M,55.2,M,,*5A",
            "$GPGGA,160450,3723.2475,N,12158.3416,W,1,04,1.2,23.4,M,-34.0,M,,*40",
            "$GPGGA,235959,5540.123,N,03736.654,E,1,09,0.8,150.0,M,45.0,M,,*45"
        };

        var decoder = new GgaDecoder();
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
            "$GPGLL,3723.2475,N,12158.3416,W,174530,A*32"
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

public class GsaTests
{
    [Fact]
    public void TestGPGSA()
    {
        var testSentences = new string[]
        {
        };

        foreach (var sentence in testSentences)
        {
        }
    }
}

public class GsvTests
{
    [Fact]
    public void TestGPGSV()
    {
        var testSentences = new string[]
        {
        };

        foreach (var sentence in testSentences)
        {
        }
    }
}

public class VtgTests
{
    [Fact]
    public void TestGPVTG()
    {
        var testSentences = new string[]
        {
            "$GPVTG, 309.62, T, ,M, 0.13, N, 0.2, K, A*23"
        };

        foreach (var sentence in testSentences)
        {
        }
    }
}
