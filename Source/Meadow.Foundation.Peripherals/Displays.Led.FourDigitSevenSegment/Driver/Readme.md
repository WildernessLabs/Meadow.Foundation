# Meadow.Foundation.Displays.Led.FourDigitSevenSegment

**Digital Four digit seven segment displays**

The **FourDigitSevenSegment** library is included in the **Meadow.Foundation.Displays.Led.FourDigitSevenSegment** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.Displays.Led.FourDigitSevenSegment`
## Usage

```csharp
FourDigitSevenSegment sevenSegment;

public override Task Initialize()
{
    Resolver.Log.Info("Initializing...");

    sevenSegment = new FourDigitSevenSegment
    (
        portDigit1: Device.CreateDigitalOutputPort(Device.Pins.D00),
        portDigit2: Device.CreateDigitalOutputPort(Device.Pins.D03),
        portDigit3: Device.CreateDigitalOutputPort(Device.Pins.D04),
        portDigit4: Device.CreateDigitalOutputPort(Device.Pins.D06),
        portA: Device.CreateDigitalOutputPort(Device.Pins.D01),
        portB: Device.CreateDigitalOutputPort(Device.Pins.D05),
        portC: Device.CreateDigitalOutputPort(Device.Pins.D08),
        portD: Device.CreateDigitalOutputPort(Device.Pins.D10),
        portE: Device.CreateDigitalOutputPort(Device.Pins.D11),
        portF: Device.CreateDigitalOutputPort(Device.Pins.D02),
        portG: Device.CreateDigitalOutputPort(Device.Pins.D07),
        portDecimal: Device.CreateDigitalOutputPort(Device.Pins.D09),
        isCommonCathode: true
    );

    return base.Initialize();
}

public override Task Run()
{
    sevenSegment.SetDisplay("1234");

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


