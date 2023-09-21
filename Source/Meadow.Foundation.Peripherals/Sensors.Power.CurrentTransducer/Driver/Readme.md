# Meadow.Foundation.Sensors.Power.CurrentTransducer

**Current transducer library**

The **CurrentTransducer** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
private CurrentTransducer transducer = default!;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    var bus = Device.CreateI2cBus();
    transducer = new CurrentTransducer(
        Device.Pins.A00.CreateAnalogInputPort(1),
        new Voltage(3.3, Voltage.UnitType.Volts), // a reading of 3.3V
        new Current(10, Current.UnitType.Amps)    // equals 10 amps of current
        );

    Resolver.Log.Info($"-- Current Transducer Sample App ---");
    transducer.Updated += (s, v) =>
    {
        Resolver.Log.Info($"Current is now {v.New.Amps}A");
    };

    return Task.CompletedTask;
}

public override Task Run()
{
    transducer.StartUpdating(TimeSpan.FromSeconds(2));
    return Task.CompletedTask;
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
