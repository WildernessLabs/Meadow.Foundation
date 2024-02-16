# Meadow.Foundation.ICs.IOExpanders.Ht16k33

**HT16K33 I2C IO expander, led driver, and character display controller**

The **Ht16k33** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
Ht16k33 ht16k33;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");
    ht16k33 = new Ht16k33(Device.CreateI2cBus());

    return base.Initialize();
}

public override async Task Run()
{
    int index = 0;
    bool on = true;

    while (true)
    {
        ht16k33.SetLed((byte)index, on);
        ht16k33.UpdateDisplay();
        index++;

        if (index >= 128)
        {
            index = 0;
            on = !on;
        }

        await Task.Delay(100);
    }
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
