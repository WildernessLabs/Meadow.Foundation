using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Rotary;
using Meadow.Peripherals.Sensors.Rotary;

namespace Sensors.Rotary.RotaryEncoderWithButton_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        protected int value = 0;
        protected RotaryEncoderWithButton rotaryEncoder;

        public MeadowApp()
        {
            Initialize();
        }

        void Initialize()
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

            //==== IObservable
            var observer = RotaryEncoder.CreateObserver(
                handler: result => { Console.WriteLine("Observer triggered, rotation has switched!"); },
                // only notify if the rotation has switched (a little contrived, but a fun use of filtering)
                filter: result => result.Old != null && result.New != result.Old.Value
                // for all events, pass null or return true for filter:
                //filter: null
            );
            rotaryEncoder.Subscribe(observer);

            //==== Button events
            rotaryEncoder.Clicked += (s, e) => {
                Console.WriteLine("Button Clicked");
            };
            rotaryEncoder.PressEnded += (s, e) => {
                Console.WriteLine("Press ended");
            };
            rotaryEncoder.PressStarted += (s, e) => {
                Console.WriteLine("Press started");
            };


            Console.WriteLine("Hardware initialization complete.");
        }

        private void RotaryEncoder_Rotated(object sender, RotaryChangeResult e)
        {
            switch (e.New) {
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
    }
}