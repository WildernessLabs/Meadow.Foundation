# Meadow.Foundation.Displays.WinForms

**Windows Forms display driver for Meadow**

The **WinForms** library is included in the **Meadow.Foundation.Displays.WinForms** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.Displays.WinForms`
## Usage

```csharp
public class MeadowApp : App<Meadow.Windows>
{
WinFormsDisplay? display;
MicroGraphics graphics = default!;

public override Task Initialize()
{
    display = new WinFormsDisplay(320, 240, displayScale: 1.5f);

    graphics = new MicroGraphics(display)
    {
        CurrentFont = new Font12x20(),
        Stroke = 1
    };

    _ = Task.Run(() =>
    {
        graphics.Clear();

        graphics.DrawTriangle(10, 30, 50, 50, 10, 50, Color.Red);
        graphics.DrawRectangle(20, 45, 40, 20, Color.Yellow, false);
        graphics.DrawCircle(50, 50, 40, Color.Blue, false);
        graphics.DrawText(5, 5, "Meadow on WinForms", Color.White);

        graphics.Show();
    });

    return Task.CompletedTask;
}

public override Task Run()
{
    Application.Run(display!);

    return Task.CompletedTask;
}

public static async Task Main(string[] args)
{
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);
    ApplicationConfiguration.Initialize();

    await MeadowOS.Start(args);
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


