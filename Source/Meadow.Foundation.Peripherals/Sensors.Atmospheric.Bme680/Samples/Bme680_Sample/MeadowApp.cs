using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using System;
using System.Threading.Tasks;

namespace Sensors.Atmospheric.BME680_Sample
{
    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        //<!—SNIP—>

        Bme680? sensor;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            //CreateSpiSensor();
            CreateI2CSensor();

            Console.WriteLine("A");

            var consumer = Bme680.CreateObserver(
                handler: result =>
                {
                    Console.WriteLine($"Observer: Temp changed by threshold; new temp: {result.New.Temperature?.Celsius:N2}C, old: {result.Old?.Temperature?.Celsius:N2}C");
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

            Console.WriteLine("B");

            if(sensor != null)
            {
                sensor.Updated += (sender, result) => {
                    Console.WriteLine($"  Temperature: {result.New.Temperature?.Celsius:N2}C");
                    Console.WriteLine($"  Relative Humidity: {result.New.Humidity:N2}%");
                    Console.WriteLine($"  Pressure: {result.New.Pressure?.Millibar:N2}mbar ({result.New.Pressure?.Pascal:N2}Pa)");
                };
            }

            Console.WriteLine("D");

            sensor?.StartUpdating(TimeSpan.FromSeconds(1));

            Console.WriteLine("C");

            ReadConditions().Wait();

            Console.WriteLine("E");
        }

        void CreateSpiSensor()
        {
            Console.WriteLine("Create BME680 sensor with SPI...");

          //  var spiBus = Device.CreateSpiBus();
          //  sensor = new Bme680(spiBus, Device.CreateDigitalOutputPort(Device.Pins.D14));
        }

        void CreateI2CSensor()
        {
            Console.WriteLine("Create BME680 sensor with I2C...");

            var i2c = Device.CreateI2cBus();
            sensor = new Bme680(i2c, (byte)Bme680.Addresses.Address_0x76); // SDA pulled down

        }

        async Task ReadConditions()
        {
            if(sensor == null) { return; }

            var (Temperature, Humidity, Pressure) = await sensor.Read();
            Console.WriteLine("Initial Readings:");
            Console.WriteLine($"  Temperature: {Temperature?.Celsius:N2}C");
            Console.WriteLine($"  Pressure: {Pressure?.Hectopascal:N2}hPa");
            Console.WriteLine($"  Relative Humidity: {Humidity?.Percent:N2}%");
        }

        //<!—SNOP—>
    }
}