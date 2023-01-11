using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using System;
using System.Threading.Tasks;

namespace Sensors.Atmospheric.BME680_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

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
                    //c# 8 pattern match syntax. checks for !null and assigns var.
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

            if(sensor != null)
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
            if(sensor != null)
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
            if(sensor == null) { return; }

            var (Temperature, Humidity, Pressure, Resistance) = await sensor.Read();

            Resolver.Log.Info("Initial Readings:");
            Resolver.Log.Info($"  Temperature: {Temperature?.Celsius:N2}C");
            Resolver.Log.Info($"  Pressure: {Pressure?.Hectopascal:N2}hPa");
            Resolver.Log.Info($"  Relative Humidity: {Humidity?.Percent:N2}%");
            Resolver.Log.Info($"  Gas Resistance: {Resistance?.Ohms:N0}Ohms");
        }

        //<!=SNOP=>
    }
}