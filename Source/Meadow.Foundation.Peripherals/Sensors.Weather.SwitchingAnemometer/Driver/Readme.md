# Meadow.Foundation.Sensors.Weather.SwitchingAnemometer

**Digital Switching Anemometer wind speed sensor**

The **SwitchingAnemometer** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
SwitchingAnemometer anemometer;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    anemometer = new SwitchingAnemometer(Device.Pins.A01);

    //==== classic events example
    anemometer.Updated += (sender, result) =>
    {
        Resolver.Log.Info($"new speed: {result.New.KilometersPerHour:n1}kmh, old: {result.Old?.KilometersPerHour:n1}kmh");
    };

    //==== IObservable example
    var observer = SwitchingAnemometer.CreateObserver(
        handler: result =>
        {
            Resolver.Log.Info($"new speed (from observer): {result.New.KilometersPerHour:n1}kmh, old: {result.Old?.KilometersPerHour:n1}kmh");
        },
        null
        );
    anemometer.Subscribe(observer);

    return Task.CompletedTask;
}

public override Task Run()
{
    // start raising updates
    anemometer.StartUpdating();
    Resolver.Log.Info("Hardware initialized.");

    return Task.CompletedTask;
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
