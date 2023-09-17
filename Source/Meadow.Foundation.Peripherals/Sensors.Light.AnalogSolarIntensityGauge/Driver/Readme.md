# Meadow.Foundation.Sensors.Light.AnalogSolarIntensityGauge

**Analog solar intensity sensor**

The **AnalogSolarIntensityGauge** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

## Usage

```csharp
AnalogSolarIntensityGauge solarGauge;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    solarGauge = new AnalogSolarIntensityGauge(Device.Pins.A02, updateInterval: TimeSpan.FromSeconds(1));

    //==== classic .NET Event
    solarGauge.SolarIntensityUpdated += (s, result) => Resolver.Log.Info($"SolarIntensityUpdated: {result.New * 100:n2}%");

    //==== Filterable observer
    var observer = AnalogSolarIntensityGauge.CreateObserver(
        handler: result => Resolver.Log.Info($"Observer filter satisfied, new intensity: {result.New * 100:n2}%"),
        filter: result =>
        {
            if (result.Old is { } old)
            {
                return (Math.Abs(result.New - old) > 0.05); // only notify if change is > 5%
            }
            return false;
        });
    solarGauge.Subscribe(observer);

    return Task.CompletedTask;
}

public override async Task Run()
{
    var result = await solarGauge.Read();
    Resolver.Log.Info($"Solar Intensity: {result * 100:n2}%");

    solarGauge.StartUpdating(TimeSpan.FromSeconds(1));
}

```
