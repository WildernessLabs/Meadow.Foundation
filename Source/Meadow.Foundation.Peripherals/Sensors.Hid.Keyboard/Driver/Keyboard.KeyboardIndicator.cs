using Meadow.Hardware;
using System;
using static Meadow.Foundation.Sensors.Hid.Keyboard.Interop;

namespace Meadow.Foundation.Sensors.Hid;

public partial class Keyboard
{
    /// <summary>
    /// Represents a Keyboard indicator as a Meadow digital output
    /// </summary>
    public class KeyboardIndicator : DigitalOutputPortBase
    {
        private readonly KeyboardIndicatorPin _pin;

        /// <summary>
        /// Sets the state of the indicator
        /// </summary>
        public override bool State
        {
            get => GetState();
            set => SetState(value);
        }

        internal KeyboardIndicator(IPin pin, IDigitalChannelInfo info, bool? initialState)
            : base(pin, info, false, OutputType.PushPull)
        {
            if (pin is KeyboardIndicatorPin indicator)
            {
                _pin = indicator;

                if (initialState.HasValue)
                {
                    SetState(initialState.Value);
                }
            }
            else
            {
                throw new ArgumentException("pin must be a KeyboardIndicatorPin");
            }
        }

        private void SetState(bool state)
        {
            (_pin.Controller as Keyboard)?.SetIndicatorState((Indicators)(Convert.ToInt16(_pin.Key)), state);
        }

        private bool GetState()
        {
            return (_pin.Controller as Keyboard)?.GetIndicatorState((Indicators)(Convert.ToInt16(_pin.Key))) ?? false;
        }
    }
}

