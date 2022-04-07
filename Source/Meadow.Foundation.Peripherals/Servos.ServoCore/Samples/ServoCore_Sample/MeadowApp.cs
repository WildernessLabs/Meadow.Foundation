using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Servos;
using Meadow.Units;
using AU = Meadow.Units.Angle.UnitType;

namespace Servos.Servo_Sample
{
    public class MeadowApp : App<F7FeatherV2, MeadowApp>
    {
        //<!=SNIP=>

        protected Servo servo;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            servo = new Servo(Device.CreatePwmPort(Device.Pins.D02), NamedServoConfigs.SG90);

            servo.RotateTo(new Angle(0, AU.Degrees));

            while (true)
            {
                for (int i = 0; i <= servo.Config.MaximumAngle.Degrees; i++)
                {
                    servo.RotateTo(new Angle(i, AU.Degrees));
                    Console.WriteLine($"Rotating to {i}");
                    Thread.Sleep(40);
                }
                Thread.Sleep(2000);
                for (int i = 180; i >= servo.Config.MinimumAngle.Degrees; i--)
                {
                    servo.RotateTo(new Angle(i, AU.Degrees));
                    Console.WriteLine($"Rotating to {i}");
                    Thread.Sleep(40);
                }
                Thread.Sleep(2000);
            }
        }

        //<!=SNOP=>
    }
}