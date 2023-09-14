using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders;

public sealed class Ft23xxDigitalOutputPort : DigitalOutputPortBase
{
    private FtdiDevice _device;
    private bool _state;

    internal Ft23xxDigitalOutputPort(FtdiDevice device, IPin pin, IDigitalChannelInfo channel, bool initialState, OutputType initialOutputType)
        : base(pin, channel, initialState, initialOutputType)
    {
        _device = device;
    }

    public override bool State
    {
        get => _state;
        set
        {
            byte s = _device.GpioState;

            if (value)
            {
                s |= (byte)Pin.Key;
            }
            else
            {
                s &= (byte)~(byte)Pin.Key;
            }

            _device.SetGpioState(true, _device.GpioDirectionMask, s);
            _device.GpioState = s;
            _state = value;
        }
    }
}
