using Meadow.Hardware;
using Meadow.Foundation.Sensors.Buttons;
using System;


namespace Meadow.Foundation.Sensors.Rotary
{
    public class RotaryEncoderWithButton : RotaryEncoder, IRotaryEncoderWithButton
    {
        public event EventHandler PressStarted = delegate { };
        public event EventHandler PressEnded = delegate { };
        public event EventHandler Clicked = delegate { };

        public bool State => _button.State;

        public PushButton Button => _button;
        readonly PushButton _button;

        public RotaryEncoderWithButton(IDigitalPin aPhasePin, IDigitalPin bPhasePin, IDigitalPin buttonPin, 
            CircuitTerminationType buttonCircuitTerminationType, int debounceDuration = 20)
            : base(aPhasePin, bPhasePin)
        {
            _button = new PushButton(buttonPin, buttonCircuitTerminationType, debounceDuration);

            _button.Clicked += Button_Clicked;
            _button.PressStarted += Button_PressStarted;
            _button.PressEnded += Button_PressEnded;
        }

        protected void Button_PressEnded(object sender, EventArgs e)
        {
            this.PressEnded(this, e);
        }

        protected void Button_PressStarted(object sender, EventArgs e)
        {
            this.PressStarted(this, e);
        }

        protected void Button_Clicked(object sender, EventArgs e)
        {
            this.Clicked(this, e);
        }
    }
}
