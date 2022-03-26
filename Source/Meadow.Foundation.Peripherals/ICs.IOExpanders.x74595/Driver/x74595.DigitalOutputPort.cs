using System;
using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class x74595
    {
        public class DigitalOutputPort : DigitalOutputPortBase
        {
            readonly x74595 _x74595;

            public override bool State
            {
                get => state;
                set
                {
                    _x74595.WriteToPin(Pin, value);
                }
            }
            protected bool state;

            public DigitalOutputPort(
                x74595 x74595,
                IPin pin,
                bool initialState, 
                OutputType outputType)
                : base(pin, (IDigitalChannelInfo)pin.SupportedChannels[0], initialState, outputType)
            {

                _x74595 = x74595;
            }

            public override void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
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

            // Finalizer
            ~DigitalOutputPort()
            {
                Dispose(false);
            }
        }
    }
}