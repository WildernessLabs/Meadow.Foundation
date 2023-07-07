using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Sensors.HallEffect;
using System;
using System.Threading.Tasks;

namespace Sensors.HallEffect_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        private LinearHallEffectTachometer hallSensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

            hallSensor = new LinearHallEffectTachometer(
                inputPort: Device.CreateDigitalInterruptPort(Device.Pins.D02, Meadow.Hardware.InterruptMode.EdgeRising, Meadow.Hardware.ResistorMode.InternalPullUp, TimeSpan.Zero, TimeSpan.FromMilliseconds(1)),
                type: CircuitTerminationType.CommonGround,
                numberOfMagnets: 2,
                rpmChangeNotificationThreshold: 1);
            hallSensor.RPMsChanged += HallSensorRPMsChanged;

            Resolver.Log.Info("done");

            return Task.CompletedTask;
        }

        private void HallSensorRPMsChanged(object sender, ChangeResult<float> e)
        {
            Resolver.Log.Info($"RPM: {e.New}");
        }

        //<!=SNOP=>
    }
}