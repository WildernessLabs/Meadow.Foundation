# Meadow.Foundation.Sensors.Environmental.NextPm

**TERA Sensor NextPM serial particulate matter sensor**

The **NextPm** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

## Usage

```csharp
NextPm nextPm;

public override Task Initialize()
{
    var port = Device
        .PlatformOS
        .GetSerialPortName("COM1")
        .CreateSerialPort();

    nextPm = new NextPm(port);

    nextPm.Readings10sUpdated += NextPm_Readings10sUpdated;

    return Task.CompletedTask;
}

private void NextPm_Readings10sUpdated(object sender, IChangeResult<ParticulateReading> e)
{
    Resolver.Log.Info($"Past 10 seconds");
    Resolver.Log.Info($"  {e.New.CountOf1micronParticles.ParticlesPerLiter:0} 1 micron particles per liter");
    Resolver.Log.Info($"  {e.New.CountOf2_5micronParticles.ParticlesPerLiter:0} 2.5 micron particles per liter");
    Resolver.Log.Info($"  {e.New.CountOf10micronParticles.ParticlesPerLiter:0} 10 micron particles per liter");
    Resolver.Log.Info($"  {e.New.EnvironmentalPM_1micron.MicroGramsPerMetersCubed:0} ug/L^3 1 micron particles");
    Resolver.Log.Info($"  {e.New.EnvironmentalPM_2_5micron.MicroGramsPerMetersCubed:0} ug/L^3 1 micron particles");
    Resolver.Log.Info($"  {e.New.EnvironmentalPM_10micron.MicroGramsPerMetersCubed:0} ug/L^3 1 micron particles");
}

public override async Task Run()
{
    Resolver.Log.Info("Run...");

    var firmware = await nextPm.GetFirmwareVersion();
    Resolver.Log.Info($"Firmware: 0x{firmware:X4}");

    var tempAndHumidity = await nextPm.GetTemperatureAndHumidity();
    Resolver.Log.Info($"Temp: {tempAndHumidity.temperature:0.0}C  Humidity: {tempAndHumidity.humidity}%");

    nextPm.StartUpdating();
}

```
