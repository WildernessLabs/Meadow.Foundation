# Meadow.Foundation.Sensors.Distance.Vl53l0x

**Vl53l0x I2C time of flight ranging distance sensor**

The **Vl53l0x** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
Vl53l0x sensor;

public override Task Initialize()
{
    Resolver.Log.Info("Initializing hardware...");

    var i2cBus = Device.CreateI2cBus(I2cBusSpeed.FastPlus);
    sensor = new Vl53l0x(i2cBus);

    sensor.Updated += Sensor_Updated;

    return Task.CompletedTask;
}

public override Task Run()
{
    sensor.StartUpdating(TimeSpan.FromMilliseconds(250));

    return Task.CompletedTask;
}

private void Sensor_Updated(object sender, IChangeResult<Length> result)
{
    if (result.New == null) { return; }

    if (result.New < new Length(0, LU.Millimeters))
    {
        Resolver.Log.Info("out of range.");
    }
    else
    {
        Resolver.Log.Info($"{result.New.Millimeters}mm / {result.New.Inches:n3}\"");
    }
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
