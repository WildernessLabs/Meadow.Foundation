using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders;

public sealed class FtdiDigitalOutputPort : DigitalOutputPortBase
{
    private FtdiExpander _expander;
    private bool _state;
    private byte _key;
    private bool _lowByte;

    internal FtdiDigitalOutputPort(FtdiExpander expander, IPin pin, IDigitalChannelInfo channel, bool initialState, OutputType initialOutputType)
        : base(pin, channel, initialState, initialOutputType)
    {
        _expander = expander;

        if (pin is FtdiPin p)
        {
            _key = (byte)p.Key;
            _lowByte = p.IsLowByte;
        }
    }

    public override bool State
    {
        get => _state;
        set
        {
            byte s = _expander.GpioStateLow;

            if (value)
            {
                s |= _key;
            }
            else
            {
                s &= (byte)~_key;
            }

            _expander.SetGpioDirectionAndState(_lowByte, _expander.GpioDirectionLow, s);
            _expander.GpioStateLow = s;
            _state = value;
        }
    }
}
