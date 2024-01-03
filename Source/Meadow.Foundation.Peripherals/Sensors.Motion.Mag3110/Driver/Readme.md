# Meadow.Foundation.Sensors.Motion.Mag3110

**Freescale MAG3110 I2C 3 axis magnetometer**

The **MAG3110** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
Mag3110 sensor;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    sensor = new Mag3110(Device.CreateI2cBus());

    // classical .NET events can  be used
    sensor.Updated += (sender, result) =>
    {
        Resolver.Log.Info($"Magnetic Field: [X:{result.New.MagneticField3D?.X.MicroTesla:N2}," +
            $"Y:{result.New.MagneticField3D?.Y.MicroTesla:N2}," +
            $"Z:{result.New.MagneticField3D?.Z.MicroTesla:N2} (microTeslas)]");

        Resolver.Log.Info($"Temp: {result.New.Temperature?.Celsius:N2}C");
    };

    // Example that uses an IObservable subscription to only be notified when the filter is satisfied
    var consumer = Mag3110.CreateObserver(
        handler: result => Resolver.Log.Info($"Observer: [x] changed by threshold; new [x]: X:{result.New.MagneticField3D?.X.MicroTesla:N2}, old: X:{result.Old?.MagneticField3D?.X.MicroTesla:N2}"),
        // only notify if there's a greater than 1 micro tesla on the Y axis
        filter: result =>
        {
            if (result.Old is { } old)
            {
                return ((result.New.MagneticField3D - old.MagneticField3D)?.Y > new MagneticField(1, MU.MicroTesla));
            }
            return false;
        });
    sensor.Subscribe(consumer);

    return Task.CompletedTask;
}

public async override Task Run()
{
    var result = await sensor.Read();
    Resolver.Log.Info("Initial Readings:");
    Resolver.Log.Info($"Magnetic field: [X:{result.MagneticField3D?.X.MicroTesla:N2}," +
        $"Y:{result.MagneticField3D?.Y.MicroTesla:N2}," +
        $"Z:{result.MagneticField3D?.Z.MicroTesla:N2} (microTeslas)]");

    Resolver.Log.Info($"Temp: {result.Temperature?.Celsius:N2}C");

    sensor.StartUpdating(TimeSpan.FromMilliseconds(500));
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
