using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Hid;
using Meadow.Hardware;
using System.Threading.Tasks;

namespace Xpt2046_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        private Xpt2046 touchScreen;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            var i2cBus = Device.CreateI2cBus(I2cBusSpeed.Fast);

            touchScreen = new Xpt2046(
                Device.CreateSpiBus(),
                Device.Pins.D04.CreateDigitalInterruptPort(InterruptMode.EdgeFalling, ResistorMode.InternalPullUp),
                Device.Pins.D05.CreateDigitalOutputPort(true));

            touchScreen.TouchDown += TouchScreen_TouchDown;

            return Task.CompletedTask;
        }

        private void TouchScreen_TouchDown(ITouchScreen sender, TouchPoint point)
        {
            Resolver.Log.Info($"Touch at location: X:{point.ScreenX}, Y:{point.ScreenY}");
        }

        //<!=SNOP=>
    }
}