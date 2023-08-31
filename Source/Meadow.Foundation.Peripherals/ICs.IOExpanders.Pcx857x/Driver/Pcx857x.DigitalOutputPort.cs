using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Pcx857x
    {
        /// <summary>
        /// A Pcx857x-specific implementation of the IOutputPort
        /// </summary>
        public class DigitalOutputPort : DigitalOutputPortBase
        {
            internal event EventHandler Disposed = delegate { };

            /// <summary>
            /// The port's containing Pcx857x
            /// </summary>
            public Pcx857x Peripheral { get; }

            /// <summary>
            /// Creates a DigitalOutputPort instance
            /// </summary>
            public DigitalOutputPort(Pcx857x peripheral, IPin pin, bool initialState, OutputType initialOutputType = OutputType.PushPull)
                : base(pin, (IDigitalChannelInfo)pin!.SupportedChannels![0], initialState, initialOutputType)
            {
                Peripheral = peripheral;

                State = initialState;
            }

            /// <inheritdoc/>
            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                Disposed?.Invoke(this, EventArgs.Empty);
            }

            /// <inheritdoc/>
            public override bool State
            {

                get => Peripheral.GetPinState(Pin);
                set => Peripheral.SetPinState(Pin, value);
            }
        }
    }
}