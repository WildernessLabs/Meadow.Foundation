using FTD2XX;
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Linq;

namespace Meadow.Foundation.ICs.IOExpanders;

public abstract partial class FtdiExpander :
    IPinController,
    IDigitalInputOutputController,
    ISpiController,
    II2cController
{
    internal byte GpioDirectionLow { get; set; }
    internal byte GpioStateLow { get; set; }
    internal byte GpioDirectionHigh { get; set; }
    internal byte GpioStateHigh { get; set; }

    internal FTDI Device { get; private set; }
    internal int Index { get; private set; }
    internal string? SerialNumber { get; private set; }
    internal string? Description { get; private set; }

    /// <inheritdoc/>
    public abstract II2cBus CreateI2cBus(int channel = 0, I2cBusSpeed busSpeed = I2cBusSpeed.Standard);
    /// <inheritdoc/>
    public abstract ISpiBus CreateSpiBus(int channel, SpiClockConfiguration configuration);

    /// <summary>
    /// The pins
    /// </summary>
    public PinDefinitions Pins { get; }

    internal static FtdiExpander Create(
        FTDI device,
        int index,
        FT_DEVICE deviceType,
        string serialNumber,
        string description)
    {
        FtdiExpander expander = deviceType switch
        {
            FT_DEVICE.FT_DEVICE_232H => new Ft232h
            {
                Device = device,
                Index = index,
                SerialNumber = serialNumber,
                Description = description,
            },
            _ => throw new NotSupportedException(),
        };

        if (!device.IsOpen)
        {
            device
                .OpenByIndex(index)
                .ThrowIfNotOK();
        }

        expander.ConfigureMpsse(device);

        return expander;
    }

    internal FtdiExpander()
    {
        Pins = new PinDefinitions(this);
    }

    private void ConfigureMpsse(FTDI _device)
    {
        _device.ResetDevice();

        _device.SetTimeouts(1000, 1000).ThrowIfNotOK();
        _device.SetLatency(16).ThrowIfNotOK();
        _device.SetFlowControl(FT_FLOW_CONTROL.FT_FLOW_RTS_CTS, 0x00, 0x00).ThrowIfNotOK();
        _device.SetBitMode(0x00, 0x00).ThrowIfNotOK(); // RESET
        _device.SetBitMode(0x00, 0x02).ThrowIfNotOK(); // MPSSE

        _device.FlushBuffer();

        /***** Synchronize the MPSSE interface by sending bad command 0xAA *****/
        _device.Write(new byte[] { 0xAA }).ThrowIfNotOK();
        byte[] rx1 = _device.ReadBytes(2, out FT_STATUS status1);
        status1.ThrowIfNotOK();
        if ((rx1[0] != 0xFA) || (rx1[1] != 0xAA))
            throw new InvalidOperationException($"bad echo bytes: {rx1[0]} {rx1[1]}");

        /***** Synchronize the MPSSE interface by sending bad command 0xAB *****/
        _device.Write(new byte[] { 0xAB }).ThrowIfNotOK();
        byte[] rx2 = _device.ReadBytes(2, out FT_STATUS status2);
        status2.ThrowIfNotOK();
        if ((rx2[0] != 0xFA) || (rx2[1] != 0xAB))
            throw new InvalidOperationException($"bad echo bytes: {rx2[0]} {rx2[1]}");

        const uint ClockDivisor = 199; //49 for 200 KHz, 199 for 100 KHz
        const byte I2C_Data_SDAhi_SCLhi = 0x03;
        const byte I2C_Dir_SDAout_SCLout = 0x03;

        int numBytesToSend = 0;
        byte[] buffer = new byte[100];
        buffer[numBytesToSend++] = 0x8A;   // Disable clock divide by 5 for 60Mhz master clock
        buffer[numBytesToSend++] = 0x97;   // Turn off adaptive clocking
        buffer[numBytesToSend++] = 0x8C;   // Enable 3 phase data clock, used by I2C to allow data on both clock edges
                                           // The SK clock frequency can be worked out by below algorithm with divide by 5 set as off
                                           // SK frequency  = 60MHz /((1 +  [(1 +0xValueH*256) OR 0xValueL])*2)
        buffer[numBytesToSend++] = 0x86;   //Command to set clock divisor
        buffer[numBytesToSend++] = (byte)(ClockDivisor & 0x00FF);  //Set 0xValueL of clock divisor
        buffer[numBytesToSend++] = (byte)((ClockDivisor >> 8) & 0x00FF);   //Set 0xValueH of clock divisor
        buffer[numBytesToSend++] = 0x85;           // loopback off

        buffer[numBytesToSend++] = 0x9E;       //Enable the FT232H's drive-zero mode with the following enable mask...
        buffer[numBytesToSend++] = 0x07;       // ... Low byte (ADx) enables - bits 0, 1 and 2 and ... 
        buffer[numBytesToSend++] = 0x00;       //...High byte (ACx) enables - all off

        buffer[numBytesToSend++] = 0x80;   //Command to set directions of lower 8 pins and force value on bits set as output 
        buffer[numBytesToSend++] = I2C_Data_SDAhi_SCLhi;
        buffer[numBytesToSend++] = I2C_Dir_SDAout_SCLout;

        byte[] msg = buffer.Take(numBytesToSend).ToArray();
        _device.Write(msg).ThrowIfNotOK();
    }

    internal byte GetGpioStates(bool lowByte)
    {
        Span<byte> outBuffer = stackalloc byte[2];
        Span<byte> inBuffer = stackalloc byte[1];
        outBuffer[0] = (byte)(lowByte ? Native.FT_OPCODE.ReadDataBitsLowByte : Native.FT_OPCODE.ReadDataBitsHighByte);
        outBuffer[1] = (byte)Native.FT_OPCODE.SendImmediate;
        Device.Write(outBuffer.ToArray());
        inBuffer = Device.ReadBytes(inBuffer.Length, out FT_STATUS status);
        return inBuffer[0];
    }

    internal void SetGpioDirectionAndState(bool lowByte, byte direction, byte state)
    {
        Span<byte> outBuffer = stackalloc byte[3];
        outBuffer[0] = (byte)(lowByte ? Native.FT_OPCODE.SetDataBitsLowByte : Native.FT_OPCODE.SetDataBitsHighByte);
        outBuffer[1] = state; //data
        outBuffer[2] = direction; //direction 1 == output, 0 == input

        // Console.WriteLine($"{(BitConverter.ToString(outBuffer.ToArray()))}");
        Device.Write(outBuffer.ToArray());

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
        throw new NotSupportedException();
        /*
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

        return new DigitalInputPort(this, pin, (pin.SupportedChannels.First() as IDigitalChannelInfo)!, resistorMode);
        */
    }
}
