using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Rotary;
using Meadow.Peripherals.Sensors.Rotary;
using System;
using System.Threading.Tasks;

namespace Sensors.Rotary.RotaryEncoderWithButton_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        protected int value = 0;
        protected RotaryEncoderWithButton rotaryEncoder;

        public override Task Initialize()
        {
            Console.WriteLine("Initializing Hardware...");

            // Note: on the rotary encoder in the hack kit, the pinout is as
            // follows:
            //
            // | Encoder Name | Driver Pin Name |
            // |--------------|-----------------|
            // | `SW`         | `buttonPin`     |
            // | `DT`         | `aPhasePin`     |
            // | `CLK`        | `bPhasePin`     |

            // initialize the encoder
            rotaryEncoder = new RotaryEncoderWithButton(Device, Device.Pins.D07, Device.Pins.D08, Device.Pins.D06);

            //==== Classic Events
            rotaryEncoder.Rotated += RotaryEncoder_Rotated;

            rotaryEncoder.Clicked += (s, e) => Console.WriteLine("Button Clicked");
          
            rotaryEncoder.PressEnded += (s, e) => Console.WriteLine("Press ended");
       
            rotaryEncoder.PressStarted += (s, e) => Console.WriteLine("Press started");
     
            Console.WriteLine("Hardware initialization complete.");

            return Task.CompletedTask;
        }

        private void RotaryEncoder_Rotated(object sender, RotaryChangeResult e)
        {
            switch (e.New) 
            {
                case RotationDirection.Clockwise:
                    value++;
                    Console.WriteLine("/\\ Value = {0} CW", value);
                    break;
                case RotationDirection.CounterClockwise:
                    value--;
                    Console.WriteLine("\\/ Value = {0} CCW", value);
                    break;
            }
        }

        //<!=SNOP=>
    }
}