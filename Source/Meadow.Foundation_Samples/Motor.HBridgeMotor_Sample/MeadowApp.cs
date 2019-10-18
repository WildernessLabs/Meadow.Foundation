using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Motors;

namespace Motor.HBridgeMotor_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        protected HBridgeMotor motor1;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            motor1 = new HBridgeMotor
            (
                a1Pin: Device.CreatePwmPort(Device.Pins.D07),
                a2Pin: Device.CreatePwmPort(Device.Pins.D08),
                enablePin: Device.CreateDigitalOutputPort(Device.Pins.D09)
            );
            
            TestMotor();
        }

        protected void TestMotor()
        {
            Console.WriteLine("TestMotor...");

            while (true)
            {
                // Motor Forwards
                motor1.Speed = 1f;
                Thread.Sleep(1000);

                // Motor Stops
                motor1.Speed = 0f;
                Thread.Sleep(500);

                // Motor Backwards
                motor1.Speed = -1f;
                Thread.Sleep(1000);
            }
        }
    }
}