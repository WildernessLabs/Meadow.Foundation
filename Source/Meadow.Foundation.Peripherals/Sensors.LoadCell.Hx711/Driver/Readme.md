# Meadow.Foundation.Sensors.LoadCell.Hx711

**Hx711 digital load cell amplifier**

The **Hx711** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
Hx711 loadSensor;

public int CalibrationFactor { get; set; } = 0; //9834945 - 8458935; // TODO: change this based on your scale (using the method provided below)
public double CalibrationWeight { get; set; } = 1.6; // TODO: enter the known-weight (in units below) you used in calibration

public override async Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    loadSensor = new Hx711(Device.Pins.D04, Device.Pins.D03);

    if (CalibrationFactor == 0)
    {
        GetAndDisplayCalibrationUnits(loadSensor);
    }
    else
    {   // wait for the ADC to settle
        await Task.Delay(500);

        // Set the current load to be zero
        loadSensor.SetCalibrationFactor(CalibrationFactor, new Mass(CalibrationWeight, Mass.UnitType.Grams));
        loadSensor.Tare();
    }

    loadSensor.MassUpdated += (sender, values) => Resolver.Log.Info($"Mass is now returned {values.New.Grams:N2}g");
}

public override Task Run()
{
    loadSensor.StartUpdating(TimeSpan.FromSeconds(2));

    return Task.CompletedTask;
}

public void GetAndDisplayCalibrationUnits(Hx711 sensor)
{   // first notify the user we're starting
    Resolver.Log.Info($"Beginning Calibration. First we'll tare (set a zero).");
    Resolver.Log.Info($"Make sure scale bed is clear. Next step in 5 seconds...");
    Thread.Sleep(5000);
    sensor.Tare();
    Resolver.Log.Info($"Place a known weight on the scale. Next step in 5 seconds...");
    Thread.Sleep(5000);
    var factor = sensor.CalculateCalibrationFactor();
    Resolver.Log.Info($"Your scale's Calibration Factor is: {factor}.  Enter this into the code for future use.");
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
