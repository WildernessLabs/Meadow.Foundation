# Meadow.Foundation.Motors.Tb67h420ftg

**Tb67h420ftg digital input motor controller**

The **Tb67h420ftg** library is included in the **Meadow.Foundation.Motors.Tb67h420ftg** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.Motors.Tb67h420ftg`
## Usage

```csharp
Tb67h420ftg motorDriver;

PushButton button1;
PushButton button2;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    button1 = new PushButton(Device.Pins.D12, ResistorMode.InternalPullDown);
    button2 = new PushButton(Device.Pins.D13, ResistorMode.InternalPullDown);

    button1.PressStarted += Button1_PressStarted;
    button1.PressEnded += Button1_PressEnded;
    button2.PressStarted += Button2_PressStarted;
    button2.PressEnded += Button2_PressEnded;

    motorDriver = new Tb67h420ftg(
        inA1: Device.Pins.D04, inA2: Device.Pins.D03, pwmA: Device.Pins.D01,
        inB1: Device.Pins.D05, inB2: Device.Pins.D06, pwmB: Device.Pins.D00,
        fault1: Device.Pins.D02, fault2: Device.Pins.D07,
        hbMode: Device.Pins.D09, tblkab: Device.Pins.D10);

    // 6V motors with a 12V input. this clamps them to 6V
    motorDriver.Motor1.MotorCalibrationMultiplier = 0.5f;
    motorDriver.Motor2.MotorCalibrationMultiplier = 0.5f;

    Resolver.Log.Info("Initialization complete.");

    return base.Initialize();
}

private void Button1_PressStarted(object sender, EventArgs e)
{
    Resolver.Log.Info("Motor 1 start.");
    motorDriver.Motor1.Power = 1f;
}
private void Button1_PressEnded(object sender, EventArgs e)
{
    Resolver.Log.Info("Motor 1 stop.");
    motorDriver.Motor1.Power = 0f;
}

private void Button2_PressStarted(object sender, EventArgs e)
{
    Resolver.Log.Info("Motor 2 start.");
    motorDriver.Motor2.Power = 0.5f;
}
private void Button2_PressEnded(object sender, EventArgs e)
{
    Resolver.Log.Info("Motor 2 stop.");
    motorDriver.Motor2.Power = 0f;
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


