# Meadow.Foundation.Sensors.Hid.Tsc2004

**Tsc2004 I2C capacitive touch screen**

The **Tsc2004** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
Tsc2004 touchScreen;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    var i2cBus = Device.CreateI2cBus(I2cBusSpeed.Fast);

    touchScreen = new Tsc2004(i2cBus)
    {
        DisplayWidth = 240,
        DisplayHeight = 320,
        XMin = 260,
        XMax = 3803,
        YMin = 195,
        YMax = 3852,
        Rotation = RotationType._90Degrees
    };

    return Task.CompletedTask;
}

public override Task Run()
{
    return Task.Run(() =>
    {
        Point3d pt;

        while (true)
        {
            if (touchScreen.IsTouched())
            {
                pt = touchScreen.GetPoint();
                Resolver.Log.Info($"Location: X:{pt.X}, Y:{pt.Y}, Z:{pt.Z}");
            }

            Thread.Sleep(0);
        }
    });
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
