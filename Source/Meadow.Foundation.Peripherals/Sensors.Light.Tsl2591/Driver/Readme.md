# Meadow.Foundation.Sensors.Light.Tsl2591

**Tsl2591 I2C high dynamic range light sensor**

The **Tsl2591** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
private Tsl2591 sensor;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    // configure our sensor on the I2C Bus
    var i2c = Device.CreateI2cBus();
    sensor = new Tsl2591(i2c);

    // Example that uses an IObservable subscription to only be notified when the filter is satisfied
    var consumer = Tsl2591.CreateObserver(
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
        Resolver.Log.Info($"  Integrated Light: {result.New.Lux:N2}Lux");
    };

    sensor.InfraredUpdated += (sender, result) =>
    {
        Resolver.Log.Info($"  Infrared Light: {result.New.Lux:N2}Lux");
    };

    sensor.VisibleLightUpdated += (sender, result) =>
    {
        Resolver.Log.Info($"  Visible Light: {result.New.Lux:N2}Lux");
    };

    sensor.FullSpectrumUpdated += (sender, result) =>
    {
        Resolver.Log.Info($"  Full Spectrum Light: {result.New.Lux:N2}Lux");
    };

    return Task.CompletedTask;
}

public override async Task Run()
{
    var result = await sensor.Read();
    Resolver.Log.Info("Initial Readings:");
    Resolver.Log.Info($"  Full Spectrum Light: {result.FullSpectrum?.Lux:N2}Lux");
    Resolver.Log.Info($"  Infrared Light: {result.Infrared?.Lux:N2}Lux");
    Resolver.Log.Info($"  Visible Light: {result.VisibleLight?.Lux:N2}Lux");
    Resolver.Log.Info($"  Integrated Light: {result.Integrated?.Lux:N2}Lux");

    sensor.StartUpdating(TimeSpan.FromSeconds(1));
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
