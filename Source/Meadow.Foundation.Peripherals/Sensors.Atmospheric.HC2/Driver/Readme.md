# Meadow.Foundation.Sensors.Atmospheric.HC2

**HC2 Humidity Probe**

The **HC2** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all of Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

## Usage

```
HC2 sensor;

public override Task Initialize()
{
    Resolver.Log.Info("Initializing...");

    //Analog
    //sensor = new HC2(Device.Pins.A00, Device.Pins.A01);

    //Serial
    sensor = new HC2(Device, Device.PlatformOS.GetSerialPortName("COM4"));

    var consumer = HC2.CreateObserver(
        handler: result =>
        {
            Resolver.Log.Info($"Observer: Temp changed by threshold; new temp: {result.New.Temperature?.Celsius:N2}C, old: {result.Old?.Temperature?.Celsius:N2}C");
        },
        filter: result =>
        {
            //c# 8 pattern match syntax. checks for !null and assigns var.
            if (result.Old is { } old)
            {
                return (
                (result.New.Temperature.Value - old.Temperature.Value).Abs().Celsius > 0.5);
            }
            return false;
        }
    );
    sensor.Subscribe(consumer);

    sensor.Updated += (sender, result) =>
    {
        Resolver.Log.Info($"  Relative Humidity: {result.New.Humidity?.Percent:N2}%");
        Resolver.Log.Info($"  Temperature: {result.New.Temperature?.Celsius:N2}C");
    };

    return Task.CompletedTask;
}

public override async Task Run()
{
    var conditions = await sensor.Read();
    Resolver.Log.Info($"Relative Humidity: {conditions.Humidity?.Percent}%, Temperature: {conditions.Temperature?.Celsius}°C");

    sensor.StartUpdating(TimeSpan.FromSeconds(5));
}

        
```

