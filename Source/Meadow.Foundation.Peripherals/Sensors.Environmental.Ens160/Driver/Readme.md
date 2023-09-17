# Meadow.Foundation.Sensors.Environmental.Ens160

**ENS160 I2C C02, Ethanol and AQI sensor**

The **Ens160** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

## Usage

```csharp
Ens160 sensor;

public override Task Initialize()
{
    Resolver.Log.Info("Initializing...");

    var i2cBus = Device.CreateI2cBus(Meadow.Hardware.I2cBusSpeed.Standard);

    sensor = new Ens160(i2cBus, (byte)Ens160.Addresses.Address_0x53);

    var consumer = Ens160.CreateObserver(
        handler: result =>
        {
            Resolver.Log.Info($"Observer: C02 concentration changed by threshold; new: {result.New.CO2Concentration?.PartsPerMillion:N0}ppm");
        },
        filter: result =>
        {
            if (result.Old?.CO2Concentration is { } oldCon &&
                result.New.CO2Concentration is { } newCon)
            {
                return Math.Abs((newCon - oldCon).PartsPerMillion) > 10;
            }
            return false;
        }
    );

    sensor?.Subscribe(consumer);

    if (sensor != null)
    {
        sensor.Updated += (sender, result) =>
        {
            Resolver.Log.Info($"  CO2 Concentration: {result.New.CO2Concentration?.PartsPerMillion:N0}ppm");
            Resolver.Log.Info($"  Ethanol Concentration: {result.New.EthanolConcentration?.PartsPerBillion:N0}ppb");
            Resolver.Log.Info($"  TVOC Concentration: {result.New.TVOCConcentration?.PartsPerBillion:N0}ppb");
            Resolver.Log.Info($"  AQI: {sensor.GetAirQualityIndex()}");
        };
    }

    sensor?.StartUpdating(TimeSpan.FromSeconds(2));

    return base.Initialize();
}

```
