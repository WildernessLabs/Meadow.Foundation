using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Sensors.HallEffect;
using System;

namespace Sensors.HallEffect_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        LinearHallEffectTachometer hallSensor;

        public MeadowApp()
        {
            Console.Write("Initializing...");

            hallSensor = new LinearHallEffectTachometer(
                inputPort: Device.CreateDigitalInputPort(Device.Pins.D02, Meadow.Hardware.InterruptMode.EdgeRising, Meadow.Hardware.ResistorMode.InternalPullUp, 0, 10),
                type: CircuitTerminationType.CommonGround,
                numberOfMagnets: 2,
                rpmChangeNotificationThreshold: 1);
            hallSensor.RPMsChanged += HallSensorRPMsChanged;

            Console.WriteLine("done");
        }

        void HallSensorRPMsChanged(object sender, FloatChangeResult e)
        {
            Console.WriteLine($"RPM: {e.New}");
        }
    }
}