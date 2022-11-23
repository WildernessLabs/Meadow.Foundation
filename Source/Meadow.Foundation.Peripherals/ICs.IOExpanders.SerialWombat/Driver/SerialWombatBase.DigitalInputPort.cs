using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public abstract partial class SerialWombatBase
    {
        /// <summary>
        /// Serial Wombat DigitalInputPort
        /// </summary>
        public class DigitalInputPort : DigitalInputPortBase
        {
            private SerialWombatBase _controller;
            private ResistorMode _resistor;

            /// <summary>
            /// Debounce filter duration
            /// </summary>
            public override TimeSpan DebounceDuration { get; set; }

            /// <summary>
            /// Glitch filter duration
            /// </summary>
            public override TimeSpan GlitchDuration { get; set; }

            /// <summary>
            /// Create a new DigitalInputPort object
            /// </summary>
            public DigitalInputPort(SerialWombatBase controller, IPin pin, InterruptMode interruptMode, ResistorMode resistorMode)
                : base(pin, GetChannelInfoForPin(pin), interruptMode)
            {
                if (interruptMode != InterruptMode.None) throw new NotSupportedException("Interrupts not supported");

                Resistor = resistorMode;
                _controller = controller;
                _controller.ConfigureInputPin((byte)pin.Key, Resistor);
            }

            /// <summary>
            /// The port state
            /// </summary>
            public override bool State
            {
                get => _controller.ReadPublicData((byte)Pin.Key) != 0;
            }

            /// <summary>
            /// The port resistor mode
            /// </summary>
            public override ResistorMode Resistor
            {
                get => _resistor;
                set
                {
                    if (value == Resistor) return;
                    _resistor = value;
                    _controller.ConfigureInputPin((byte)Pin.Key, Resistor);
                }
            }
        }
    }
}