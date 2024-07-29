using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Linq;

namespace Meadow.Foundation.ICs.DAC;

/// <summary>
/// This class represents an MCP492x digital-to-analog converter (DAC) and implements 
/// the ISpiPeripheral and IAnalogOutputController interfaces.
/// </summary>
public abstract partial class Mcp492x : ISpiPeripheral, IAnalogOutputController
{
    private readonly int OffsetChannelSelect = 15;
    private readonly int OffsetBufferControl = 14;
    private readonly int OffsetGain = 13;
    private readonly int OffsetPower = 12;

    private readonly ISpiBus spiBus;
    private readonly IDigitalOutputPort? chipSelectPort;

    /// <summary>
    /// Gets the default SPI bus mode (Mode0).
    /// </summary>
    public SpiClockConfiguration.Mode DefaultSpiBusMode => SpiClockConfiguration.Mode.Mode0;

    /// <summary>
    /// Gets the default SPI bus speed (20 MHz).
    /// </summary>
    public Frequency DefaultSpiBusSpeed => new Frequency(20, Frequency.UnitType.Megahertz);

    /// <summary>
    /// Gets or sets the SPI bus mode.
    /// </summary>
    public SpiClockConfiguration.Mode SpiBusMode { get; set; }

    /// <summary>
    /// Gets or sets the SPI bus speed.
    /// </summary>
    public Frequency SpiBusSpeed { get; set; }

    internal enum Channel
    {
        ChannelA = 0,
        ChannelB = 1
    }

    /// <summary>
    /// Enumeration for gain settings
    /// </summary>
    public enum Gain
    {
        /// <summary>
        /// 2x gain
        /// </summary>
        Gain2x = 0,
        /// <summary>
        /// 1x gain
        /// </summary>
        Gain1x = 1
    }

    /// <summary>
    /// Initializes a new instance of the Mcp492x class.
    /// </summary>
    /// <param name="spiBus">The SPI bus.</param>
    /// <param name="chipSelectPort">The chip select port (can be null).</param>
    public Mcp492x(ISpiBus spiBus, IDigitalOutputPort? chipSelectPort)
    {
        this.spiBus = spiBus;
        this.chipSelectPort = chipSelectPort;
        SpiBusMode = DefaultSpiBusMode;
        SpiBusSpeed = DefaultSpiBusSpeed;
    }

    internal void WriteOutput(ushort value12Bit, Channel channel, Gain gain, bool bufferedInput, bool poweredOn)
    {
        var register = value12Bit & 0x0fff;
        register |= ((poweredOn ? 1 : 0) << OffsetPower);
        register |= ((int)gain << OffsetGain);
        register |= ((bufferedInput ? 1 : 0) << OffsetBufferControl);
        register |= ((int)channel << OffsetChannelSelect);

        Span<byte> data = stackalloc byte[]
            {
            (byte)((register & 0xff00) >> 8),
            (byte)(register & 0xff)
        };

        spiBus.Write(chipSelectPort, data);
    }

    /// <inheritdoc/>
    public IAnalogOutputPort CreateAnalogOutputPort(IPin pin)
    {
        return CreateAnalogOutputPort(pin, Gain.Gain1x, false);
    }

    /// <inheritdoc cref="CreateAnalogOutputPort(IPin)"/>
    public IAnalogOutputPort CreateAnalogOutputPort(IPin pin, Gain gain = Gain.Gain1x, bool bufferedInput = false)
    {
        if (pin.Controller == null || !pin.Controller.Equals(this))
        {
            throw new ArgumentException("The provided pin must be on this controller");
        }

        return new AnalogOutputPort(
            pin,
            pin.SupportedChannels?.First(c => c is IAnalogChannelInfo) as IAnalogChannelInfo
                ?? throw new ArgumentException("Pin does not support analog output"),
            this,
            (Channel)(pin.Key))
        {
            Gain = gain,
            BufferedInput = bufferedInput,
        };
    }
}