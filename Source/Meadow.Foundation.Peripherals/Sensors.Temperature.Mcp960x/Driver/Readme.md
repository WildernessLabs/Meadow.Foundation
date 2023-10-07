# Meadow.Foundation.Sensors.Temperature.Mcp960x

**Mcp960x I2C thermocouple temperature sensor**

The **Mcp960x** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
Mcp9600 sensor;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    sensor = new Mcp9600(Device.CreateI2cBus());

    var consumer = Mcp9600.CreateObserver(
        handler: result =>
        {
            Resolver.Log.Info($"Temperature New Value {result.New.TemperatureHot.Value.Celsius}C");
        },
        filter: null
    );
    sensor.Subscribe(consumer);

    sensor.Updated += Sensor_Updated;
    return Task.CompletedTask;
}

private void Sensor_Updated(object sender, IChangeResult<(Meadow.Units.Temperature? TemperatureHot, Meadow.Units.Temperature? TemperatureCold)> e)
{
    Resolver.Log.Info($"Temperature hot: {e.New.TemperatureHot.Value.Celsius:n2}C, Temperature cold: {e.New.TemperatureCold.Value.Celsius:n2}C");
}

public override async Task Run()
{
    sensor.StartUpdating();
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
