using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Pca9671
    {
        /// <summary>
        /// A Pca9671-specific implementation of the IOutputPort
        /// </summary>
        public class DigitalOutputPort : DigitalOutputPortBase
        {
            internal event EventHandler Disposed = default!;

            /// <summary>
            /// The port's containing Pca9671
            /// </summary>
            public Pca9671 Peripheral { get; }

            /// <summary>
            /// Creates a DigitalOutputPort instance
            /// </summary>
            public DigitalOutputPort(Pca9671 peripheral, IPin pin, bool initialState, OutputType initialOutputType = OutputType.PushPull)
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

                get => Peripheral.GetState(Pin);
                set => Peripheral.SetState(Pin, value);
            }
        }
    }
}