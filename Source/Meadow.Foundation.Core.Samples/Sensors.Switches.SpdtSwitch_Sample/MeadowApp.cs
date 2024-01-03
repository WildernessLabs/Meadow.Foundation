using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Switches;
using Meadow.Hardware;
using System.Threading.Tasks;

namespace Sensors.Switches.SpdtSwitch_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        protected SpdtSwitch spdtSwitch;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

            spdtSwitch = new SpdtSwitch(Device.CreateDigitalInterruptPort(Device.Pins.D15, InterruptMode.EdgeBoth, ResistorMode.InternalPullDown));
            spdtSwitch.Changed += (s, e) =>
            {
                Resolver.Log.Info(spdtSwitch.IsOn ? "Switch is on" : "Switch is off");
            };

            Resolver.Log.Info("SpdtSwitch ready...");

            return Task.CompletedTask;
        }

        //<!=SNOP=>
    }
}