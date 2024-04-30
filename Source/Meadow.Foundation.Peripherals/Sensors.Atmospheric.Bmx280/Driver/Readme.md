# Meadow.Foundation.Sensors.Atmospheric.Bmx280

**Bosch BMx280 SPI and I2C family of atmospheric sensor**

The **Bmx280** library is included in the **Meadow.Foundation.Sensors.Atmospheric.Bmx280** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.Sensors.Atmospheric.Bmx280`
## Usage

```csharp
protected Bme280 sensor;

protected virtual IPin SpiChipSelect { get; }

public override Task Initialize()
{
    Resolver.Log.Info("Initializing...");

    //CreateSpiSensor();
    CreateI2CSensor();

    var consumer = Bme280.CreateObserver(
        handler: result =>
        {
            Resolver.Log.Info($"Observer: Temp changed by threshold; new temp: {result.New.Temperature?.Celsius:N1}C, old: {result.Old?.Temperature?.Celsius:N1}C");
        },
        filter: result =>
        {
            if (result.Old?.Temperature is { } oldTemp &&
                result.Old?.Humidity is { } oldHumidity &&
                result.New.Temperature is { } newTemp &&
                result.New.Humidity is { } newHumidity)
            {
                return
                (newTemp - oldTemp).Abs().Celsius > 0.5 &&
                (newHumidity - oldHumidity).Percent > 0.05;
            }
            return false;
        }
    );
    sensor.Subscribe(consumer);

    sensor.Updated += (sender, result) =>
    {
        try
        {
            Resolver.Log.Info($"  Temperature: {result.New.Temperature?.Celsius:N1}C");
            Resolver.Log.Info($"  Relative Humidity: {result.New.Humidity:N1}%");
            Resolver.Log.Info($"  Pressure: {result.New.Pressure?.Millibar:N1}mbar ({result.New.Pressure?.Pascal:N1}Pa)");
        }
        catch (Exception ex)
        {
            Resolver.Log.Error(ex, "Error reading sensor");
        }
    };

    return Task.CompletedTask;
}

public override async Task Run()
{
    var conditions = await sensor.Read();
    Resolver.Log.Info("Initial Readings:");
    Resolver.Log.Info($"  Temperature: {conditions.Temperature?.Celsius:N1}C");
    Resolver.Log.Info($"  Pressure: {conditions.Pressure?.Bar:N1}hPa");
    Resolver.Log.Info($"  Relative Humidity: {conditions.Humidity?.Percent:N1}%");

    sensor.StartUpdating(TimeSpan.FromSeconds(1));
}

private void CreateSpiSensor()
{
    Resolver.Log.Info("Create BME280 sensor with SPI...");

    var spi = Device.CreateSpiBus();
    sensor = new Bme280(spi, SpiChipSelect.CreateDigitalOutputPort());
}

private void CreateI2CSensor()
{
    Resolver.Log.Info("Create BME280 sensor with I2C...");

    var i2c = Device.CreateI2cBus();
    sensor = new Bme280(i2c, (byte)Bmx280.Addresses.Default); // SDA pulled up

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


