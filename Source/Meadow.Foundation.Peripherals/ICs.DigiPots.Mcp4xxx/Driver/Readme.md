# Meadow.Foundation.ICs.DigiPots.Mcp4xxx

**Mcp4xxx SPI digital rheostats and potentiometers**

The **Mcp4xxx** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
protected Mcp4162 mcp;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    mcp = new Mcp4162(
        Device.CreateSpiBus(),
        Device.Pins.D15.CreateDigitalOutputPort(),
        new Resistance(5, Resistance.UnitType.Kiloohms)
        );

    return base.Initialize();
}

public override async Task Run()
{
    Resolver.Log.Info("Run");

    for (var i = 0; i <= mcp.MaxResistance.Ohms; i += 100)
    {
        var r = new Resistance(i, Resistance.UnitType.Ohms);
        Resolver.Log.Info($"Setting resistance to {r.Ohms:0} ohms");
        mcp.Rheostats[0].Resistance = r;
        await Task.Delay(1000);
    }

    Resolver.Log.Info("Done");
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
