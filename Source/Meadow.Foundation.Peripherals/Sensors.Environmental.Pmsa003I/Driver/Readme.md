# Meadow.Foundation.Sensors.Environmental.Pmsa003i

**PMSA003I I2C particulate matter AQI sensor**

The **Pmsa003i** library is included in the **Meadow.Foundation.Sensors.Environmental.Pmsa003i** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.Sensors.Environmental.Pmsa003i`
## Usage

```csharp
Pmsa003i pmsa003i;

public override Task Initialize()
{
    var bus = Device.CreateI2cBus(I2cBusSpeed.Standard);
    pmsa003i = new Pmsa003i(bus);

    pmsa003i.Updated += Pmsa003i_Updated;

    return base.Initialize();
}

public override Task Run()
{
    Resolver.Log.Info("Run...");

    pmsa003i.StartUpdating(TimeSpan.FromSeconds(2));

    return base.Run();
}

private void Pmsa003i_Updated(object sender, IChangeResult<(
    Density? StandardParticulateMatter_1micron,
    Density? StandardParticulateMatter_2_5micron,
    Density? StandardParticulateMatter_10micron,
    Density? EnvironmentalParticulateMatter_1micron,
    Density? EnvironmentalParticulateMatter_2_5micron,
    Density? EnvironmentalParticulateMatter_10micron,
    ParticleDensity? ParticleDensity_0_3microns,
    ParticleDensity? ParticleDensity_0_5microns,
    ParticleDensity? ParticleDensity_10microns,
    ParticleDensity? ParticleDensity_25microns,
    ParticleDensity? ParticleDensity_50microns,
    ParticleDensity? ParticleDensity_100microns)> e)
{
    Resolver.Log.Info($"Standard Particulate Matter 1 micron: {e.New.StandardParticulateMatter_1micron.Value.MicroGramsPerMetersCubed} micrograms per m^3");
    Resolver.Log.Info($"Standard Particulate Matter 2_5micron: {e.New.StandardParticulateMatter_2_5micron.Value.MicroGramsPerMetersCubed} micrograms per m^3");
    Resolver.Log.Info($"Standard Particulate Matter 10 micron: {e.New.StandardParticulateMatter_10micron.Value.MicroGramsPerMetersCubed} micrograms per m^3");
    Resolver.Log.Info($"Environmental Particulate Matter 1 micron: {e.New.EnvironmentalParticulateMatter_1micron.Value.MicroGramsPerMetersCubed} micrograms per m^3");
    Resolver.Log.Info($"Environmental Particulate Matter 2.5 micron: {e.New.EnvironmentalParticulateMatter_2_5micron.Value.MicroGramsPerMetersCubed} micrograms per m^3");
    Resolver.Log.Info($"Environmental Particulate Matter 10 micron: {e.New.EnvironmentalParticulateMatter_10micron.Value.MicroGramsPerMetersCubed} micrograms per m^3"); ;

    Resolver.Log.Info($"Count of particles - 0.3 microns: {e.New.ParticleDensity_0_3microns.Value.ParticlesPerCentiliter} in 0.1 liters of air");
    Resolver.Log.Info($"Count of particles - 0.5 microns: {e.New.ParticleDensity_0_5microns.Value.ParticlesPerCentiliter} in 0.1 liters of air");
    Resolver.Log.Info($"Count of particles - 10 microns: {e.New.ParticleDensity_10microns.Value.ParticlesPerCentiliter} in 0.1 liters of air");
    Resolver.Log.Info($"Count of particles - 25 microns: {e.New.ParticleDensity_25microns.Value.ParticlesPerCentiliter} in 0.1 liters of air");
    Resolver.Log.Info($"Count of particles - 50 microns: {e.New.ParticleDensity_50microns.Value.ParticlesPerCentiliter} in 0.1 liters of air");
    Resolver.Log.Info($"Count of particles - 100 microns: {e.New.ParticleDensity_100microns.Value.ParticlesPerCentiliter} in 0.1 liters of air");
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


