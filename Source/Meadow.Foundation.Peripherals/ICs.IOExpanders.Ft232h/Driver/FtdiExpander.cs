using Meadow.Hardware;
using Meadow.Units;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using static Meadow.Foundation.ICs.IOExpanders.Native.Ftd2xx;

namespace Meadow.Foundation.ICs.IOExpanders;

public abstract partial class FtdiExpander :
    IPinController,
//    IDisposable,
//    IDigitalInputOutputController,
    IDigitalOutputController,
    ISpiController,
    II2cController
{
    internal byte GpioDirectionLow { get; set; }
    internal byte GpioStateLow { get; set; }
    internal byte GpioDirectionHigh { get; set; }
    internal byte GpioStateHigh { get; set; }

    internal uint Index { get; private set; }
    internal uint Flags { get; private set; }
    internal uint ID { get; private set; }
    internal uint LocID { get; private set; }
    internal string SerialNumber { get; private set; }
    internal string Description { get; private set; }
    internal IntPtr Handle { get; private set; }

    public abstract II2cBus CreateI2cBus(int channel = 0, I2cBusSpeed busSpeed = I2cBusSpeed.Standard);
    public abstract ISpiBus CreateSpiBus(int channel, SpiClockConfiguration configuration);

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
        FtdiExpander expander = deviceType switch
        {
            FtDeviceType.Ft232H => new Ft232hExpander
            {
                Index = index,
                Flags = flags,
                ID = id,
                LocID = locid,
                SerialNumber = serialNumber,
                Description = description,
                Handle = handle
            },
            FtDeviceType.Ft2232 => new Ft2232Expander
            {
                Index = index,
                Flags = flags,
                ID = id,
                LocID = locid,
                SerialNumber = serialNumber,
                Description = description,
                Handle = handle
            },
            FtDeviceType.Ft2232H => throw new NotImplementedException(),
            FtDeviceType.Ft4232H => throw new NotImplementedException(),
            _ => throw new NotSupportedException(),
        };

        expander.Open();
        expander.InitializeGpio();

        return expander;
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
        Native.FT_STATUS status;
        status = Native.Ftd2xx.FT_SetUSBParameters(Handle, 65536, 65536);    // Set USB request transfer sizes
        status |= Native.Ftd2xx.FT_SetChars(Handle, 0, 0, 0, 0);              // Disable event and error characters
        status |= Native.Ftd2xx.FT_SetTimeouts(Handle, 5000, 5000);           // Set the read and write timeouts to 5 seconds
        status |= Native.Ftd2xx.FT_SetLatencyTimer(Handle, 16);               // Keep the latency timer at default of 16ms

        status |= Native.Ftd2xx.FT_SetFlowControl(Handle, Native.FT_FLOWCONTROL.FT_FLOW_RTS_CTS, 0, 0);

        status |= Native.Ftd2xx.FT_SetBitMode(Handle, 0x00, Native.FT_BITMODE.FT_BITMODE_RESET); // Reset the mode to whatever is set in EEPROM
        status |= Native.Ftd2xx.FT_SetBitMode(Handle, 0x00, Native.FT_BITMODE.FT_BITMODE_MPSSE); // Enable MPSSE mode

        Native.CheckStatus(status);

        Thread.Sleep(50); // the FTDI C example does this, so we keep it

        ClearInputBuffer();
        InitializeMpsse();
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
        outBuffer[0] = (byte)(lowByte ? Native.FT_OPCODE.SetDataBitsLowByte : Native.FT_OPCODE.SetDataBitsHighByte);
        outBuffer[1] = state; //data
        outBuffer[2] = direction; //direction 1 == output, 0 == input

        // Console.WriteLine($"{(BitConverter.ToString(outBuffer.ToArray()))}");
        Write(outBuffer);

        if (lowByte)
        {
            GpioStateLow = state;
            GpioDirectionLow = direction;
        }
        else
        {
            GpioStateHigh = state;
            GpioDirectionHigh = direction;
        }
    }

    private void InitializeMpsse()
    {
        // Synchronise the MPSSE by sending bad command AA to it
        Span<byte> writeBuffer = stackalloc byte[1];
        writeBuffer[0] = 0xAA;
        Write(writeBuffer);
        Span<byte> readBuffer = stackalloc byte[2];
        ReadInto(readBuffer);
        if (!((readBuffer[0] == 0xFA) && (readBuffer[1] == 0xAA)))
        {
            throw new IOException($"Failed to setup device in MPSSE mode using magic 0xAA sync");
        }

        // Synchronise the MPSSE by sending bad command AB to it
        writeBuffer[0] = 0xAB;
        Write(writeBuffer);
        ReadInto(readBuffer);
        if (!((readBuffer[0] == 0xFA) && (readBuffer[1] == 0xAB)))
        {
            throw new IOException($"Failed to setup device in MPSSE mode using magic 0xAB sync");
        }
    }

    internal int ReadInto(Span<byte> buffer)
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

    internal void Write(ReadOnlySpan<byte> data)
    {
        uint written = 0;

        Native.CheckStatus(
            FT_Write(Handle, in MemoryMarshal.GetReference(data), (ushort)data.Length, ref written));
    }

    /// <inheritdoc/>
    public IDigitalOutputPort CreateDigitalOutputPort(IPin pin, bool initialState = false, OutputType initialOutputType = OutputType.PushPull)
    {
        var p = pin as FtdiPin;

        // TODO: make sure the pin isn't in use

        if (p.IsLowByte)
        {
            // update the expanders direction mask to make this an output
            GpioDirectionLow |= (byte)pin.Key;

            // update initial state
            if (initialState)
            {
                GpioStateLow |= (byte)pin.Key;
            }

            SetGpioDirectionAndState(
                p.IsLowByte,
                GpioDirectionLow,
                GpioStateLow);
        }
        else
        {
            GpioDirectionHigh |= (byte)pin.Key;

            // update initial state
            if (initialState)
            {
                GpioStateHigh |= (byte)pin.Key;
            }

            SetGpioDirectionAndState(
                p.IsLowByte,
                GpioDirectionHigh,
                GpioStateHigh);
        }

        var channel = p.SupportedChannels.FirstOrDefault(channel => channel is IDigitalChannelInfo) as IDigitalChannelInfo;
        return new DigitalOutputPort(this, pin, channel, initialState, initialOutputType);
    }

    /// <inheritdoc/>
    public II2cBus CreateI2cBus(IPin[] pins, I2cBusSpeed busSpeed)
    {
        return CreateI2cBus(1);
    }

    /// <inheritdoc/>
    public II2cBus CreateI2cBus(IPin clock, IPin data, I2cBusSpeed busSpeed)
    {
        return CreateI2cBus(1);
    }

    /// <inheritdoc/>
    public ISpiBus CreateSpiBus(IPin clock, IPin copi, IPin cipo, SpiClockConfiguration config)
    {
        return CreateSpiBus(0, config);
    }

    /// <inheritdoc/>
    public ISpiBus CreateSpiBus(IPin clock, IPin copi, IPin cipo, Frequency speed)
    {
        return CreateSpiBus(0, new SpiClockConfiguration(speed));
    }

    public ISpiBus CreateSpiBus(SpiClockConfiguration configuration)
    {
        return CreateSpiBus(0, configuration);
    }

    public ISpiBus CreateSpiBus(int channel = 0)
    {
        return CreateSpiBus(channel, new SpiClockConfiguration(1000000.Hertz()));
    }
}
