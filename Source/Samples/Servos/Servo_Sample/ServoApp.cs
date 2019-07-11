using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Servos;
using Meadow.Hardware;

namespace Servo_Sample
{
    public class ServoApp : App<F7Micro, ServoApp>
    {
        readonly IPwmPort pwm;
        readonly Servo servo;

        public ServoApp() 
        {
            pwm = Device.CreatePwmPort(Device.Pins.D05);

            servo = new Servo(pwm, NamedServoConfigs.Ideal180Servo);

            TestServo();
        }

        void TestServo()
        {
            while(true)
            {
                if (servo.Angle <= servo.Config.MinimumAngle)
                {
                    Console.WriteLine($"Rotating to {servo.Config.MaximumAngle}");
                    servo.RotateTo(servo.Config.MaximumAngle);
                }
                else
                {
                    Console.WriteLine($"Rotating to {servo.Config.MinimumAngle}");
                    servo.RotateTo(servo.Config.MinimumAngle);
                }
                Thread.Sleep(4000);
            }
        }
    }
}