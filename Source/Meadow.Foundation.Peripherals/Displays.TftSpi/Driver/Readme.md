# Meadow.Foundation.Displays.TftSpi

**SPI Color TFT and OLED displays (GC9A01, HC8357B, HX8357D, ILI9163, ILI9341, ILI9481, ILI9488, RM68140, S6D02A1, SSD1331, SSD1351, ST7735, ST7789)**

The **TftSpi** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

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
    graphics.DrawCircle(120, 120, 100, Meadow.Foundation.Color.Cyan, false);
    graphics.DrawRoundedRectangle(50, 50, 140, 140, 50, Meadow.Foundation.Color.BlueViolet, false);
    graphics.DrawText(120, 120, "Meadow F7", alignmentH: HorizontalAlignment.Center, alignmentV: VerticalAlignment.Center);
    graphics.Show();

    return base.Run();
}

```
