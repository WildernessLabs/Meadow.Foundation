using System;
using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public abstract partial class Mcp23x
    {
        public class DigitalOutputPort : DigitalOutputPortBase
        {
            private readonly Mcp23x _mcp;
            private readonly int _port;
            private readonly byte _pinKey;

            public override bool State
            {
                get => _state;
                set
                {
                    _mcp.WritePin(_port, _pinKey, value);
                    _state = value;
                }
            }

            private bool _state;

            internal DigitalOutputPort(
                Mcp23x mcpController,
                IPin pin,
                int port,
                bool initialState = false,
                OutputType outputType = OutputType.OpenDrain)
                : base(pin, (IDigitalChannelInfo)pin.SupportedChannels[0], initialState, outputType)
            {
                _mcp = mcpController;
                _port = port;
                _pinKey = (byte) pin.Key;

                // verify mcp, pin and port are valid
                // allows us to use private methods on _mcp
                if (_port < 0 || _port >= _mcp.Ports.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(port));
                }

                if (!_mcp.Ports[_port].AllPins.Contains(Pin))
                {
                    throw new ArgumentException("Pin does not belong to mcp controller.");
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
                    _mcp.ResetPin(_port, _pinKey);
                }
                disposed = true;
            }

            // Finalizer
            ~DigitalOutputPort()
            {
                Dispose(false);
            }

        }
    }
}
