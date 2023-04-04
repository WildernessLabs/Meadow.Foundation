using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Environmental;
using Meadow.Hardware;
using System;
using System.Threading.Tasks;

namespace Pma003I_Sample
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV1>
    {
        private Pmsa003I pmsa003I;

        public override Task Run()
        {
            Console.WriteLine("Run...");

            pmsa003I.StartUpdating(TimeSpan.FromSeconds(2));

            return base.Run();
        }

        public override Task Initialize()
        {
            var bus = Device.CreateI2cBus(I2cBusSpeed.Standard);
            pmsa003I = new Pmsa003I(bus);

            pmsa003I.Updated += Pmsa003I_Updated;

            return base.Initialize();
        }

        private void Pmsa003I_Updated(object sender, IChangeResult<(Meadow.Units.Density? PM1_0Std, Meadow.Units.Density? PM2_5Std, Meadow.Units.Density? PM10_0Std, Meadow.Units.Density? PM1_0Env, Meadow.Units.Density? PM2_5Env, Meadow.Units.Density? PM10_0Env, Meadow.Units.Concentration? particles_0_3microns, Meadow.Units.Concentration? particles_0_5microns, Meadow.Units.Concentration? particles_10microns, Meadow.Units.Concentration? particles_25microns, Meadow.Units.Concentration? particles_50microns, Meadow.Units.Concentration? particles_100microns)> e)
        {
            Console.WriteLine($"{e.New}");
        }
    }
}