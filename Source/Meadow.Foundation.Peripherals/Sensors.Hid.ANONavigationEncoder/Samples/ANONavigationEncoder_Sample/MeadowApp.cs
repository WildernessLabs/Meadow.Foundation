using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Hid;
using System.Threading.Tasks;

namespace ANONavigationEncoder_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        ANONavigationEncoder encoder;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            encoder = new ANONavigationEncoder(Device.Pins.A01, Device.Pins.A02, Device.Pins.A03, Device.Pins.A04,
                                                Device.Pins.A05, true, Device.Pins.D02, Device.Pins.D03, true);

            encoder.ButtonCenter.Clicked += ButtonCenter_Clicked;
            encoder.DirectionalPad.Updated += DirectionalPad_Updated;
            encoder.RotaryEncoder.Rotated += RotaryEncoder_Rotated;

            return Task.CompletedTask;
        }

        private void RotaryEncoder_Rotated(object sender, Meadow.Peripherals.Sensors.Rotary.RotaryChangeResult e)
        {
            Resolver.Log.Info($"Rotary dial moved: {e.New}");
        }

        private void DirectionalPad_Updated(object sender, ChangeResult<Meadow.Peripherals.Sensors.Hid.DigitalJoystickPosition> e)
        {
            Resolver.Log.Info($"DPad state updated: {e.New}");
        }

        private void ButtonCenter_Clicked(object sender, System.EventArgs e)
        {
            Resolver.Log.Info("Center button clicked");
        }
    }
}