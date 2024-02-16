# Meadow.Foundation.RTCs.Ds323x

**Ds323x I2C real time clock (DS3231)**

The **Ds323x** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
Ds3231 sensor;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    sensor = new Ds3231(Device.CreateI2cBus(), Device.Pins.D06);
    sensor.OnAlarm1Raised += Sensor_OnAlarm1Raised;

    return base.Initialize();
}

public override Task Run()
{
    sensor.CurrentDateTime = new DateTime(2020, 1, 1);

    Resolver.Log.Info($"Current time: {sensor.CurrentDateTime}");
    Resolver.Log.Info($"Temperature: {sensor.Temperature}");

    sensor.ClearInterrupt(Ds323x.Alarm.BothAlarmsRaised);

    sensor.SetAlarm(Ds323x.Alarm.Alarm1Raised,
        new DateTime(2020, 1, 1, 1, 0, 0),
        Ds323x.AlarmType.WhenSecondsMatch);

    sensor.DisplayRegisters();

    return base.Run();
}

private void Sensor_OnAlarm1Raised(object sender)
{
    var rtc = (Ds3231)sender;
    Resolver.Log.Info("Alarm 1 has been activated: " + rtc.CurrentDateTime.ToString("dd MMM yyyy HH:mm:ss"));
    rtc.ClearInterrupt(Ds323x.Alarm.Alarm1Raised);
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
