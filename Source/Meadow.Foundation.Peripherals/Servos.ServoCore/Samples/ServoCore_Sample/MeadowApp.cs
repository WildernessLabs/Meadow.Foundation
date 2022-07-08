using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Servos;
using Meadow.Units;
using AU = Meadow.Units.Angle.UnitType;

namespace Servos.Servo_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        protected Servo servo;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            servo = new Servo(Device, Device.Pins.D02, NamedServoConfigs.SG90);

            return Task.CompletedTask;
        }

        public async override Task Run()
        { 
            await servo.RotateTo(new Angle(0, AU.Degrees));

            while (true)
            {
                for (int i = 0; i <= servo.Config.MaximumAngle.Degrees; i++)
                {
                    await servo.RotateTo(new Angle(i, AU.Degrees));
                    Console.WriteLine($"Rotating to {i}");
                }

                await Task.Delay(2000);

                for (int i = 180; i >= servo.Config.MinimumAngle.Degrees; i--)
                {
                    await servo.RotateTo(new Angle(i, AU.Degrees));
                    Console.WriteLine($"Rotating to {i}");
                }
                await Task.Delay(2000);
            }
        }

        //<!=SNOP=>
    }
}