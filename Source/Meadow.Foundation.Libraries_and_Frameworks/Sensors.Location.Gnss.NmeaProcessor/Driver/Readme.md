# Meadow.Foundation.Sensors.Location.Gnss.NmeaProcessor

**GNSS NMEA parsing library**

The **NmeaProcessor** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

## Usage

```csharp
List<string> sentences;
NmeaSentenceProcessor nmeaProcessor;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");
    
    sentences = GetSampleNmeaSentences();

    InitDecoders();

    foreach (string sentence in sentences) 
    {
        Resolver.Log.Info($"About to process:{sentence}");
        nmeaProcessor.ProcessNmeaMessage(sentence);
    }

    Resolver.Log.Info("Made it through all sentences");

    return Task.CompletedTask;
}

void InitDecoders()
{
    Resolver.Log.Info("Create NMEA");
    nmeaProcessor = new NmeaSentenceProcessor();

    Resolver.Log.Info("Add decoders");

    // GGA
    var ggaDecoder = new GgaDecoder();
    Resolver.Log.Info("Created GGA");
    nmeaProcessor.RegisterDecoder(ggaDecoder);
    ggaDecoder.PositionReceived += (object sender, GnssPositionInfo location) => {
        Resolver.Log.Info("*********************************************");
        Resolver.Log.Info(location.ToString());
        Resolver.Log.Info("*********************************************");
    };

    // GLL
    var gllDecoder = new GllDecoder();
    nmeaProcessor.RegisterDecoder(gllDecoder);
    gllDecoder.GeographicLatitudeLongitudeReceived += (object sender, GnssPositionInfo location) => {
        Resolver.Log.Info("*********************************************");
        Resolver.Log.Info(location.ToString());
        Resolver.Log.Info("*********************************************");
    };

    // GSA
    var gsaDecoder = new GsaDecoder();
    nmeaProcessor.RegisterDecoder(gsaDecoder);
    gsaDecoder.ActiveSatellitesReceived += (object sender, ActiveSatellites activeSatellites) => {
        Resolver.Log.Info("*********************************************");
        Resolver.Log.Info(activeSatellites.ToString());
        Resolver.Log.Info("*********************************************");
    };

    // RMC (recommended minimum)
    var rmcDecoder = new RmcDecoder();
    nmeaProcessor.RegisterDecoder(rmcDecoder);
    rmcDecoder.PositionCourseAndTimeReceived += (object sender, GnssPositionInfo positionCourseAndTime) => {
        Resolver.Log.Info("*********************************************");
        Resolver.Log.Info(positionCourseAndTime.ToString());
        Resolver.Log.Info("*********************************************");

    };

    // VTG (course made good)
    var vtgDecoder = new VtgDecoder();
    nmeaProcessor.RegisterDecoder(vtgDecoder);
    vtgDecoder.CourseAndVelocityReceived += (object sender, CourseOverGround courseAndVelocity) => {
        Resolver.Log.Info("*********************************************");
        Resolver.Log.Info($"{courseAndVelocity}");
        Resolver.Log.Info("*********************************************");
    };

    // GSV (satellites in view)
    var gsvDecoder = new GsvDecoder();
    nmeaProcessor.RegisterDecoder(gsvDecoder);
    gsvDecoder.SatellitesInViewReceived += (object sender, SatellitesInView satellites) => {
        Resolver.Log.Info("*********************************************");
        Resolver.Log.Info($"{satellites}");
        Resolver.Log.Info("*********************************************");
    };
}

List<string> GetSampleNmeaSentences()
{
    List<string> sentences = new List<string>() {
        "$GPGGA,000049.799,,,,,0,00,,,M,,M,,*72", // i think not valid.
        "$GNGGA,001043.00,4404.14036,N,12118.85961,W,1,12,0.98,1113.0,M,-21.3,M,,*47", // valid
        "$GNRMC,001031.00,A,4404.13993,N,12118.86023,W,0.146,,100117,,,A*7B", // valid
        "$GPVTG,220.86,T,,M,2.550,N,4.724,K,A*34", // Valid
        "$GPVTG,0.00,T,,M,0.00,N,0.00,K,N*32",
        "$GPRMC,162254.00,A,3723.02837,N,12159.39853,W,0.820,188.36,110706,,,A*74",
        "$GPVTG,188.36,T,,M,0.820,N,1.519,K,A*3F",
        "$GPGGA,162254.00,3723.02837,N,12159.39853,W,1,03,2.36,525.6,M,-25.6,M,,*65",
        "$GPGSA,A,2,25,01,22,,,,,,,,,,2.56,2.36,1.00*02",

        "$GPGSA,A,3,10,32,14,01,,,,,,,,,2.41,2.20,0.99*01",

        "$GPGSV,3,1,11,03,03,111,00,04,15,270,00,06,01,010,00,13,06,292,00*74",
        "$GPGSV,3,2,11,14,25,170,00,16,57,208,39,18,67,296,40,19,40,246,00*74",
        "$GPGSV,3,3,11,22,42,067,42,24,14,311,43,27,05,244,00,,,,*4D",

        "$GPGLL,3723.02837,N,12159.39853,W,162254.00,A,A*7C",
        "$GPZDA,162254.00,11,07,2006,00,00*63",
        "$GNGLL,,,,,,V,N*7A",
        "$GLGSV,1,1,00*65",
    };

    return sentences;
}

```
