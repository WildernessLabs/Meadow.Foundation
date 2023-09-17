# Meadow.Foundation.Sensors.Atmospheric.Ccs811

**Ccs811 I2C VOC Air Quality Sensor**

The **Ccs811** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

## Usage

```csharp
Ccs811 sensor;

public override Task Initialize()
{
    Resolver.Log.Info("Initializing...");

    var i2cBus = Device.CreateI2cBus(Meadow.Hardware.I2cBusSpeed.Fast);
    sensor = new Ccs811(i2cBus);

    var consumer = Ccs811.CreateObserver(
        handler: result =>
        {
            Resolver.Log.Info($"Observer triggered:");
            Resolver.Log.Info($"   new CO2: {result.New.Co2?.PartsPerMillion:N1}ppm, old: {result.Old?.Co2?.PartsPerMillion:N1}ppm.");
            Resolver.Log.Info($"   new VOC: {result.New.Voc?.PartsPerBillion:N1}ppb, old: {result.Old?.Voc?.PartsPerBillion:N1}ppb.");
        },
        filter: result =>
        {
            //c# 8 pattern match syntax. checks for !null and assigns var.
            if (result.Old is { } old)
            {
                return (
                (result.New.Co2.Value - old.Co2.Value).Abs().PartsPerMillion > 1000 // 1000ppm
                  &&
                (result.New.Voc.Value - old.Voc.Value).Abs().PartsPerBillion > 100 // 100ppb
                );
            }
            return false;
        }
    );
    sensor.Subscribe(consumer);

    sensor.Updated += (sender, result) =>
    {
        Resolver.Log.Info($"CO2: {result.New.Co2.Value.PartsPerMillion:n1}ppm, VOC: {result.New.Voc.Value.PartsPerBillion:n1}ppb");
    };

    return Task.CompletedTask;
}

public override async Task Run()
{
    var result = await sensor.Read();
    Resolver.Log.Info("Initial Readings:");
    Resolver.Log.Info($"  CO2: {result.Co2.Value.PartsPerMillion:n1}ppm");
    Resolver.Log.Info($"  VOC: {result.Voc.Value.PartsPerBillion:n1}ppb");

    sensor.StartUpdating(TimeSpan.FromSeconds(1));
}

```
