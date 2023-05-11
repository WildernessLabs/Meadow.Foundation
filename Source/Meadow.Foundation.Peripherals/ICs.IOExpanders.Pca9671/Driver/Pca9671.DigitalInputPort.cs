using Meadow.Hardware;
using System;
using System.Linq;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Pca9671
    {
        public class DigitalInputPort : DigitalInputPortBase
        {
            private ResistorMode _resistorMode = ResistorMode.Disabled;

            internal event EventHandler Disposed = delegate { };

            public Pca9671 Peripheral { get; }

            public DigitalInputPort(Pca9671 peripheral, IPin pin)
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

            public override bool State => Peripheral.GetState(Pin);
        }
    }
}