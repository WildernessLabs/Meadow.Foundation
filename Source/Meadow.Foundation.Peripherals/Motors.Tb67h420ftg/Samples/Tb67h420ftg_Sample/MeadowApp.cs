using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Motors;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        //RgbPwmLed onboardLed;
        Tb67h420ftg motorDriver;

        PushButton button1;
        PushButton button2;

        public MeadowApp()
        {
            Initialize();
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            // this causes unterrupts to fail, for some reason:
            //IDigitalInputPort test = Device.CreateDigitalInputPort(Device.Pins.D07);
            // this does not.
            IDigitalOutputPort test = Device.CreateDigitalOutputPort(Device.Pins.D07);

            Console.WriteLine("Made it here.");

            button1 = new PushButton(Device, Device.Pins.D12, Meadow.Hardware.ResistorMode.InternalPullDown);
            button2 =  new PushButton(Device, Device.Pins.D13, Meadow.Hardware.ResistorMode.InternalPullDown);

            button1.PressStarted += Button1_PressStarted;
            button1.PressEnded += Button1_PressEnded;
            button2.PressStarted += Button2_PressStarted;
            button2.PressEnded += Button2_PressEnded;

            motorDriver = new Tb67h420ftg(Device,
                inA1: Device.Pins.D04, inA2: Device.Pins.D03, pwmA: Device.Pins.D01,
                inB1: Device.Pins.D05, inB2: Device.Pins.D06, pwmB: Device.Pins.D00,
                fault1: Device.Pins.D02, fault2: Device.Pins.D07,
                hbMode: Device.Pins.D09, tblkab: Device.Pins.D10);

            // 6V motors with a 12V input. this clamps them to 6V
            motorDriver.Motor1.MotorCalibrationMultiplier = 0.5f;
            motorDriver.Motor2.MotorCalibrationMultiplier = 0.5f;

            Console.WriteLine("Initialization complete.");
        }


        private void Button1_PressStarted(object sender, EventArgs e)
        {
            Console.WriteLine("Motor 1 start.");
            motorDriver.Motor1.Speed = 1f;
        }
        private void Button1_PressEnded(object sender, EventArgs e)
        {
            Console.WriteLine("Motor 1 stop.");
            motorDriver.Motor1.Speed = 0f;
        }

        private void Button2_PressStarted(object sender, EventArgs e)
        {
            Console.WriteLine("Motor 2 start.");
            motorDriver.Motor2.Speed = 0.5f;
        }
        private void Button2_PressEnded(object sender, EventArgs e)
        {
            Console.WriteLine("Motor 2 stop.");
            motorDriver.Motor2.Speed = 0f;
        }

    }
}