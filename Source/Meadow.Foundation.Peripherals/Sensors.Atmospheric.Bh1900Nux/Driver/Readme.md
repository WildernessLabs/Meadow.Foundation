# Meadow.Foundation.Sensors.Atmospheric.Bh1900Nux

**Rohm BH1900NUX I2C temperature sensor**

The **Bh1900Nux** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

## Usage

```csharp
private Bh1900Nux _sensor;

public override Task Initialize()
{
    Resolver.Log.Info("Initializing...");

    _sensor = new Bh1900Nux(Device.CreateI2cBus(), Bh1900Nux.Addresses.Default);

    var consumer = Bh1900Nux.CreateObserver(
        handler: result =>
        {
            Resolver.Log.Info($"Observer: Temp changed by threshold; new temp: {result.New.Celsius:N2}C, old: {result.Old?.Celsius:N2}C");
        },
        filter: result =>
        {
            //c# 8 pattern match syntax. checks for !null and assigns var.
            if (result.Old is { } old)
            {
                return (result.New - old).Abs().Celsius > 0.5;
            }
            return false;
        }
    );
    _sensor.Subscribe(consumer);

    _sensor.Updated += (sender, result) =>
    {
        Resolver.Log.Info($"  Temperature: {result.New.Celsius:N2}C");
    };
   
    return Task.CompletedTask;
}

public async override Task Run()
{
    var conditions = await _sensor.Read();
    Resolver.Log.Info("Initial Readings:");
    Resolver.Log.Info($"  Temperature: {conditions.Celsius:N2}C");

    _sensor.StartUpdating(TimeSpan.FromSeconds(1));
}

```
