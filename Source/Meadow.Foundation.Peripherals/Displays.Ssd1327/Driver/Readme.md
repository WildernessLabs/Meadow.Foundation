# Meadow.Foundation.Displays.Ssd1327

**SSD1327 SPI OLED 4bpp greyscale display**

The **Ssd1327** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all of Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

## Usage

```
MicroGraphics graphics;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize display...");

    var spiBus = Device.CreateSpiBus();

    var display = new Ssd1327(spiBus, Device.Pins.D02, Device.Pins.D01, Device.Pins.D00);

    display.SetContrast(60);

    graphics = new MicroGraphics(display);
    graphics.CurrentFont = new Font8x12();

    return base.Initialize();
}

public override Task Run()
{
    graphics.Clear();

    for (int i = 10; i > 0; i--)
    {   //interate across different brightnesses
        graphics.DrawText(0, i * 11, "SSD1327", Color.FromRgb(i * 0.1, i * 0.1, i * 0.1));
    }

    graphics.Show();

    return base.Run();
}

        
```

