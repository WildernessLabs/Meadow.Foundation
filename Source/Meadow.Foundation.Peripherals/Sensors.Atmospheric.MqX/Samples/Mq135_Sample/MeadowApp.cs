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

        Mq135 sensor;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            var input = Device.CreateAnalogInputPort(Device.Pins.A05);

            sensor = new Mq135(input);

            Task.Run(async() =>
            {
                while(true)
                {
                    var sample = await sensor.TakeSample();
                    Console.WriteLine($"Sensor val: {sample.Volts:f3}V");
                }
            });


            /*
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
            */
        }

        async Task ReadConditions()
        {
//            var result = await sensor.Read();
            Console.WriteLine("Initial Readings:");
//            Console.WriteLine($"  Temperature: {result}");
        }

        //<!=SNOP=>
    }
}