# Meadow.Foundation.Displays.WinForms

**Windows Forms display driver for Meadow**

The **WinForms** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
public class MeadowApp : App<Meadow.Windows>
{
WinFormsDisplay? _display;
MicroGraphics _graphics = default!;

public override Task Initialize()
{
    _display = new WinFormsDisplay(240, 320);

    _graphics = new MicroGraphics(_display)
    {
        CurrentFont = new Font12x20(),
        Stroke = 1
    };

    _ = Task.Run(() =>
    {
        Thread.Sleep(2000);

        _graphics.Clear();

        _graphics.DrawTriangle(10, 30, 50, 50, 10, 50, Color.Red);
        _graphics.DrawRectangle(20, 45, 40, 20, Color.Yellow, false);
        _graphics.DrawCircle(50, 50, 40, Color.Blue, false);
        _graphics.DrawText(5, 5, "Meadow F7", Color.White);

        _graphics.Show();
    });

    return Task.CompletedTask;
}

public override Task Run()
{
    Application.Run(_display!);

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
