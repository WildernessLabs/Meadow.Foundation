# Meadow.Foundation.Sensors.Light.Veml7700

**Veml7700 I2C high accuracy ambient light sensor**

The **Veml7700** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
Veml7700 sensor;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    sensor = new Veml7700(Device.CreateI2cBus());
    sensor.DataSource = Veml7700.SensorTypes.Ambient;
    
    sensor.RangeExceededHigh += (s, a) => Resolver.Log.Info("Too bright to measure");
    sensor.RangeExceededLow += (s, a) => Resolver.Log.Info("Too dim to measure");

    // classical .NET events can also be used:
    sensor.Updated += (sender, result) => Resolver.Log.Info($"Illuminance: {result.New.Lux:n3}Lux");

    return Task.CompletedTask;
}

public override async Task Run()
{
    var conditions = await sensor.Read();

    Resolver.Log.Info("Initial Readings:");
    Resolver.Log.Info($"  Illuminance: {conditions.Lux:n3}Lux");

    sensor.StartUpdating(TimeSpan.FromSeconds(1));
}
    
```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
