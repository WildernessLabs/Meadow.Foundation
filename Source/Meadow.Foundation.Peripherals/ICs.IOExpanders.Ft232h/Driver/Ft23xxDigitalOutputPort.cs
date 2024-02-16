using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders;

/// <summary>
/// Represents a digital output port implementation for the FT23xx device.
/// </summary>
public sealed class Ft23xxDigitalOutputPort : DigitalOutputPortBase
{
    private FtdiDevice _device;
    private bool _state;
    private bool _isHighByte;
    private byte _key;

    internal Ft23xxDigitalOutputPort(FtdiDevice device, IPin pin, IDigitalChannelInfo channel, bool initialState, OutputType initialOutputType)
        : base(pin, channel, initialState, initialOutputType)
    {
        _device = device;

        // TODO: make sure the pin isn't already in use
        var key = Convert.ToUInt16(Pin.Key);
        if (key > 255)
        {
            _isHighByte = true;
            _key = (byte)(key >> 8);
        }
        else
        {
            _isHighByte = false;
            _key = (byte)(key & 0xff);
        }

        _device.GpioDirectionMask |= _key;
    }

    /// <inheritdoc/>
    public override bool State
    {
        get => _state;
        set
        {
            byte s = _device.GpioState;

            if (value)
            {
                s |= _key;
            }
            else
            {
                s &= (byte)~_key;
            }

            _device.SetGpioState(!_isHighByte, _device.GpioDirectionMask, s);
            _device.GpioState = s;
            _state = value;
        }
    }
}
