using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.DigiPots;
using Meadow.Hardware;
using Meadow.Units;
using System.Threading.Tasks;

namespace ICs.DigiPots.Mcp4162_Sample;

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
            new Resistance(5, Resistance.UnitType.Kiloohms)
            );

        return base.Initialize();
    }

    public override async Task Run()
    {
        Resolver.Log.Info("Run");

        for (var i = 0; i <= mcp.MaxResistance.Ohms; i += 100)
        {
            var r = new Resistance(i, Resistance.UnitType.Ohms);
            Resolver.Log.Info($"Setting resistance to {r.Ohms:0} ohms");
            mcp.Rheostats[0].Resistance = r;
            await Task.Delay(1000);
        }

        Resolver.Log.Info("Done");
    }

    //<!=SNOP=>
}