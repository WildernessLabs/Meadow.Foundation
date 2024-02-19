using Meadow.Hardware;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using static Meadow.Foundation.ICs.IOExpanders.Native.Ftd2xx;

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
            byte s = _expander.GpioState;

            if (value)
            {
                s |= _key;
            }
            else
            {
                s &= (byte)~_key;
            }

            _expander.SetGpioDirectionAndState(_lowByte, _expander.GpioDirectionMask, s);
            _expander.GpioState = s;
            _state = value;
        }
    }
}

public class Ft232HExpander : FtdiExpander
{
    internal Ft232HExpander()
    {
    }
}

public abstract partial class FtdiExpander :
    IPinController,
//    IDisposable,
//    IDigitalInputOutputController,
    IDigitalOutputController
//    ISpiController,
//    II2cController
{
    //    public abstract II2cBus CreateI2cBus(int busNumber = 1, I2cBusSpeed busSpeed = I2cBusSpeed.Standard);
    //    public abstract II2cBus CreateI2cBus(IPin[] pins, I2cBusSpeed busSpeed);
    //    public abstract II2cBus CreateI2cBus(IPin clock, IPin data, I2cBusSpeed busSpeed);

    internal byte GpioDirectionMask { get; set; }
    internal byte GpioState { get; set; }

    internal uint Index { get; private set; }
    internal uint Flags { get; private set; }
    internal uint ID { get; private set; }
    internal uint LocID { get; private set; }
    internal string SerialNumber { get; private set; }
    internal string Description { get; private set; }
    internal IntPtr Handle { get; private set; }

    /// <summary>
    /// The pins
    /// </summary>
    public PinDefinitions Pins { get; }

    internal static FtdiExpander Create(
        uint index,
        uint flags,
        FtDeviceType deviceType,
        uint id,
        uint locid,
        string serialNumber,
        string description,
        IntPtr handle)
    {
        var device = deviceType switch
        {
            FtDeviceType.Ft232H => new Ft232HExpander
            {
                Index = index,
                Flags = flags,
                ID = id,
                LocID = locid,
                SerialNumber = serialNumber,
                Description = description,
                Handle = handle
            },
            _ => throw new NotSupportedException(),
        };

        device.Open();
        device.InitializeGpio();

        return device;
    }

    private void Open()
    {
        if (Handle == IntPtr.Zero)
        {
            Native.CheckStatus(
                FT_OpenEx(LocID, Native.FT_OPEN_TYPE.FT_OPEN_BY_LOCATION, out IntPtr handle)
                );
            Handle = handle;
        }
    }

    internal FtdiExpander()
    {
        Pins = new PinDefinitions(this);
    }

    private void InitializeGpio()
    {
        Native.CheckStatus(Native.Ftd2xx.FT_SetBitMode(Handle, 0x00, Native.FT_BITMODE.FT_BITMODE_RESET));
        Native.CheckStatus(Native.Ftd2xx.FT_SetBitMode(Handle, 0x00, Native.FT_BITMODE.FT_BITMODE_MPSSE));

        Thread.Sleep(50);

        ClearInputBuffer();
        InitializeMpsse();
    }

    private bool CheckStatus(FTDI.FT_STATUS status)
    {
        if (status == FTDI.FT_STATUS.FT_OK)
        {
            return true;
        }

        throw new Exception($"Native error: {status}");
    }

    private void ClearInputBuffer()
    {
        var available = GetAvailableBytes();

        if (available > 0)
        {
            var rxBuffer = new byte[available];
            uint bytesRead = 0;
            Native.CheckStatus(
                 FT_Read(Handle, in rxBuffer[0], available, ref bytesRead));
        }
    }

    private uint GetAvailableBytes()
    {
        uint availableBytes = 0;

        Native.CheckStatus(
            FT_GetQueueStatus(Handle, ref availableBytes));

        return availableBytes;
    }

    internal void SetGpioDirectionAndState(bool lowByte, byte direction, byte state)
    {
        Span<byte> outBuffer = stackalloc byte[3];
        outBuffer[0] = (byte)(lowByte ? FTDI.FT_OPCODE.SetDataBitsLowByte : FTDI.FT_OPCODE.SetDataBitsHighByte);
        outBuffer[1] = state; //data
        outBuffer[2] = direction; //direction 1 == output, 0 == input

        // Console.WriteLine($"{(BitConverter.ToString(outBuffer.ToArray()))}");
        Write(outBuffer);
    }

    private void InitializeMpsse()
    {
        Span<byte> writeBuffer = stackalloc byte[1];
        writeBuffer[0] = 0xAA;
        Write(writeBuffer);
        Span<byte> readBuffer = stackalloc byte[2];
        ReadInto(readBuffer);
        if (!((readBuffer[0] == 0xFA) && (readBuffer[1] == 0xAA)))
        {
            throw new IOException($"Failed to setup device in MPSSE mode using magic 0xAA sync");
        }

        // Second with 0xAB
        writeBuffer[0] = 0xAB;
        Write(writeBuffer);
        ReadInto(readBuffer);
        if (!((readBuffer[0] == 0xFA) && (readBuffer[1] == 0xAB)))
        {
            throw new IOException($"Failed to setup device in MPSSE mode using magic 0xAB sync");
        }
    }

    public int ReadInto(Span<byte> buffer)
    {
        var totalRead = 0;
        uint read = 0;

        while (totalRead < buffer.Length)
        {
            var available = GetAvailableBytes();
            if (available > 0)
            {
                Native.CheckStatus(
                    FT_Read(Handle, in buffer[totalRead], available, ref read));

                totalRead += (int)read;
            }
        }

        return totalRead;
    }

    public void Write(ReadOnlySpan<byte> data)
    {
        uint written = 0;

        Native.CheckStatus(
            FT_Write(Handle, in MemoryMarshal.GetReference(data), (ushort)data.Length, ref written));
    }

    public IDigitalOutputPort CreateDigitalOutputPort(IPin pin, bool initialState = false, OutputType initialOutputType = OutputType.PushPull)
    {
        var p = pin as FtdiPin;

        // TODO: make sure the pin isn't in use

        // update the expanders direction mask to make this an output
        GpioDirectionMask |= (byte)pin.Key;

        // update the direction
        SetGpioDirectionAndState(
            p.IsLowByte,
            GpioDirectionMask,
            GpioState);

        var channel = p.SupportedChannels.FirstOrDefault(channel => channel is IDigitalChannelInfo) as IDigitalChannelInfo;
        return new FtdiDigitalOutputPort(this, pin, channel, initialState, initialOutputType);
    }
}
