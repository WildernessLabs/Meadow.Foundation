using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using System;
using System.Threading.Tasks;

namespace Sensors.AirQuality.Ccs811_Sample
{
    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        //<!—SNIP—>

        Ccs811 sensor;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            var i2c = Device.CreateI2cBus(Meadow.Hardware.I2cBusSpeed.Fast);
            sensor = new Ccs811(i2c);

            var consumer = Ccs811.CreateObserver(
                handler: result => 
                {
                    Console.WriteLine($"Observer triggered:");
                    Console.WriteLine($"   new CO2: {result.New.Co2?.PartsPerMillion:N1}ppm, old: {result.Old?.Co2?.PartsPerMillion:N1}ppm.");
                    Console.WriteLine($"   new VOC: {result.New.Voc?.PartsPerBillion:N1}ppb, old: {result.Old?.Voc?.PartsPerBillion:N1}ppb.");
                },
                filter: result => 
                {
                    //c# 8 pattern match syntax. checks for !null and assigns var.
                    if (result.Old is { } old) 
                    { 
                        return (
                        (result.New.Co2.Value - old.Co2.Value).Abs().PartsPerMillion > 1000 // 1000ppm
                          &&
                        (result.New.Voc.Value - old.Voc.Value).Abs().PartsPerBillion > 100 // 100ppb
                        );
                    }
                    return false;
                }                
            );
            sensor.Subscribe(consumer);

            sensor.Updated += (sender, result) => 
            {
                Console.WriteLine($"CO2: {result.New.Co2.Value.PartsPerMillion:n1}ppm, VOC: {result.New.Voc.Value.PartsPerBillion:n1}ppb");
            };

            ReadConditions().Wait();

            sensor.StartUpdating(TimeSpan.FromSeconds(1));
        }

        async Task ReadConditions()
        {
            var result = await sensor.Read();
            Console.WriteLine("Initial Readings:");
            Console.WriteLine($"  CO2: {result.Co2.Value.PartsPerMillion:n1}ppm");
            Console.WriteLine($"  VOC: {result.Voc.Value.PartsPerBillion:n1}ppb");
        }

        //<!—SNOP—>
    }
}