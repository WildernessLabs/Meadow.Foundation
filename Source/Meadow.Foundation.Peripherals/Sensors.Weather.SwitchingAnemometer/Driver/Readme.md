# Meadow.Foundation.Sensors.Weather.SwitchingAnemometer

**Digital Switching Anemometer wind speed sensor**

The **SwitchingAnemometer** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all of Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

## Usage

```
SwitchingAnemometer anemometer;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    anemometer = new SwitchingAnemometer(Device.Pins.A01);

    //==== classic events example
    anemometer.WindSpeedUpdated += (sender, result) =>
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

