# Meadow.Foundation.Sensors.Environmental.AtlasScientificGravityDOMeter

**Atlas Scientific analog gravity dissolved oxygen sensor**

The **AtlasScientificGravityDOMeter** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
AtlasScientificGravityDOMeter sensor;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    sensor = new AtlasScientificGravityDOMeter(Device.Pins.A01);
    sensor.CalibrationInAir = new Voltage(0.04, Voltage.UnitType.Volts);

    // Example that uses an IObservable subscription to only be notified when the saturation changes
    var consumer = AtlasScientificGravityDOMeter.CreateObserver(
        handler: result =>
        {
            string oldValue = (result.Old is { } old) ? $"{old * 100:n1}" : "n/a";
            string newValue = $"{result.New * 100:n1}";
            Resolver.Log.Info($"New: {newValue}%, Old: {oldValue}%");
        },
        filter: null
    );
    sensor.Subscribe(consumer);

    // optional classical .NET events can also be used:
    sensor.SaturationUpdated += (sender, result) =>
    {
        //    string oldValue = (result.Old is { } old) ? $"{old * 100:n0}%" : "n/a";
        //    Resolver.Log.Info($"Updated - New: {result.New * 100:n0}%, Old: {oldValue}");
    };

    return Task.CompletedTask;
}

public override async Task Run()
{
    Resolver.Log.Info("Run...");

    await ReadSensor();

    //example calibration setting, ensure the sensor is set up for calibration 
    var calibrationVoltage = await sensor.GetCurrentVoltage();
    sensor.CalibrationInAir = calibrationVoltage;

    Resolver.Log.Info($"Calibration voltage: {calibrationVoltage.Volts}V");

    sensor.StartUpdating(TimeSpan.FromSeconds(2));
}

protected async Task ReadSensor()
{
    var saturation = await sensor.Read();
    Resolver.Log.Info($"Initial saturation: {saturation * 100:N1}%");
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
