using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class x74595
    {
        /// <summary>
        /// Represents a digital output port on the x74595
        /// </summary>
        public class DigitalOutputPort : DigitalOutputPortBase
        {
            readonly x74595 x74595;

            /// <summary>
            /// Port state
            /// </summary>
            public override bool State
            {
                get => state;
                set
                {
                    x74595.WriteToPin(Pin, value);
                }
            }
            bool state;

            /// <summary>
            /// Create a new x74595 digital output port
            /// </summary>
            public DigitalOutputPort(
                x74595 x74595,
                IPin pin,
                bool initialState,
                OutputType outputType)
                : base(pin, (IDigitalChannelInfo)pin.SupportedChannels![0], initialState, outputType)
            {
                this.x74595 = x74595;
            }

            ///<inheritdoc/>
            protected override void Dispose(bool disposing)
            {
                if (!disposed)
                {
                    if (disposing)
                    {
                        state = false;
                    }
                    disposed = true;
                }
            }
        }
    }
}