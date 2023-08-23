# Meadow.Foundation.Sensors.Motion.Bmi270

**BMI270 I2C 6-axis accelerometer and motion sensor**

The **Bmi270** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all of Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

## Usage

```
Bmi270 bmi270;

public override Task Initialize()
{
    Console.WriteLine("Initialize hardware...");
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

    Console.WriteLine($"AccelX={accel.X.Gravity:0.##}g, AccelY={accel.Y.Gravity:0.##}g, AccelZ={accel.Z.Gravity:0.##}g, GyroX={gyro.X.RadiansPerMinute:0.##}rpm, GyroY={gyro.Y.RadiansPerMinute:0.##}rpm, GyroZ={gyro.Z.RadiansPerMinute:0.##}rpm, {temp.Celsius:0.##}C");
}

        
```

