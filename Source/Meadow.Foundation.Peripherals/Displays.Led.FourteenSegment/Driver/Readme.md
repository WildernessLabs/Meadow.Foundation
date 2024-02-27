# Meadow.Foundation.Displays.Led.FourteenSegment

**Digital Fourteen (14) segment display**

The **FourteenSegment** library is included in the **Meadow.Foundation.Displays.Led.FourteenSegment** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.Displays.Led.FourteenSegment`
## Usage

```csharp
FourteenSegment fourteenSegment;

public override Task Initialize()
{
    Resolver.Log.Info("Initializing...");

    fourteenSegment = new FourteenSegment
    (
        portA: Device.CreateDigitalOutputPort(Device.Pins.D00),
        portB: Device.CreateDigitalOutputPort(Device.Pins.D01),
        portC: Device.CreateDigitalOutputPort(Device.Pins.D02),
        portD: Device.CreateDigitalOutputPort(Device.Pins.D03),
        portE: Device.CreateDigitalOutputPort(Device.Pins.D04),
        portF: Device.CreateDigitalOutputPort(Device.Pins.D05),
        portG: Device.CreateDigitalOutputPort(Device.Pins.D06),
        portG2: Device.CreateDigitalOutputPort(Device.Pins.D07),
        portH: Device.CreateDigitalOutputPort(Device.Pins.D08),
        portJ: Device.CreateDigitalOutputPort(Device.Pins.D09),
        portK: Device.CreateDigitalOutputPort(Device.Pins.D10),
        portL: Device.CreateDigitalOutputPort(Device.Pins.D11),
        portM: Device.CreateDigitalOutputPort(Device.Pins.D12),
        portN: Device.CreateDigitalOutputPort(Device.Pins.D13),
        portDecimal: Device.CreateDigitalOutputPort(Device.Pins.D14),
        isCommonCathode: false
    );

    return base.Initialize();
}

public override Task Run()
{
    fourteenSegment.SetDisplay(asciiCharacter: 'A', showDecimal: true);

    return base.Run();
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


