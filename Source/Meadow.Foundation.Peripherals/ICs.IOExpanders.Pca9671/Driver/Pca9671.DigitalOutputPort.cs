using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Pca9671
    {
        public class DigitalOutputPort : DigitalOutputPortBase
        {
            internal event EventHandler Disposed = delegate { };

            public Pca9671 Peripheral { get; }

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

            public override bool State
            {

                get => Peripheral.GetState(Pin);
                set => Peripheral.SetState(Pin, value);
            }
        }
    }
}