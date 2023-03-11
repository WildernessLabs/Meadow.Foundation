using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Hid;
using System.Threading.Tasks;

namespace Bbq10Keyboard_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        BBQ10Keyboard keyboard;

        public override Task Initialize(string[]? args)
        {
            Resolver.Log.Info("Initialize...");

            var i2cBus = Device.CreateI2cBus(0);
            keyboard = new BBQ10Keyboard(i2cBus, Device.Pins.D10);
            keyboard.OnKeyEvent += Keyboard_OnKeyEvent;

            return Task.CompletedTask;
        }

        private void Keyboard_OnKeyEvent(object sender, BBQ10Keyboard.KeyEvent e)
        {
            if (e.KeyState == BBQ10Keyboard.KeyState.StatePress)
            {
                Resolver.Log.Info($"{e.AsciiValue}, {(byte)e.AsciiValue}, pressed");
            }
        }
    }
}
