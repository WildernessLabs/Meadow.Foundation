using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.DigiPots;
using Meadow.Hardware;
using System.Threading.Tasks;

namespace ICs.DigiPots.Ds3502_Sample
{
    public class MeadowApp : App<F7FeatherV1>
    {
        //<!=SNIP=>

        protected Mcp4162 mcp;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            mcp = new Mcp4162(
                Device.CreateSpiBus(),
                Device.Pins.D15.CreateDigitalOutputPort(),
                new Meadow.Units.Resistance(5000, Meadow.Units.Resistance.UnitType.Ohms)
                );

            return base.Initialize();
        }

        public override async Task Run()
        {
            for (var r = 0; r <= 5000; r += 100)
            {
                Resolver.Log.Info($"Writing {r} ohms:0");
                mcp.Rheostats[0].Resistance = new Meadow.Units.Resistance(r);
                await Task.Delay(1000);

                Resolver.Log.Info($"Read {mcp.Rheostats[0].Resistance.Ohms} ohms:0");
                await Task.Delay(1000);
            }
        }

        //<!=SNOP=>
    }
}