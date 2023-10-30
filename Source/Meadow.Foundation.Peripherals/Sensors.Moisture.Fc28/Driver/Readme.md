# Meadow.Foundation.Sensors.Moisture.Fc28

**FC28 analog soil moisture sensor**

The **Fc28** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
Fc28 fc28;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    fc28 = new Fc28(
        Device.CreateAnalogInputPort(Device.Pins.A01, 5, TimeSpan.FromMilliseconds(40), new Voltage(3.3, Voltage.UnitType.Volts)),
        Device.CreateDigitalOutputPort(Device.Pins.D15),
        minimumVoltageCalibration: new Voltage(3.24f, VU.Volts),
        maximumVoltageCalibration: new Voltage(2.25f, VU.Volts)
    );

    var consumer = Fc28.CreateObserver(
        handler: result =>
        {
            // the first time through, old will be null.
            string oldValue = (result.Old is { } old) ? $"{old:n2}" : "n/a";
            Resolver.Log.Info($"Subscribed - " +
                $"new: {result.New}, " +
                $"old: {oldValue}");
        },
        filter: null
    );
    fc28.Subscribe(consumer);

    fc28.MoistureUpdated += (object sender, IChangeResult<double> e) =>
    {
        Resolver.Log.Info($"Moisture Updated: {e.New}");
    };

    return Task.CompletedTask;
}

public async override Task Run()
{
    var moisture = await fc28.Read();
    Resolver.Log.Info($"Moisture Value {moisture}");

    fc28.StartUpdating(TimeSpan.FromMilliseconds(5000));
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
