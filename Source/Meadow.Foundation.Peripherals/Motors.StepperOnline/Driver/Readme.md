# Meadow.Foundation.Sensors.Atmospheric.Bme68x

**Bosch BME68x SPI / I2C humidity, barometric pressure, ambient temperature and gas (VOC) sensor**

The **Bme68x** library is included in the **Meadow.Foundation.Sensors.Atmospheric.Bme68x** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.Sensors.Atmospheric.Bme68x`
## Usage

```csharp
Bme680? sensor;

public override Task Initialize()
{
    Resolver.Log.Info("Initializing...");

    //CreateSpiSensor();
    CreateI2CSensor();

    //uncomment to enable on sensor heater for gas readings
    //EnableGasHeater();

    var consumer = Bme680.CreateObserver(
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
            Resolver.Log.Info($"  Temperature: {result.New.Temperature?.Celsius:N2}C");
            Resolver.Log.Info($"  Relative Humidity: {result.New.Humidity:N2}%");
            Resolver.Log.Info($"  Pressure: {result.New.Pressure?.Millibar:N2}mbar ({result.New.Pressure?.Pascal:N2}Pa)");
            if (sensor.GasConversionIsEnabled)
            {
                Resolver.Log.Info($"  Gas Resistance: {result.New.GasResistance:N0}Ohms");
            }
        };
    }

    sensor?.StartUpdating(TimeSpan.FromSeconds(2));

    ReadConditions().Wait();

    return base.Initialize();
}

void EnableGasHeater()
{
    if (sensor != null)
    {
        sensor.GasConversionIsEnabled = true;
        sensor.HeaterIsEnabled = true;
        sensor.ConfigureHeatingProfile(Bme688.HeaterProfileType.Profile1, new Meadow.Units.Temperature(300), TimeSpan.FromMilliseconds(100), new Meadow.Units.Temperature(22));
        sensor.HeaterProfile = Bme688.HeaterProfileType.Profile1;
    }
}

void CreateSpiSensor()
{
    Resolver.Log.Info("Create BME680 sensor with SPI...");

    var spiBus = Device.CreateSpiBus();
    sensor = new Bme680(spiBus, Device.CreateDigitalOutputPort(Device.Pins.D14));
}

void CreateI2CSensor()
{
    Resolver.Log.Info("Create BME680 sensor with I2C...");

    var i2c = Device.CreateI2cBus();
    sensor = new Bme680(i2c, (byte)Bme688.Addresses.Address_0x76);
}

async Task ReadConditions()
{
    if (sensor == null) { return; }

    var (Temperature, Humidity, Pressure, Resistance) = await sensor.Read();

    Resolver.Log.Info("Initial Readings:");
    Resolver.Log.Info($"  Temperature: {Temperature?.Celsius:N2}C");
    Resolver.Log.Info($"  Pressure: {Pressure?.Hectopascal:N2}hPa");
    Resolver.Log.Info($"  Relative Humidity: {Humidity?.Percent:N2}%");
    Resolver.Log.Info($"  Gas Resistance: {Resistance?.Ohms:N0}Ohms");
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


