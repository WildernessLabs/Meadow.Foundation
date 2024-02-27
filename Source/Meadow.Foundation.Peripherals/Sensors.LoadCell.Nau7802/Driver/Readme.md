# Meadow.Foundation.Sensors.LoadCell.Nau7802

**NAU7802 I2C 24-Bit dual channel analog to digital converter**

The **Nau7802** library is included in the **Meadow.Foundation.Sensors.LoadCell.Nau7802** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.Sensors.LoadCell.Nau7802`
## Usage

```csharp
private Nau7802 loadSensor;

public int CalibrationFactor { get; set; } = 16526649; // TODO: change this based on your scale (using the method provided below)
public Mass CalibrationWeight { get; set; } = new Mass(1600, Mass.UnitType.Grams); // TODO: enter the known-weight you used in calibration

public override async Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    loadSensor = new Nau7802(Device.CreateI2cBus());

    if (CalibrationFactor == 0)
    {
        await GetAndDisplayCalibrationUnits(loadSensor);
    }
    else
    {   // wait for the ADC to settle
        await Task.Delay(500);

        // Set the current load to be zero
        loadSensor.SetCalibrationFactor(CalibrationFactor, CalibrationWeight);
        loadSensor.Tare();
    }

    loadSensor.Updated += (sender, values) => Resolver.Log.Info($"Mass is now returned {values.New.Grams:N2}g");
}

public override Task Run()
{
    loadSensor.StartUpdating(TimeSpan.FromSeconds(2));

    return Task.CompletedTask;
}

public async Task GetAndDisplayCalibrationUnits(Nau7802 sensor)
{
    // first notify the user we're starting
    Resolver.Log.Info($"Beginning Calibration. First we'll tare (set a zero).");
    Resolver.Log.Info($"Make sure scale bed is clear. Next step in 5 seconds...");

    await Task.Delay(5000);
    sensor.Tare();
    Resolver.Log.Info($"Place a known weight on the scale. Next step in 5 seconds...");

    await Task.Delay(500);
    var factor = sensor.CalculateCalibrationFactor();

    Resolver.Log.Info($"Your scale's Calibration Factor is: {factor}. Enter this into the code for future use.");
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
## About Meadow

Meadow is a complete, IoT platform with defense-grade security that runs full .NET applications on embeddable microcontrollers and Linux single-board computers including Raspberry Pi and NVIDIA Jetson.

### Build

Use the full .NET platform and tooling such as Visual Studio and plug-and-play hardware drivers to painlessly build IoT solutions.

### Connect

Utilize native support for WiFi, Ethernet, and Cellular connectivity to send sensor data to the Cloud and remotely control your peripherals.

### Deploy

Instantly deploy and manage your fleet in the cloud for OtA, health-monitoring, logs, command + control, and enterprise backend integrations.


