# Meadow.Foundation.Sensors.Weather.WindVane

**WindVane analog wind direction sensor**

The **WindVane** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all of Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

## Usage

```
WindVane windVane;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    // initialize the wind vane driver
    windVane = new WindVane(Device.Pins.A00);

    //==== Classic event example:
    windVane.Updated += (sender, result) => Resolver.Log.Info($"Updated event {result.New.DecimalDegrees}");

    //==== IObservable Pattern
    var observer = WindVane.CreateObserver(
        handler: result => Resolver.Log.Info($"Wind Direction: {result.New.Compass16PointCardinalName}"),
        filter: null
    );
    windVane.Subscribe(observer);

    return Task.CompletedTask;
}

public override async Task Run()
{
    // get initial reading, just to test the API
    Azimuth azi = await windVane.Read();
    Resolver.Log.Info($"Initial azimuth: {azi.Compass16PointCardinalName}");

    // start updating
    windVane.StartUpdating(TimeSpan.FromSeconds(1));
}

        
```

