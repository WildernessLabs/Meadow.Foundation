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
    IDigitalInputOutputController,
    ISpiController,
    II2cController,
    IDisposable
{

    internal byte GpioDirectionLow { get; set; }
    internal byte GpioStateLow { get; set; }
    internal byte GpioDirectionHigh { get; set; }
    internal byte GpioStateHigh { get; set; }

    internal uint Index { get; private set; }
    internal uint Flags { get; private set; }
    internal uint ID { get; private set; }
    internal uint LocID { get; private set; }
    /// <summary>
    /// Gets the serial number of the device.
    /// </summary>
    public string? SerialNumber { get; private set; }

    /// <summary>
    /// Gets the description of the device.
    /// </summary>
    public string? Description { get; private set; }
    internal IntPtr Handle { get; private set; }

    /// <inheritdoc/>
    public abstract II2cBus CreateI2cBus(int channel = 0, I2cBusSpeed busSpeed = I2cBusSpeed.Standard);
    /// <inheritdoc/>
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
            FtDeviceType.Ft232H => new Ft232h
            {
                Index = index,
                Flags = flags,
                ID = id,
                LocID = locid,
                SerialNumber = serialNumber,
                Description = description,
                Handle = handle
            },
            // Mac driver sometimes mis-identifies FT232H as FT232B/FT245B (Type 0)
            FtDeviceType.Ft232BOrFt245B => new Ft232h
            {
                Index = index,
                Flags = flags,
                ID = id,
                LocID = locid,
                SerialNumber = serialNumber,
                Description = description,
                Handle = handle
            },
            FtDeviceType.UnknownDevice => new Ft232h
            {
                Index = index,
                Flags = flags,
                ID = id,
                LocID = locid,
                SerialNumber = serialNumber,
                Description = description,
                Handle = handle
            },
            FtDeviceType.Ft2232 => new Ft2232
            {
                Index = index,
                Flags = flags,
                ID = id,
                LocID = locid,
                SerialNumber = serialNumber,
                Description = description,
                Handle = handle
            },
            FtDeviceType.Ft2232H => new Ft2232H
        {
            Index = index,
            Flags = flags,
            ID = id,
            LocID = locid,
            SerialNumber = serialNumber,
            Description = description,
            Handle = handle
        },
            FtDeviceType.Ft4232H => throw new NotImplementedException(),
            _ => throw new NotSupportedException(),
        };

        expander.Open();
        expander.InitializeGpio();

        return expander;
    }

    private void Open()
    {
        if (Handle != IntPtr.Zero) return;

        Native.FT_STATUS status;
        IntPtr handle;

        // 1. Try to open by Location ID
        // On macOS, LocID might be 0 but FT_OpenEx(0, BY_LOCATION) works!
        status = FT_OpenEx(LocID, Native.FT_OPEN_TYPE.FT_OPEN_BY_LOCATION, out handle);
        if (status == Native.FT_STATUS.FT_OK)
        {
            Handle = handle;
            return;
        }

        // 2. Try to open by Serial Number (if available)
        if (!string.IsNullOrEmpty(SerialNumber))
        {
            status = FT_OpenEx(SerialNumber, Native.FT_OPEN_TYPE.FT_OPEN_BY_SERIAL_NUMBER, out handle);
            if (status == Native.FT_STATUS.FT_OK)
            {
                Handle = handle;
                return;
            }
        }

        // 3. Try to open by Description (if available)
        if (!string.IsNullOrEmpty(Description))
        {
            status = FT_OpenEx(Description, Native.FT_OPEN_TYPE.FT_OPEN_BY_DESCRIPTION, out handle);
            if (status == Native.FT_STATUS.FT_OK)
            {
                Handle = handle;
                return;
            }
        }

        // 4. Fallback: Open by Index
        status = FT_Open(Index, out handle);
        Native.CheckStatus(status);
        Handle = handle;
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

    internal byte GetGpioStates(bool lowByte)
    {
        Span<byte> outBuffer = stackalloc byte[2];
        Span<byte> inBuffer = stackalloc byte[1];
        outBuffer[0] = (byte)(lowByte ? Native.FT_OPCODE.ReadDataBitsLowByte : Native.FT_OPCODE.ReadDataBitsHighByte);
        outBuffer[1] = (byte)Native.FT_OPCODE.SendImmediate;
        Write(outBuffer);
        ReadInto(inBuffer);
        return inBuffer[0];
    }

    internal void SetGpioDirectionAndState(bool lowByte, byte direction, byte state)
    {
        Span<byte> outBuffer = stackalloc byte[3];
        outBuffer[0] = (byte)(lowByte ? Native.FT_OPCODE.SetDataBitsLowByte : Native.FT_OPCODE.SetDataBitsHighByte);
        outBuffer[1] = state; //data
        outBuffer[2] = direction; //direction 1 == output, 0 == input

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

    internal int ReadInto(Span<byte> buffer, int timeoutMs = 5000)
    {
        var totalRead = 0;
        var startTime = Environment.TickCount;

        while (totalRead < buffer.Length)
        {
            var available = GetAvailableBytes();

            if (available > 0)
            {
                // Calculate how many bytes to read (don't read more than needed)
                var toRead = Math.Min((int)available, buffer.Length - totalRead);
                uint bytesRead = 0;

                // FIXED: Properly get reference to the buffer slice
                var remainingBuffer = buffer.Slice(totalRead, toRead);

                try
                {
                    var status = Native.Ftd2xx.FT_Read(
                        Handle,
                        in MemoryMarshal.GetReference(remainingBuffer), // Correct way to get buffer reference
                        (uint)toRead,
                        ref bytesRead);

                    Native.CheckStatus(status);
                    totalRead += (int)bytesRead;

                    // If we didn't read any bytes despite them being available, something is wrong
                    if (bytesRead == 0 && available > 0)
                    {
                        Thread.Sleep(1); // Small delay before retry
                    }
                }
                catch
                {
                    throw;
                }
            }
            else
            {
                // Check for timeout
                if (Environment.TickCount - startTime > timeoutMs)
                {
                    throw new TimeoutException($"Timeout reading data. Expected {buffer.Length} bytes, got {totalRead} bytes");
                }

                // Small delay to prevent busy waiting
                Thread.Sleep(1);
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
        var p = pin as FtdiPin ?? throw new ArgumentException();

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

        var channel = p.SupportedChannels?.FirstOrDefault(channel => channel is IDigitalChannelInfo) as IDigitalChannelInfo;
        if (channel == null)
        {
            throw new NotSupportedException($"Pin {pin.Name} does not support digital output");
        }

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
        return CreateI2cBus(1, I2cBusSpeed.Standard);
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

    /// <inheritdoc/>
    public ISpiBus CreateSpiBus(int channel, Frequency speed)
    {
        if (channel != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(channel), "Only channel 0 is supported");
        }

        return CreateSpiBus(channel, new SpiClockConfiguration(speed));
    }

    /// <summary>
    /// Creates a SPI bus instance for the requested control pins and bus speed
    /// </summary>
    /// <param name="configuration">The SPI clock configuration.</param>
    /// <returns>Returns an instance of <see cref="ISpiBus"/>.</returns>
    public ISpiBus CreateSpiBus(SpiClockConfiguration configuration)
    {
        return CreateSpiBus(0, configuration);
    }

    /// <summary>
    /// Creates an SPI bus on the expander.
    /// </summary>
    /// <param name="channel">The channel number to use for the SPI bus.</param>
    /// <returns>Returns an instance of <see cref="ISpiBus"/>.</returns>
    public ISpiBus CreateSpiBus(int channel = 0)
    {
        return CreateSpiBus(channel, new SpiClockConfiguration(1000000.Hertz()));
    }

    /// <inheritdoc/>
    public IDigitalInputPort CreateDigitalInputPort(IPin pin, ResistorMode resistorMode)
    {
        switch (resistorMode)
        {
            case ResistorMode.InternalPullUp:
            case ResistorMode.InternalPullDown:
                throw new Exception("Internal resistors are not supported");
        }

        var p = pin as FtdiPin ?? throw new ArgumentException();

        // TODO: make sure the pin isn't in use

        if (p.IsLowByte)
        {
            // update the expanders direction mask to make this an output
            GpioDirectionLow &= (byte)~(byte)pin.Key;

            SetGpioDirectionAndState(
                p.IsLowByte,
                GpioDirectionLow,
                GpioStateLow);
        }
        else
        {
            GpioDirectionHigh &= (byte)~(byte)pin.Key;

            SetGpioDirectionAndState(
                p.IsLowByte,
                GpioDirectionHigh,
                GpioStateHigh);
        }

        var channel = pin.SupportedChannels?.First() as IDigitalChannelInfo;
        if (channel == null)
        {
            throw new NotSupportedException($"Pin {pin.Name} does not support digital input");
        }

        return new DigitalInputPort(this, pin, channel, resistorMode);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (Handle != IntPtr.Zero)
        {
            Native.Ftd2xx.FT_Close(Handle);
            Handle = IntPtr.Zero;
        }
    }
}
