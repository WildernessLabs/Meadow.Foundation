# Meadow.Foundation.Sensors.Light.Temt6000

**Temt6000 analog ambient light sensor**

The **Temt6000** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
Temt6000 sensor;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    // configure our sensor
    sensor = new Temt6000(Device.Pins.A03);

    // Example that uses an IObservable subscription to only be notified when the voltage changes by at least 0.5V
    var consumer = Temt6000.CreateObserver(
        handler: result => Resolver.Log.Info($"Observer filter satisfied: {result.New.Volts:N2}V, old: {result.Old?.Volts:N2}V"),
        // only notify if the change is greater than 0.5V
        filter: result =>
        {
            if (result.Old is { } old)
            {
                return (result.New - old).Abs().Volts > 0.5; // returns true if > 0.5V change.
            }
            return false;
        });

    sensor.Subscribe(consumer);

    // classical .NET events can also be used:
    sensor.Updated += (sender, result) =>
    {
        Resolver.Log.Info($"Voltage Changed, new: {result.New.Volts:N2}V, old: {result.Old?.Volts:N2}V");
    };

    return Task.CompletedTask;
}

public override async Task Run()
{
    var result = await sensor.Read();
    Resolver.Log.Info($"Initial temp: {result.Volts:N2}V");

    sensor.StartUpdating(TimeSpan.FromMilliseconds(1000));
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
