using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using System.Threading.Tasks;

namespace ICs.IOExpanders.Tca9535_Sample;

public class MeadowApp : App<F7CoreComputeV2>
{
    //<!=SNIP=>

    private Tca95x5 expander;

    public override Task Initialize()
    {
        Resolver.Log.Info("Initialize...");
        var i2CBus = Device.CreateI2cBus(1);

        expander = new Tca9535(i2CBus);

        return base.Initialize();
    }

    public override async Task Run()
    {
        var port0 = expander.Pins.P00.CreateDigitalOutputPort();
        var port1 = expander.Pins.P01.CreateDigitalOutputPort();
        var port2 = expander.Pins.P02.CreateDigitalOutputPort();
        var port3 = expander.Pins.P03.CreateDigitalOutputPort();

        bool state = false;

        while (true)
        {
            Resolver.Log.Info($"state: {state}");
            port0.State = state;
            port1.State = state;
            port2.State = state;
            port3.State = state;

            state = !state;
            await Task.Delay(2000);
        }
    }

    //<!=SNOP=>
}
