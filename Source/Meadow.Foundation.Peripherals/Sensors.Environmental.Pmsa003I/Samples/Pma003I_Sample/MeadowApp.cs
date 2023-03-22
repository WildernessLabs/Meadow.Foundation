using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;

using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Foundation.Sensors.Environmental;
using Meadow.Hardware;

namespace Pma003I_Sample
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        private Pmsa003I _pmsa003I;

        public override async Task Run()
        {
            Console.WriteLine("Run...");

            while (true)
            {
                var (pm10Std, pm25Std, pm100Std, pm10Env, pm25Env, pm100Env, p03Um, p05Um, p10Um, p25Um, p50Um, p100Um) = await _pmsa003I.ReadSensor2()
                    .ConfigureAwait(false);

                Console.WriteLine(
                    $"{pm10Std}, {pm25Std}, {pm100Std}, {pm10Env}, {pm25Env}, {pm100Env}, {p03Um}, {p05Um}, {p10Um}, {p25Um}, {p50Um}, {p100Um}");
            }

            await base.Run();
        }

        public override Task Initialize()
        {
            var bus = Device.CreateI2cBus(I2cBusSpeed.Standard);
            _pmsa003I = new Pmsa003I(bus);

            return base.Initialize();
        }
    }
}