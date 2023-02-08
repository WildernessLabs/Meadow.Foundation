using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public sealed class Ft232DigitalOutputPort : DigitalOutputPortBase
    {
        private IFt232Bus _bus;
        private bool _state;

        internal Ft232DigitalOutputPort(IPin pin, IDigitalChannelInfo info, bool initialState, OutputType initialOutputType, IFt232Bus bus)
            : base(pin, info, initialState, initialOutputType)
        {
            if (initialOutputType != OutputType.PushPull)
            {
                throw new NotSupportedException("The FT232 only supports push-pull outputs");
            }

            _bus = bus;
            State = initialState;
        }

        public override bool State
        {
            get => _state;
            set
            {
                byte s = _bus.GpioState;

                if (value)
                {
                    s |= (byte)((byte)Pin.Key);
                }
                else
                {
                    s &= (byte)~((byte)Pin.Key);
                }

                var result = Native.Functions.FT_WriteGPIO(_bus.Handle, _bus.GpioDirectionMask, s);
                Native.CheckStatus(result);

                _bus.GpioState = s;
                _state = value;
            }
        }

    }
}