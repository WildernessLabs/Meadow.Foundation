using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Motors;
using Meadow.Foundation.Sensors.Rotary;
using Meadow.Peripherals.Sensors.Rotary;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace ElectronicSpeedController_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Frequency frequency = new Frequency(50, Frequency.UnitType.Hertz);
        const float armMs = 0.5f;
        const float powerIncrement = 0.05f;

        ElectronicSpeedController esc;
        RotaryEncoderWithButton rotary;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            //==== rotary encoder
            rotary = new RotaryEncoderWithButton(Device, Device.Pins.D07, Device.Pins.D08, Device.Pins.D06);
            rotary.Rotated += RotaryRotated;
            rotary.Clicked += (s, e) => {
                Console.WriteLine($"Arming the device.");
                esc.Arm();
            }; ;

            //==== Electronic Speed Controller
            esc = new ElectronicSpeedController(Device, Device.Pins.D02, frequency);

            Console.WriteLine("Hardware initialized.");

            return base.Initialize();
        }

        private void RotaryRotated(object sender, RotaryChangeResult e)
        {
            esc.Power += (e.New == RotationDirection.Clockwise) ? powerIncrement : -powerIncrement;
            DisplayPowerOnLed(esc.Power);

            Console.WriteLine($"New Power: {esc.Power * (float)100:n0}%");
        }

        /// <summary>
        /// Displays the ESC power on the onboard LED as full red @ `100%`,
        /// blue @ `0%`, and a proportional mix, in between those speeds.
        /// </summary>
        /// <param name="power"></param>
        void DisplayPowerOnLed(float power)
        {
            // `0.0` - `1.0`
            int r = (int)ExtensionMethods.Map(power, 0f, 1f, 0f, 255f);
            int b = (int)ExtensionMethods.Map(power, 0f, 1f, 255f, 0f);

            var color = Color.FromRgb(r, 0, b);
        }

        public override Task Run()
        {
            DisplayPowerOnLed(esc.Power);

            return base.Run();
        }

        //<!=SNOP=>
    }
}