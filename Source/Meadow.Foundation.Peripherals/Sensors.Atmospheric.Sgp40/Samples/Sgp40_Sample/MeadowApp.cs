using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using System;
using System.Threading.Tasks;

namespace BasicSensors.Atmospheric.SI7021_Sample
{
    public class MeadowApp : App<F7FeatherV1, MeadowApp>
    {
        //<!=SNIP=>

        Sgp40 sensor;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            sensor = new Sgp40(Device.CreateI2cBus());

            Console.WriteLine($"Sensor SN: {sensor.SerialNumber:x6}");

            if (sensor.RunSelfTest())
            {
                Console.WriteLine("Self test successful");
            }
            else
            {
                Console.WriteLine("Self test failed");
            }

            var consumer = Sgp40.CreateObserver(
                handler: result => 
                {
                    Console.WriteLine($"Observer: VOC changed by threshold; new index: {result.New}");
                },
                filter: result =>
                {
                    //c# 8 pattern match syntax. checks for !null and assigns var.
                    return Math.Abs(result.New - result.Old ?? 0) > 10;
                }
            );
            sensor.Subscribe(consumer);

            sensor.Updated += (sender, result) =>
            {
                Console.WriteLine($"  VOC: {result.New}");
            };

            ReadConditions().Wait();

            sensor.StartUpdating(TimeSpan.FromSeconds(1));
        }

        async Task ReadConditions()
        {
            var result = await sensor.Read();
            Console.WriteLine("Initial Readings:");
            Console.WriteLine($"  Temperature: {result}");
        }

        //<!=SNOP=>
    }
}