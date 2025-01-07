using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Servos;
using Meadow.Hardware;
using Meadow.Peripherals.Servos;
using Meadow.Units;
using System.Threading.Tasks;

namespace Servos.Servo_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        private IAngularServo servo;
        private IPwmPort pwm;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            pwm = Device.Pins.D10.CreatePwmPort(50.Hertz());

            servo = new Sg90(pwm);

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            var center = 0;
            var step = 10;
            var direction = 1;
            var target = center;

            await Task.Delay(1000);

            while (true)
            {
                var test = target + (step * direction);
                if (test > servo.MaximumAngle.Degrees || test < servo.MinimumAngle.Degrees)
                {
                    direction *= -1;
                    test = target + (step * direction);
                }
                target = test;
                var delay = target * 10;
                if (delay < 10) delay = 10;

                Resolver.Log.Info($"Rotating to {target}");
                servo.RotateTo(new Angle(target, Angle.UnitType.Degrees));
                await Task.Delay(delay);
                Resolver.Log.Info($"Rotating to {-target}");
                servo.RotateTo(new Angle(-target, Angle.UnitType.Degrees));
                await Task.Delay(delay);
            }
        }

        //<!=SNOP=>

        private async Task RangeFinder()
        {
            var min = 200;
            var max = 3000;
            var p = min;
            var step = 10;
            var direction = 1;

            pwm.Start();

            while (true)
            {
                Resolver.Log.Info($"Duration: {p} us");

                pwm.Duration = TimePeriod.FromMicroseconds(p);
                await Task.Delay(200);

                var test = p + (step * direction);
                if (test < min || test > max) direction *= -1;
                p = p + (step * direction);
            }
        }
    }
}