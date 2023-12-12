# Meadow.Foundation.Displays.ePaper

**SPI eInk / ePaper display controllers (IL0373, IL0376F, IL0398, IL3897, IL91874, ILI91874v3, SSD1608)**

The **EPaper** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
MicroGraphics graphics;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize ...");

    var display = new Il0373(
        spiBus: Device.CreateSpiBus(),
        chipSelectPin: Device.Pins.D03,
        dcPin: Device.Pins.D02,
        resetPin: Device.Pins.D01,
        busyPin: Device.Pins.D00,
        width: 400,
        height: 300);

    graphics = new MicroGraphics(display);

    return Task.CompletedTask;
}

public override Task Run()
{
    //any color but black will show the ePaper alternate color 
    graphics.DrawRectangle(1, 1, 126, 32, Color.Black, false);

    graphics.CurrentFont = new Font12x16();
    graphics.DrawText(2, 2, "IL0373", Color.Black);
    graphics.DrawText(2, 30, "Meadow F7", Color.Black);

    graphics.Show();

    return Task.CompletedTask;
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
