# Meadow.Foundation.Sensors.Power.Ina2xx

**INA2xx Series I2C current and power monitor**

The **Ina2xx** library is included in the **Meadow.Foundation.Sensors.Power.Ina2xx** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.Sensors.Power.Ina2xx`
## Usage

```csharp
Ina219 ina219;

public override Task Initialize()
{
    Resolver.Log.Debug("Initialize...");

    var bus = Device.CreateI2cBus(I2cBusSpeed.Fast);
    ina219 = new Ina219(bus);
    ina219.Configure(busVoltageRange: Ina219.BusVoltageRange.Range_32V,
        maxExpectedCurrent: new Current(1.0),
        adcMode: Ina219.ADCModes.ADCMode_4xAvg_2128us);
    Resolver.SensorService.RegisterSensor(ina219);

    Resolver.Log.Info($"--- INA219 Sample App ---");
    ina219.Updated += (sender, result) =>
    {
        Resolver.Log.Info($"{result.New.Voltage:N3} V @ {result.New.Current:N3} A");
    };

    return Task.CompletedTask;
}

public override Task Run()
{
    Resolver.Log.Debug("Run...");
    ina219.StartUpdating(TimeSpan.FromSeconds(2));
    return Task.CompletedTask;
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


