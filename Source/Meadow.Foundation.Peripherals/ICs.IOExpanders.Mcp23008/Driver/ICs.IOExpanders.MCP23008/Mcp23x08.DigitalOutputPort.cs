using System;
using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Mcp23x08
    {
        public class DigitalOutputPort : DigitalOutputPortBase
        {
            Mcp23x08 _mcp;

            public override bool State {
                get => this.state;
                set {
                    _mcp.WriteToPort(this.Pin, value);
                }
            } protected bool state;

            public DigitalOutputPort(
                Mcp23x08 mcpController,
                IPin pin,
                bool initialState = false)
                : base(pin, (IDigitalChannelInfo)pin.SupportedChannels[0], initialState)
            {
                _mcp = mcpController;
            }

            public override void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                // TODO: we should consider moving this logic to the finalizer
                // but the problem with that is that we don't know when it'll be called
                // but if we do it in here, we may need to check the _disposed field
                // elsewhere

                if (!disposed) {
                    if (disposing) {
                        this.state = false;
                        _mcp.ResetPin(this.Pin);
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
