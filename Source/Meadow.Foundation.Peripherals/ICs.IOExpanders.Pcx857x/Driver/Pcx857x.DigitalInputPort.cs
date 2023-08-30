using Meadow.Hardware;
using System;
using System.Linq;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Pcx857x
    {
        /// <summary>
        /// A Pca9671-specific implementation of the IInputPort
        /// </summary>
        public class DigitalInputPort : DigitalInputPortBase
        {
            private ResistorMode _resistorMode = ResistorMode.Disabled;

            internal event EventHandler Disposed = delegate { };

            /// <summary>
            /// The port's containing Pca9671
            /// </summary>
            public Pcx857x Peripheral { get; }

            /// <summary>
            /// Creates a DigitalInputPort instance
            /// </summary>
            /// <param name="peripheral">the Pca9671 instance</param>
            /// <param name="pin">The IPIn to use for the port</param>
            public DigitalInputPort(Pcx857x peripheral, IPin pin)
                : base(pin, pin.SupportedChannels.OfType<IDigitalChannelInfo>().First())
            {
                Peripheral = peripheral;
            }

            /// <inheritdoc/>
            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                Disposed?.Invoke(this, EventArgs.Empty);
            }

            /// <inheritdoc/>
            public override ResistorMode Resistor
            {
                get => _resistorMode;
                set
                {
                    switch (value)
                    {
                        case ResistorMode.InternalPullUp:
                        case ResistorMode.InternalPullDown:
                            throw new NotSupportedException($"This device does not support internal resistors");
                    }
                    _resistorMode = value;
                }
            }

            /// <inheritdoc/>
            public override bool State => Peripheral.GetState(Pin);
        }
    }
}