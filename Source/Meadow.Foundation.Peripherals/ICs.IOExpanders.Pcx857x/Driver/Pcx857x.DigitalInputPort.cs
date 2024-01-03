using Meadow.Hardware;
using System;
using System.Linq;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Pcx857x
    {
        /// <summary>
        /// A Pcx857x-specific implementation of the IDigitalInputPort
        /// </summary>
        public class DigitalInputPort : DigitalInputPortBase
        {
            /// <inheritdoc/>
            public override ResistorMode Resistor
            {
                get => resistorMode;
                set
                {
                    switch (value)
                    {
                        case ResistorMode.InternalPullUp:
                        case ResistorMode.InternalPullDown:
                            throw new NotSupportedException($"This device does not support internal resistors");
                    }
                    resistorMode = value;
                }
            }

            /// <inheritdoc/>
            public override bool State => Peripheral.GetPinState(Pin);

            /// <summary>
            /// The port's containing Pcx857x
            /// </summary>
            public Pcx857x Peripheral { get; }

            private ResistorMode resistorMode = ResistorMode.Disabled;

            internal event EventHandler Disposed = default!;

            /// <summary>
            /// Creates a DigitalInputPort instance
            /// </summary>
            /// <param name="peripheral">the Pcx857x instance</param>
            /// <param name="pin">The IPIn to use for the port</param>
            public DigitalInputPort(Pcx857x peripheral, IPin pin)
                : base(pin, pin.SupportedChannels.OfType<IDigitalChannelInfo>().First())
            {
                Peripheral = peripheral;
            }

            ///<inheritdoc/>
            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                Disposed?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}