# Meadow.Foundation.Displays.TftSpi

**SPI Color TFT and OLED displays (GC9A01, HC8357B, HX8357D, ILI9163, ILI9341, ILI9481, ILI9488, RM68140, S6D02A1, SSD1331, SSD1351, ST7735, ST7789)**

The **TftSpi** library is included in the **Meadow.Foundation.Displays.TftSpi** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.Displays.TftSpi`
## Usage

```csharp
MicroGraphics graphics;

public override Task Initialize()
{
    Resolver.Log.Info("Initializing ...");

    var spiBus = Device.CreateSpiBus();

    Resolver.Log.Info("Create display driver instance");

    var display = new Gc9a01
    (
        spiBus: spiBus,
        chipSelectPin: Device.Pins.A02,
        dcPin: Device.Pins.D01,
        resetPin: Device.Pins.D00
    );

    graphics = new MicroGraphics(display)
    {
        IgnoreOutOfBoundsPixels = true,
        CurrentFont = new Font12x20(),
        Rotation = RotationType._180Degrees
    };

    return base.Initialize();
}

public override Task Run()
{
    graphics.Clear();
    graphics.DrawCircle(120, 120, 100, Color.Cyan, false);
    graphics.DrawRoundedRectangle(50, 50, 140, 140, 50, Color.BlueViolet, false);
    graphics.DrawText(120, 120, "Meadow F7", alignmentH: HorizontalAlignment.Center, alignmentV: VerticalAlignment.Center);
    graphics.Show();

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


