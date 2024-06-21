using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Servos;
using Meadow.Peripherals.Servos;
using Meadow.Units;
using System.Threading.Tasks;
using AU = Meadow.Units.Angle.UnitType;

namespace Servos.Servo_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        protected IAngularServo servo;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            servo = new Sg90(Device.Pins.D02);

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            while (true)
            {
                for (int i = 0; i <= servo.MaximumAngle.Degrees; i++)
                {
                    servo.RotateTo(new Angle(i, AU.Degrees));
                    Resolver.Log.Info($"Rotating to {i}");
                }

                await Task.Delay(2000);

                for (int i = 180; i >= servo.MinimumAngle.Degrees; i--)
                {
                    servo.RotateTo(new Angle(i, AU.Degrees));
                    Resolver.Log.Info($"Rotating to {i}");
                }
                await Task.Delay(2000);
            }
        }

        //<!=SNOP=>
    }
}