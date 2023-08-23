# Meadow.Foundation.Sensors.Gnss.NeoM8

**NEO M8 serial GNSS / GPS controller**

The **NeoM8** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all of Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

## Usage

```
NeoM8 gps;

public override Task Initialize()
{
    Resolver.Log.Info("Initializing ...");

    //SPI
    //gps = new NeoM8(Device.CreateSpiBus(), Device.CreateDigitalOutputPort(Device.Pins.D14), null);

    //I2C
    //gps = new NeoM8(Device.CreateI2cBus());

    //Serial
    gps = new NeoM8(Device, Device.PlatformOS.GetSerialPortName("COM1"), Device.Pins.D09, Device.Pins.D11);

    gps.GgaReceived += (object sender, GnssPositionInfo location) =>
    {
        Resolver.Log.Info("*********************************************");
        Resolver.Log.Info($"{location}");
        Resolver.Log.Info("*********************************************");
    };
    // GLL
    gps.GllReceived += (object sender, GnssPositionInfo location) =>
    {
        Resolver.Log.Info("*********************************************");
        Resolver.Log.Info($"{location}");
        Resolver.Log.Info("*********************************************");
    };
    // GSA
    gps.GsaReceived += (object sender, ActiveSatellites activeSatellites) =>
    {
        Resolver.Log.Info("*********************************************");
        Resolver.Log.Info($"{activeSatellites}");
        Resolver.Log.Info("*********************************************");
    };
    // RMC (recommended minimum)
    gps.RmcReceived += (object sender, GnssPositionInfo positionCourseAndTime) =>
    {
        Resolver.Log.Info("*********************************************");
        Resolver.Log.Info($"{positionCourseAndTime}");
        Resolver.Log.Info("*********************************************");

    };
    // VTG (course made good)
    gps.VtgReceived += (object sender, CourseOverGround courseAndVelocity) =>
    {
        Resolver.Log.Info("*********************************************");
        Resolver.Log.Info($"{courseAndVelocity}");
        Resolver.Log.Info("*********************************************");
    };
    // GSV (satellites in view)
    gps.GsvReceived += (object sender, SatellitesInView satellites) =>
    {
        Resolver.Log.Info("*********************************************");
        Resolver.Log.Info($"{satellites}");
        Resolver.Log.Info("*********************************************");
    };

    return Task.CompletedTask;
}

public override Task Run()
{
    gps.StartUpdating();

    return Task.CompletedTask;
}

        
```

