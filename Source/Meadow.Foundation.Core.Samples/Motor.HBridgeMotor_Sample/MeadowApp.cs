using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Motors;

namespace Motor.HBridgeMotor_Sample
{
    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        protected HBridgeMotor motor1;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            motor1 = new HBridgeMotor
            (
                a1Port: Device.CreatePwmPort(Device.Pins.D07),
                a2Port: Device.CreatePwmPort(Device.Pins.D08),
                enablePort: Device.CreateDigitalOutputPort(Device.Pins.D09)
            );
            
            TestMotor();
        }

        protected void TestMotor()
        {
            Console.WriteLine("TestMotor...");

            while (true)
            {
                // Motor Forwards
                motor1.Power = 1f;
                Thread.Sleep(1000);

                // Motor Stops
                motor1.Power = 0f;
                Thread.Sleep(500);

                // Motor Backwards
                motor1.Power = -1f;
                Thread.Sleep(1000);
            }
        }
    }
}