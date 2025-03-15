using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using System.Threading.Tasks;

namespace ProgrammableAnalogInput_Sample
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        //<!=SNIP=>
        private ProgrammableAnalogInputModule module;

        public override async Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            var bus = Device.CreateI2cBus();

            module = new ProgrammableAnalogInputModule(
                bus,
                0x10,
                0x20,
                0x21);
        }

        public override async Task Run()
        {
            Resolver.Log.Info("Run...");
            module.ConfigureChannel(0, ProgrammableAnalogInputModule.ChannelType.Voltage_0_10);
            module.ConfigureChannel(1, ProgrammableAnalogInputModule.ChannelType.Voltage_0_10);
            while (true)
            {
                var ch0 = module.Read0_10V(0);
                var ch1 = module.Read0_10V(1);

                Resolver.Log.Info($"CH0: {ch0.Volts:N3}");
                //                Resolver.Log.Info($"CH1: {ch1.Volts:N3}");

                await Task.Delay(1000);
            }
        }

        //<!=SNOP=>
    }
}