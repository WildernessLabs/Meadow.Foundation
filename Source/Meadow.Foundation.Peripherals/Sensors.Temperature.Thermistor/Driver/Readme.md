# Meadow.Foundation.Sensors.Temperature.Thermistor

**Thermistor analog temperature sensor**

The **Thermistor** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all of Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

## Usage

```
private SteinhartHartCalculatedThermistor thermistor;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    thermistor = new SteinhartHartCalculatedThermistor(Device.CreateAnalogInputPort(Device.Pins.A00), new Resistance(10, Meadow.Units.Resistance.UnitType.Kiloohms));

    var consumer = SteinhartHartCalculatedThermistor.CreateObserver(
        handler: result =>
        {
            Resolver.Log.Info($"Temperature New Value {result.New.Fahrenheit:N1}F/{result.New.Celsius:N1}C");
            Resolver.Log.Info($"Temperature Old Value {result.Old?.Fahrenheit:N1}F/{result.Old?.Celsius:N1}C");
        },
        filter: null
    );
    thermistor.Subscribe(consumer);

    thermistor.TemperatureUpdated += (object sender, IChangeResult<Meadow.Units.Temperature> e) =>
    {
        Resolver.Log.Info($"Temperature Updated: {e.New.Fahrenheit:N1}F/{e.New.Celsius:N1}C");
    };

    return Task.CompletedTask;
}

public override async Task Run()
{
    var temp = await thermistor.Read();
    Resolver.Log.Info($"Current temperature: {temp.Fahrenheit:N1}F/{temp.Celsius:N1}C");

    thermistor.StartUpdating(TimeSpan.FromSeconds(1));
}

        
```

