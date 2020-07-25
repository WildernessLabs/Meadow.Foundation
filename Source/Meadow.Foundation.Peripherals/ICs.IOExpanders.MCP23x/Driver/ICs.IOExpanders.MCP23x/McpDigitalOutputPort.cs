using System;
using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public class McpDigitalOutputPort : DigitalOutputPortBase
    {
        private readonly IMcp23x _mcp;

        private bool _state;

        internal McpDigitalOutputPort(
            IMcp23x mcpController,
            IPin pin,
            int port,
            bool initialState = false,
            OutputType outputType = OutputType.OpenDrain)
            : base(pin, (IDigitalChannelInfo) pin.SupportedChannels[0], initialState, outputType)
        {
            _mcp = mcpController;

            // verify mcp, pin and port are valid
            // allows us to use private methods on _mcp
            if (port < 0 || port >= _mcp.Ports.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(port));
            }

            if (!_mcp.Ports[port].AllPins.Contains(Pin))
            {
                throw new ArgumentException("Pin does not belong to mcp controller.");
            }
        }

        public override bool State
        {
            get => _state;
            set
            {
                _mcp.WritePin(Pin, value);
                _state = value;
            }
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

            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                _state = false;
                _mcp.ResetPin(Pin);
            }

            disposed = true;
        }

        // Finalizer
        ~McpDigitalOutputPort()
        {
            Dispose(false);
        }
    }
}
