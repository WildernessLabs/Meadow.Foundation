using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Motors;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using System;
using System.Threading.Tasks;

namespace Motors.Tb67h420ftg_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Tb67h420ftg motorDriver;

        PushButton button1;
        PushButton button2;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            button1 = new PushButton(Device.Pins.D12, ResistorMode.InternalPullDown);
            button2 = new PushButton(Device.Pins.D13, ResistorMode.InternalPullDown);

            button1.PressStarted += Button1_PressStarted;
            button1.PressEnded += Button1_PressEnded;
            button2.PressStarted += Button2_PressStarted;
            button2.PressEnded += Button2_PressEnded;

            motorDriver = new Tb67h420ftg(
                inA1: Device.Pins.D04, inA2: Device.Pins.D03, pwmA: Device.Pins.D01,
                inB1: Device.Pins.D05, inB2: Device.Pins.D06, pwmB: Device.Pins.D00,
                fault1: Device.Pins.D02, fault2: Device.Pins.D07,
                hbMode: Device.Pins.D09, tblkab: Device.Pins.D10);

            // 6V motors with a 12V input. this clamps them to 6V
            motorDriver.Motor1.MotorCalibrationMultiplier = 0.5f;
            motorDriver.Motor2.MotorCalibrationMultiplier = 0.5f;

            Resolver.Log.Info("Initialization complete.");

            return base.Initialize();
        }

        private void Button1_PressStarted(object sender, EventArgs e)
        {
            Resolver.Log.Info("Motor 1 start.");
            motorDriver.Motor1.Power = 1f;
        }
        private void Button1_PressEnded(object sender, EventArgs e)
        {
            Resolver.Log.Info("Motor 1 stop.");
            motorDriver.Motor1.Power = 0f;
        }

        private void Button2_PressStarted(object sender, EventArgs e)
        {
            Resolver.Log.Info("Motor 2 start.");
            motorDriver.Motor2.Power = 0.5f;
        }
        private void Button2_PressEnded(object sender, EventArgs e)
        {
            Resolver.Log.Info("Motor 2 stop.");
            motorDriver.Motor2.Power = 0f;
        }

        //<!=SNOP=>
    }
}