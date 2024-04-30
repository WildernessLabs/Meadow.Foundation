using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using System;
using System.Threading.Tasks;

namespace Sensors.Atmospheric.BMP280_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Bmp280 sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

            //CreateSpiSensor();
            CreateI2CSensor();

            var consumer = Bmp280.CreateObserver(
                handler: result =>
                {
                    Resolver.Log.Info($"Observer: Temp changed by threshold; new temp: {result.New.Temperature?.Celsius:N1}C, old: {result.Old?.Temperature?.Celsius:N1}C");
                },
                filter: result =>
                {
                    if (result.Old?.Temperature is { } oldTemp &&
                        result.New.Temperature is { } newTemp)
                    {
                        return (newTemp - oldTemp).Abs().Celsius > 0.5;
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

            sensor.StartUpdating(TimeSpan.FromSeconds(1));
        }

        void CreateSpiSensor()
        {
            Resolver.Log.Info("Create BME280 sensor with SPI...");

            var spi = Device.CreateSpiBus();
            sensor = new Bmp280(spi, Device.Pins.D00.CreateDigitalOutputPort());
        }

        void CreateI2CSensor()
        {
            Resolver.Log.Info("Create BME280 sensor with I2C...");

            var i2c = Device.CreateI2cBus();
            sensor = new Bmp280(i2c, (byte)Bmx280.Addresses.Default); // SDA pulled up

        }

        //<!=SNOP=>
    }
}