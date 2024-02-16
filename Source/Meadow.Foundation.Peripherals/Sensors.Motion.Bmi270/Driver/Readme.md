# Meadow.Foundation.Sensors.Motion.Bmi270

**BMI270 I2C 6-axis accelerometer and motion sensor**

The **Bmi270** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
Bmi270 bmi270;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize hardware...");
    bmi270 = new Bmi270(Device.CreateI2cBus());

    // classical .NET events can also be used:
    bmi270.Updated += HandleResult;

    // Example that uses an IObservable subscription to only be notified when the filter is satisfied
    var consumer = Bmi270.CreateObserver(handler: result => HandleResult(this, result),
                                         filter: result => FilterResult(result));

    bmi270.Subscribe(consumer);

    bmi270.StartUpdating(TimeSpan.FromMilliseconds(2000));

    return base.Initialize();
}

bool FilterResult(IChangeResult<(Acceleration3D? Acceleration3D,
                                 AngularVelocity3D? AngularVelocity3D,
                                 Temperature? Temperature)> result)
{
    return result.New.Acceleration3D.Value.Z > new Acceleration(0.1, Acceleration.UnitType.Gravity);
}

void HandleResult(object sender,
    IChangeResult<(Acceleration3D? Acceleration3D,
    AngularVelocity3D? AngularVelocity3D,
    Temperature? Temperature)> result)
{
    var accel = result.New.Acceleration3D.Value;
    var gyro = result.New.AngularVelocity3D.Value;
    var temp = result.New.Temperature.Value;

    Resolver.Log.Info($"AccelX={accel.X.Gravity:0.##}g, AccelY={accel.Y.Gravity:0.##}g, AccelZ={accel.Z.Gravity:0.##}g, GyroX={gyro.X.RadiansPerMinute:0.##}rpm, GyroY={gyro.Y.RadiansPerMinute:0.##}rpm, GyroZ={gyro.Z.RadiansPerMinute:0.##}rpm, {temp.Celsius:0.##}C");
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
