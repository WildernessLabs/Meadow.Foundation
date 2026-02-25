# Meadow.Foundation.Displays.SharpMemory

**Sharp Memory Display SPI monochrome driver (LS013B4DN04, LS027B7DH01, LS032B1L03)**

The **SharpMemoryDisplay** library is included in the **Meadow.Foundation.Displays.SharpMemory** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual Studio using the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.Displays.SharpMemory`

## Usage

```csharp
MicroGraphics graphics;

public override Task Initialize()
{
    Resolver.Log.Info("Initializing...");

    var display = new SharpMemoryDisplay(
        spiBus: Device.CreateSpiBus(),
        chipSelectPin: Device.Pins.D00,
        width: 96,
        height: 96);

    graphics = new MicroGraphics(display)
    {
        CurrentFont = new Font8x12()
    };

    return base.Initialize();
}

public override Task Run()
{
    graphics.Clear();
    graphics.DrawText(0, 0, "Sharp Memory");
    graphics.DrawRectangle(0, 30, 96, 60);
    graphics.Show();

    return base.Run();
}
```

## Supported Displays

| Part Number  | Resolution |
|--------------|------------|
| LS013B4DN04  |  96 x  96  |
| LS027B7DH01  | 400 x 240  |
| LS032B1L03   | 320 x 240  |

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
