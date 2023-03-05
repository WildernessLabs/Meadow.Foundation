using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Rotary;
using Meadow.Peripherals.Sensors.Rotary;
using System;
using System.Threading.Tasks;

namespace Sensors.Rotary.RotaryEncoder_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        protected int value = 0;
        protected RotaryEncoder rotaryEncoder;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing Hardware...");

            // Note: on the rotary encoder in the hack kit, the pinout is as
            // follows:
            //
            // | Encoder Name | Driver Pin Name |
            // |--------------|-----------------|
            // | `SW`         | `buttonPin`     |
            // | `DT`         | `aPhasePin`     |
            // | `CLK`        | `bPhasePin`     |

            // initialize the encoder
            rotaryEncoder = new RotaryEncoder(Device.Pins.D07, Device.Pins.D08);

            //==== Classic Events
            rotaryEncoder.Rotated += RotaryEncoder_Rotated;

            Resolver.Log.Info("Hardware initialization complete.");

            return Task.CompletedTask;
        }

        void RotaryEncoder_Rotated(object sender, RotaryChangeResult e)
        {
            switch (e.New)
            {
                case RotationDirection.Clockwise:
                    value++;
                    Resolver.Log.Info($"/\\ Value = {value} CW");
                    break;
                case RotationDirection.CounterClockwise:
                    value--;
                    Resolver.Log.Info($"\\/ Value = {value} CCW");
                    break;
            }
        }

        //<!=SNOP=>
    }
}