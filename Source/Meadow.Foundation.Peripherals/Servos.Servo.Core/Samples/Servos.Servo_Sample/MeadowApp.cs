using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Servos;

namespace Servos.Servo_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {        
        protected Servo servo;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            //servo = new Servo(Device, Device.Pins.D04, NamedServoConfigs.SG90);
            servo = new Servo(Device.CreatePwmPort(Device.Pins.D04), NamedServoConfigs.SG90);

            servo.RotateTo(0);

            TestServo();
        }

        void TestServo()
        {
            Console.WriteLine("TestServo...");

            while (true)
            {
                for (int i = 0; i <= servo.Config.MaximumAngle; i++)
                {
                    servo.RotateTo(i);
                    Console.WriteLine($"Rotating to {i}");
                    Thread.Sleep(40);
                }
                Thread.Sleep(2000);
                for (int i = 180; i >= servo.Config.MinimumAngle; i--)
                {
                    servo.RotateTo(i);
                    Console.WriteLine($"Rotating to {i}");
                    Thread.Sleep(40);
                }
                Thread.Sleep(2000);

                //if (servo.Angle <= servo.Config.MinimumAngle)
                //{
                //    Console.WriteLine($"Rotating to {servo.Config.MaximumAngle}");
                //    servo.RotateTo(servo.Config.MaximumAngle);
                //}
                //else
                //{
                //    Console.WriteLine($"Rotating to {servo.Config.MinimumAngle}");
                //    servo.RotateTo(servo.Config.MinimumAngle);
                //}
                //Thread.Sleep(4000);
            }
        }
    }
}