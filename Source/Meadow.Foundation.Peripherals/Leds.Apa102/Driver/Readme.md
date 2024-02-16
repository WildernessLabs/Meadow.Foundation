# Meadow.Foundation.Leds.Apa102

**Apa102 SPI RGB LED driver**

The **Apa102** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
Apa102 apa102;
readonly int numberOfLeds = 256;
readonly float maxBrightness = 0.25f;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");
    apa102 = new Apa102(Device.CreateSpiBus(), numberOfLeds, Apa102.PixelOrder.BGR);

    return base.Initialize();
}

public override Task Run()
{
    apa102.Clear();

    apa102.SetLed(index: 0, color: Color.Red, brightness: 0.5f);
    apa102.SetLed(index: 1, color: Color.Purple, brightness: 0.6f);
    apa102.SetLed(index: 2, color: Color.Blue, brightness: 0.7f);
    apa102.SetLed(index: 3, color: Color.Green, brightness: 0.8f);
    apa102.SetLed(index: 4, color: Color.Yellow, brightness: 0.9f);
    apa102.SetLed(index: 5, color: Color.Orange, brightness: 1.0f);

    apa102.Show();

    Apa102Tests();

    return Task.CompletedTask;
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
