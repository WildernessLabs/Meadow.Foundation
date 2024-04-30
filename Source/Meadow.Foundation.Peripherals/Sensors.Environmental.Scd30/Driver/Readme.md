# Meadow.Foundation.Sensors.Environmental.Scd30

**SCD30 I2C C02, temperature, and relative humidity sensor**

The **Scd30** library is included in the **Meadow.Foundation.Sensors.Environmental.Scd30** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.Sensors.Environmental.Scd30`
## Usage

```csharp
Scd30 sensor;

public override Task Initialize()
{
    Resolver.Log.Info("Initializing...");

    var i2cBus = Device.CreateI2cBus(Meadow.Hardware.I2cBusSpeed.Standard);

    sensor = new Scd30(i2cBus);

    var consumer = Scd30.CreateObserver(
        handler: result =>
        {
            Resolver.Log.Info($"Observer: Temp changed by threshold; new temp: {result.New.Temperature?.Celsius:N2}C, old: {result.Old?.Temperature?.Celsius:N2}C");
        },
        filter: result =>
        {
            if (result.Old?.Temperature is { } oldTemp &&
                result.Old?.Humidity is { } oldHumidity &&
                result.New.Temperature is { } newTemp &&
                result.New.Humidity is { } newHumidity)
            {
                return ((newTemp - oldTemp).Abs().Celsius > 0.5 &&
                        (newHumidity - oldHumidity).Percent > 0.05);
            }
            return false;
        }
    );

    sensor?.Subscribe(consumer);

    if (sensor != null)
    {
        sensor.Updated += (sender, result) =>
        {
            Resolver.Log.Info($"  Concentration: {result.New.Concentration?.PartsPerMillion:N0}ppm");
            Resolver.Log.Info($"  Temperature: {result.New.Temperature?.Celsius:N1}C");
            Resolver.Log.Info($"  Relative Humidity: {result.New.Humidity:N0}%");
        };
    }

    //The value 5 is used purely for demonstration purposes. 2 is the sensor default.
    sensor?.SetMeasurementInterval(5);
    var measurementInterval = sensor?.GetMeasurementInterval();
    Resolver.Log.Info($"Measurement Interval: {measurementInterval}sec");

    var autoSelfCalibration = sensor?.SelfCalibrationEnabled();
    Resolver.Log.Info($"Auto Self Calibration: {autoSelfCalibration}");

    var ambientPressure = sensor?.GetAmbientPressureOffset();
    Resolver.Log.Info($"Ambient Pressure offset: {ambientPressure}mbar");

    var altitudeOffset = sensor?.GetAltitudeOffset();
    Resolver.Log.Info($"Altitude Offset: {altitudeOffset}m");

    var temperatureOffset = sensor?.GetTemperatureOffset();
    Resolver.Log.Info($"Temperature Offset: {temperatureOffset}C");

    var forceCalibrationValue = sensor?.GetForceCalibrationValue();
    Resolver.Log.Info($"Force Calibration Value: {forceCalibrationValue}");

    sensor?.StartUpdating(TimeSpan.FromSeconds(5));

    return base.Initialize();
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


