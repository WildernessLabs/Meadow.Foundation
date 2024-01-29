# Meadow.Foundation.Sensors.Temperature.Mcp960x

**Pct2075 I2C temperature sensor**

The **Pct2075** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
private Pct2075 sensor;

public override Task Initialize()
{
    sensor = new Pct2075(Device.CreateI2cBus());

    sensor.Updated += OnUpdated;
    sensor.StartUpdating(TimeSpan.FromSeconds(1));

    return base.Initialize();
}

private void OnUpdated(object sender, IChangeResult<Meadow.Units.Temperature> e)
{
    Debug.WriteLine($"Temp: {e.New.Celsius:n1}C  {e.New.Fahrenheit:n1}F");
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
