using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Hardware;
using System;
using System.Threading.Tasks;

namespace Sensors.Atmospheric.BME280_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Bme280 sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

            //CreateSpiSensor();
            CreateI2CSensor();

            var consumer = Bme280.CreateObserver(
                handler: result =>
                {
                    Resolver.Log.Info($"Observer: Temp changed by threshold; new temp: {result.New.Temperature?.Celsius:N2}C, old: {result.Old?.Temperature?.Celsius:N2}C");
                },
                filter: result =>
                {
                    //c# 8 pattern match syntax. checks for !null and assigns var.
                    if (result.Old is { } old)
                    {
                        return (
                        (result.New.Temperature.Value - old.Temperature.Value).Abs().Celsius > 0.5
                        &&
                        (result.New.Humidity.Value - old.Humidity.Value).Percent > 0.05
                        );
                    }
                    return false;
                }
            );
            sensor.Subscribe(consumer);

            sensor.Updated += (sender, result) =>
            {
                Resolver.Log.Info($"  Temperature: {result.New.Temperature?.Celsius:N2}C");
                Resolver.Log.Info($"  Relative Humidity: {result.New.Humidity:N2}%");
                Resolver.Log.Info($"  Pressure: {result.New.Pressure?.Millibar:N2}mbar ({result.New.Pressure?.Pascal:N2}Pa)");
            };

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            var conditions = await sensor.Read();
            Resolver.Log.Info("Initial Readings:");
            Resolver.Log.Info($"  Temperature: {conditions.Temperature?.Celsius:N2}C");
            Resolver.Log.Info($"  Pressure: {conditions.Pressure?.Bar:N2}hPa");
            Resolver.Log.Info($"  Relative Humidity: {conditions.Humidity?.Percent:N2}%");

            sensor.StartUpdating(TimeSpan.FromSeconds(1));
        }

        void CreateSpiSensor()
        {
            Resolver.Log.Info("Create BME280 sensor with SPI...");

            var spi = Device.CreateSpiBus();
            sensor = new Bme280(spi, Device.Pins.D00.CreateDigitalOutputPort());
        }

        void CreateI2CSensor()
        {
            Resolver.Log.Info("Create BME280 sensor with I2C...");

            var i2c = Device.CreateI2cBus();
            sensor = new Bme280(i2c, (byte)Bme280.Addresses.Default); // SDA pulled up

        }

        //<!=SNOP=>
    }
}