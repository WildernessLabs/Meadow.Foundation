using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Sensors.Hid;

public partial class Keyboard
{
    public class KeyboardKey : DigitalInputPortBase
    {
        private bool _state;

        public override bool State => _state;
        public override ResistorMode Resistor { get => ResistorMode.InternalPullUp; set => throw new NotSupportedException(); }
        public override TimeSpan DebounceDuration { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override TimeSpan GlitchDuration { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        internal KeyboardKey(KeyboardPin pin, IDigitalChannelInfo info, InterruptMode interruptMode)
            : base(pin, info, interruptMode)
        {
        }

        internal void SetState(bool newState)
        {
            if (State != newState)
            {
                _state = newState;
                switch (InterruptMode)
                {
                    // this is virtually "pulled down" so pressing a key makes it go high
                    case InterruptMode.EdgeRising:
                        if (State)
                        {
                            RaiseChangedAndNotify(new DigitalPortResult(new DigitalState(State, DateTime.UtcNow), null));
                        }
                        break;
                    case InterruptMode.EdgeFalling:
                        if (!State)
                        {
                            RaiseChangedAndNotify(new DigitalPortResult(new DigitalState(State, DateTime.UtcNow), null));
                        }
                        break;
                    case InterruptMode.EdgeBoth:
                        RaiseChangedAndNotify(new DigitalPortResult(new DigitalState(State, DateTime.UtcNow), null));
                        break;
                }
            }
        }
    }
}

