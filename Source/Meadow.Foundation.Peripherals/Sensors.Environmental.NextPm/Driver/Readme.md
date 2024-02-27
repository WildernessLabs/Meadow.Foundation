# Meadow.Foundation.Sensors.Environmental.NextPm

**TERA Sensor NextPM serial particulate matter sensor**

The **NextPm** library is included in the **Meadow.Foundation.Sensors.Environmental.NextPm** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.Sensors.Environmental.NextPm`
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
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
## About Meadow

Meadow is a complete, IoT platform with defense-grade security that runs full .NET applications on embeddable microcontrollers and Linux single-board computers including Raspberry Pi and NVIDIA Jetson.

### Build

Use the full .NET platform and tooling such as Visual Studio and plug-and-play hardware drivers to painlessly build IoT solutions.

### Connect

Utilize native support for WiFi, Ethernet, and Cellular connectivity to send sensor data to the Cloud and remotely control your peripherals.

### Deploy

Instantly deploy and manage your fleet in the cloud for OtA, health-monitoring, logs, command + control, and enterprise backend integrations.


