# Meadow.Foundation.Displays.ePaperWaveShare

**WaveShare SPI eInk / ePaper display controllers**

The **ePaperWaveShare** library is included in the **Meadow.Foundation.Displays.ePaperWaveShare** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.Displays.ePaperWaveShare`
## Usage

```csharp
MicroGraphics graphics;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize ...");

    var display = new Epd5in65f(
            spiBus: Device.CreateSpiBus(),
            chipSelectPin: Device.Pins.A04,
            dcPin: Device.Pins.A03,
            resetPin: Device.Pins.A02,
            busyPin: Device.Pins.A01);

    graphics = new MicroGraphics(display);

    return Task.CompletedTask;
}

public override Task Run()
{
    Resolver.Log.Info("Run");

    graphics.Clear();

    graphics.CurrentFont = new Font12x16();
    graphics.DrawText(0, 0, "Meadow F7", Color.Black, scaleFactor: ScaleFactor.X2);
    graphics.DrawText(0, 50, "Green", Color.Green, scaleFactor: ScaleFactor.X2);
    graphics.DrawText(0, 100, "Yellow", Color.Yellow, scaleFactor: ScaleFactor.X2);
    graphics.DrawText(0, 150, "Orange", Color.Orange, scaleFactor: ScaleFactor.X2);
    graphics.DrawText(0, 200, "Red", Color.Red, scaleFactor: ScaleFactor.X2);
    graphics.DrawText(0, 250, "Blue", Color.Blue, scaleFactor: ScaleFactor.X2);

    graphics.Show();

    Resolver.Log.Info("Run complete");

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


