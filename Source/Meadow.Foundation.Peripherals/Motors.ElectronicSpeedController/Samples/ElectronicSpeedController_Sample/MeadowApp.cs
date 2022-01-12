using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Motors;
using Meadow.Foundation.Sensors.Rotary;
using Meadow.Peripherals.Sensors.Rotary;

namespace ElectronicSpeedController_Sample
{
    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        //<!—SNIP—>

        float frequency = 50f;
        const float armMs = 0.5f;
        const float powerIncrement = 0.05f;

        ElectronicSpeedController esc;
        RotaryEncoderWithButton rotary;

        public MeadowApp()
        {
            Initialize();
            DisplayPowerOnLed(esc.Power);
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            //==== rotary encoder
            rotary = new RotaryEncoderWithButton(Device, Device.Pins.D07, Device.Pins.D08, Device.Pins.D06);
            rotary.Rotated += Rotary_Rotated;
            rotary.Clicked += (s, e) => {
                Console.WriteLine($"Arming the device.");
                esc.Arm();
            }; ;

            //==== Electronic Speed Controller
            esc = new ElectronicSpeedController(Device, Device.Pins.D02, frequency);

            Console.WriteLine("Hardware initialized.");
        }

        private void Rotary_Rotated(object sender, Meadow.Peripherals.Sensors.Rotary.RotaryChangeResult e)
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
            int r = (int)Map(power, 0f, 1f, 0f, 255f);
            int b = (int)Map(power, 0f, 1f, 255f, 0f);

            var color = Color.FromRgb(r, 0, b);
        }

        float Map(float value, float fromSource, float toSource, float fromTarget, float toTarget)
        {
            return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }

        //<!—SNOP—>
    }
}