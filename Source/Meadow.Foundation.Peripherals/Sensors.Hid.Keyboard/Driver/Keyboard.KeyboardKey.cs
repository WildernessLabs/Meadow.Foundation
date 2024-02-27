using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Sensors.Hid;

public partial class Keyboard
{
    /// <summary>
    /// Represents a Keyboard key as a Meadow digital input
    /// </summary>
    public class KeyboardKey : DigitalInterruptPortBase
    {
        private bool _state;

        /// <summary>
        /// the state of the key, <b>true</b> being "down"
        /// </summary>
        public override bool State => _state;
        /// <summary>
        /// Virtual resistor mode of the key.  Always InternalPullUp
        /// </summary>
        public override ResistorMode Resistor { get => ResistorMode.InternalPullUp; set { } }
        /// <summary>
        /// Debounce duration of the key. Unsupported because the hardware handles this.
        /// </summary>
        public override TimeSpan DebounceDuration { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        /// <summary>
        /// Glitch filter duration of the key. Unsupported because the hardware handles this.
        /// </summary>
        public override TimeSpan GlitchDuration { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        internal KeyboardKey(KeyboardKeyPin pin, IDigitalChannelInfo info, InterruptMode interruptMode)
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
                            RaiseChangedAndNotify(new DigitalPortResult(new DigitalState(State, Environment.TickCount), null));
                        }
                        break;
                    case InterruptMode.EdgeFalling:
                        if (!State)
                        {
                            RaiseChangedAndNotify(new DigitalPortResult(new DigitalState(State, Environment.TickCount), null));
                        }
                        break;
                    case InterruptMode.EdgeBoth:
                        RaiseChangedAndNotify(new DigitalPortResult(new DigitalState(State, Environment.TickCount), null));
                        break;
                }
            }
        }
    }
}

