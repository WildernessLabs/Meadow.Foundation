# Meadow.Foundation.Sensors.Flow.SwissflowSF800

**Swissflow SF800 flow sensorlibrary**

The **Flow Sensor** library is included in the **Meadow.Foundation.Sensors.Flow.SwissflowSF800** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Purpose

 The SF800 is generally used in two different scenarios:

 1. Signalling for intermittent flow and the total volume during the flow event.
 2. Providing data on the flow rate of the liquid passing through the meter.

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.Sensors.Flow.SwissflowSF800`
## Usage

```csharp
private SwissflowSF800 _sf800 = default!;

    public override Task Initialize()
    {
        Resolver.Log.Info("Initialize...");

        _sf800 = new SwissflowSF800(Device.Pins.D05);
        _sf800.FlowStarted += (s, e) => Resolver.Log.Info("Flow started");
        _sf800.FlowStopped += (s, e) => Resolver.Log.Info($"Flow stopped: {e.Volume.Liters} Liters");
        return Task.CompletedTask;
    }

    public override Task Run()
    {
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


