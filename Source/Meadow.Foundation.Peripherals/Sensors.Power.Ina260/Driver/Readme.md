# Meadow.Foundation.Sensors.Power.Ina260

**INA260 I2C current and power monitor**

The **Ina260** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
Ina260 ina260;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    var bus = Device.CreateI2cBus();
    ina260 = new Ina260(bus);

    Resolver.Log.Info($"-- INA260 Sample App ---");
    Resolver.Log.Info($"Manufacturer: {ina260.ManufacturerID}");
    Resolver.Log.Info($"Die: {ina260.DieID}");
    ina260.Updated += (s, v) =>
    {
        Resolver.Log.Info($"{v.New.Item2}V @ {v.New.Item3}A");
    };

    return Task.CompletedTask;
}

public override Task Run()
{ 
    ina260.StartUpdating(TimeSpan.FromSeconds(2));
    return Task.CompletedTask;
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
