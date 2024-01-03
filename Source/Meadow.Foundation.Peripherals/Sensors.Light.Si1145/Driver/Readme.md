# Meadow.Foundation.Sensors.Light.Si1145

**SI1145 I2C ultraviolet and ambient light sensor**

The **Si1145** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
Si1145 sensor;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    sensor = new Si1145(Device.CreateI2cBus());

    // Example that uses an IObservable subscription to only be notified when the filter is satisfied
    var consumer = Si1145.CreateObserver(
        handler: result => Resolver.Log.Info($"Observer: filter satisfied: {result.New.VisibleLight?.Lux:N2}Lux, old: {result.Old?.VisibleLight?.Lux:N2}Lux"),

        // only notify if the visible light changes by 100 lux (put your hand over the sensor to trigger)
        filter: result =>
        {
            if (result.Old is { } old)
            {
                // returns true if > 100lux change
                return ((result.New.VisibleLight.Value - old.VisibleLight.Value).Abs().Lux > 100);
            }
            return false;
        });

    sensor.Subscribe(consumer);

    // classical .NET events can also be used:
    sensor.Updated += (sender, result) =>
    {
        Resolver.Log.Info($" Visible Light: {result.New.VisibleLight?.Lux:N2}Lux");
        Resolver.Log.Info($" Infrared Light: {result.New.Infrared?.Lux:N2}Lux");
        Resolver.Log.Info($" UV Index: {result.New.UltravioletIndex:N2}Lux");
    };

    return Task.CompletedTask;
}

public override async Task Run()
{
    var (VisibleLight, UltravioletIndex, Infrared) = await sensor.Read();

    Resolver.Log.Info("Initial Readings:");
    Resolver.Log.Info($" Visible Light: {VisibleLight?.Lux:N2}Lux");
    Resolver.Log.Info($" Infrared Light: {Infrared?.Lux:N2}Lux");
    Resolver.Log.Info($" UV Index: {UltravioletIndex:N2}Lux");

    sensor.StartUpdating(TimeSpan.FromSeconds(1));
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
