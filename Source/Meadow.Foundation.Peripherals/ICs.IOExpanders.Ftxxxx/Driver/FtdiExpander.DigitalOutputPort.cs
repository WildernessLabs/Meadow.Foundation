using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders;

public abstract partial class FtdiExpander
{
    public sealed class DigitalOutputPort : DigitalOutputPortBase
    {
        private FtdiExpander _expander;
        private bool _state;
        private FtdiPin _pin;

        internal DigitalOutputPort(FtdiExpander expander, IPin pin, IDigitalChannelInfo channel, bool initialState, OutputType initialOutputType)
            : base(pin, channel, initialState, initialOutputType)
        {
            _expander = expander;

            if (pin is FtdiPin p)
            {
                _pin = p;
            }
            else
            {
                throw new ArgumentException("Invalid pin");
            }
        }

        public override bool State
        {
            get => _state;
            set
            {
                byte s = _pin.IsLowByte ? _expander.GpioStateLow : _expander.GpioStateHigh;

                if (value)
                {
                    s |= (byte)_pin.Key;
                }
                else
                {
                    s &= (byte)~(byte)_pin.Key;
                }

                if (_pin.IsLowByte)
                {
                    // Dxxx pins
                    _expander.SetGpioDirectionAndState(true, _expander.GpioDirectionLow, s);
                }
                else
                {
                    // Cxxx pins
                    _expander.SetGpioDirectionAndState(false, _expander.GpioDirectionHigh, s);
                }
                _state = value;
            }
        }
    }
}