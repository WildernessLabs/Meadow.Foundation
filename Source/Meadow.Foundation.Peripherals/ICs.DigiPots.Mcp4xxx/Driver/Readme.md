# Meadow.Foundation.ICs.DigiPots.Mcp4xxx

**Mcp4xxx SPI digital rheostats and potentiometers**

The **Mcp4xxx** library is included in the **Meadow.Foundation.ICs.DigiPots.Mcp4xxx** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.ICs.DigiPots.Mcp4xxx`
## Usage

```csharp
protected Mcp4162 mcp;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    mcp = new Mcp4162(
        Device.CreateSpiBus(),
        Device.Pins.D15.CreateDigitalOutputPort(),
        new Resistance(5, Resistance.UnitType.Kiloohms)
        );

    return base.Initialize();
}

public override async Task Run()
{
    Resolver.Log.Info("Run");

    for (var i = 0; i <= mcp.MaxResistance.Ohms; i += 100)
    {
        var r = new Resistance(i, Resistance.UnitType.Ohms);
        Resolver.Log.Info($"Setting resistance to {r.Ohms:0} ohms");
        mcp.Rheostats[0].Resistance = r;
        await Task.Delay(1000);
    }

    Resolver.Log.Info("Done");
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


