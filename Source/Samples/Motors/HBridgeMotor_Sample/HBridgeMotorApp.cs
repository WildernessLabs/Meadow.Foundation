using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Motors;

namespace HBridgeMotor_Sample
{
    public class HBridgeMotorApp : App<F7Micro, HBridgeMotorApp>
    {
        HBridgeMotor motor1;

        public HBridgeMotorApp()
        {
            ConfigurePorts();
            TestMotor();
        }

        public void ConfigurePorts()
        {
            motor1 = new HBridgeMotor
            (
                a1Pin: Device.CreatePwmPort(Device.Pins.D02),
                a2Pin: Device.CreatePwmPort(Device.Pins.D03),
                enablePin: Device.CreateDigitalOutputPort(Device.Pins.D04)
            );
        }

        public void TestMotor()
        {
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
