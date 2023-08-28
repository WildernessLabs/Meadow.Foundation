# Meadow.Foundation.Sensors.Light.Bh1745

**Bh1745 I2C luminance and color light sensor**

The **Bh1745** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all of Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

## Usage

```
Bh1745 sensor;
RgbPwmLed rgbLed;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    sensor = new Bh1745(Device.CreateI2cBus());

    // instantiate our onboard LED that we'll show the color with
    rgbLed = new RgbPwmLed(
        Device.Pins.OnboardLedRed,
        Device.Pins.OnboardLedGreen,
        Device.Pins.OnboardLedBlue,
        commonType: CommonType.CommonAnode);

    // Example that uses an IObservable subscription to only be notified
    var consumer = Bh1745.CreateObserver(
        handler: result => Resolver.Log.Info($"Observer: filter satisifed: {result.New.AmbientLight?.Lux:N2}Lux, old: {result.Old?.AmbientLight?.Lux:N2}Lux"),

        // only notify if the visible light changes by 100 lux (put your hand over the sensor to trigger)
        filter: result =>
        {
            if (result.Old is { } old)
            { //c# 8 pattern match syntax. checks for !null and assigns var.
                // returns true if > 100lux change
                return ((result.New.AmbientLight.Value - old.AmbientLight.Value).Abs().Lux > 100);
            }
            return false;
        });

    sensor.Subscribe(consumer);

    //classical .NET events can also be used:
    sensor.Updated += (sender, result) =>
    {
        Resolver.Log.Info($"  Ambient Light: {result.New.AmbientLight?.Lux:N2}Lux");
        Resolver.Log.Info($"  Color: {result.New.Color}");

        if (result.New.Color is { } color)
        {
            rgbLed.SetColor(color);
        }
    };

    return Task.CompletedTask;
}

public override async Task Run()
{
    var result = await sensor.Read();

    Resolver.Log.Info("Initial Readings:");
    Resolver.Log.Info($" Visible Light: {result.AmbientLight?.Lux:N2}Lux");
    Resolver.Log.Info($" Color: {result.Color}");

    if (result.Color is { } color)
    {
        rgbLed.SetColor(color);
    }

    sensor.StartUpdating(TimeSpan.FromSeconds(1));
}

        
```

