# Meadow.Foundation.Sensors.Weather.SwitchingRainGauge

**GPIO rain gauge sensor**

The **RainGauge** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

## Usage

```
SwitchingRainGauge rainGauge;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    // initialize the rain gauge driver
    rainGauge = new SwitchingRainGauge(Device.Pins.D14);

    //==== Classic event example:
    rainGauge.Updated += (sender, result) => Resolver.Log.Info($"Updated event {result.New.Millimeters}mm");

    //==== IObservable Pattern
    var observer = SwitchingRainGauge.CreateObserver(
        handler: result => Resolver.Log.Info($"Rain depth: {result.New.Millimeters}mm"),
        filter: null
    );
    rainGauge.Subscribe(observer);

    return Task.CompletedTask;
}

public override async Task Run()
{
    // get initial reading, just to test the API - should be 0
    Length rainFall = await rainGauge.Read();
    Resolver.Log.Info($"Initial depth: {rainFall.Millimeters}mm");

    // start the sensor
    rainGauge.StartUpdating();
}

```
