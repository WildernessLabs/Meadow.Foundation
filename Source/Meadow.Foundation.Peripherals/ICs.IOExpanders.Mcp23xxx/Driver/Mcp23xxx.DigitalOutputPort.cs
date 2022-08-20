using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Mcp23xxx
    {
        public class DigitalOutputPort : DigitalOutputPortBase
        {
            Mcp23xxx mcp;

            /// <summary>
            /// The port state
            /// True for high, false for low
            /// </summary>
            public override bool State
            {
                get => state;
                set => mcp.WriteToPort(Pin, value);
            }
            bool state;

            public DigitalOutputPort(
                Mcp23xxx mcpController,
                IPin pin,
                bool initialState = false,
                OutputType outputType = OutputType.OpenDrain)
                : base(pin, (IDigitalChannelInfo)pin.SupportedChannels[0], initialState, outputType)
            {
                mcp = mcpController;
            }

            protected override void Dispose(bool disposing)
            {
                // TODO: we should consider moving this logic to the finalizer
                // but the problem with that is that we don't know when it'll be called
                // but if we do it in here, we may need to check the disposed field elsewhere
                if (!disposed)
                {
                    if (disposing)
                    {
                        state = false;
                        mcp?.ResetPin(Pin);
                    }
                    disposed = true;
                }
            }

            // Finalizer
            ~DigitalOutputPort()
            {
                Dispose(false);
            }
        }
    }
}