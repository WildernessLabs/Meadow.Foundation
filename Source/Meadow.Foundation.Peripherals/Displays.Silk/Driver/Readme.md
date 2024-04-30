# Meadow.Foundation.Displays.Silk

**Display driver for Meadow using Silk.NET and SkiaSharp**

The **Meadow.Silk** library is included in the **Meadow.Foundation.Displays.Silk** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.Displays.Silk`
## Usage

```csharp
public class Program 
{
static SilkDisplay? display;
static MicroGraphics graphics = default!;

public static void Main()
{
    Initialize();
    Run();

    Thread.Sleep(Timeout.Infinite);
}

public static void Initialize()
{
    display = new SilkDisplay(640, 480, displayScale: 1f);

    graphics = new MicroGraphics(display)
    {
        CurrentFont = new Font16x24(),
        Stroke = 1
    };
}

public static void Run()
{
    graphics.Clear();

    graphics.DrawText(10, 10, "Silk.NET", Color.White);

    graphics.DrawText(10, 40, "1234567890!@#$%^&*(){}[],./<>?;':", Color.LawnGreen);
    graphics.DrawText(10, 70, "ABCDEFGHIJKLMNOPQRSTUVWXYZ", Color.Cyan);
    graphics.DrawText(10, 100, "abcdefghijklmnopqrstuvwxyz", Color.Yellow);
    graphics.DrawText(10, 130, "Temp: 21.5°C", Color.Orange);

    graphics.DrawTriangle(10, 220, 50, 260, 10, 260, Color.Red);
    graphics.DrawRectangle(20, 185, 80, 40, Color.Yellow, false);
    graphics.DrawCircle(50, 240, 40, Color.Blue, false);

    graphics.Show();

    display!.Run();
}
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


