using System;
using Meadow.Hardware;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Meadow.Units;

namespace Meadow.Foundation.ICs.DAC;

/// <summary>
/// This class represents an MCP4728 quad digital-to-analog converter (DAC) and implements 
/// the II2CPeripheral and IAnalogOutputController interfaces.
/// </summary>
public partial class Mcp4728 : II2cPeripheral, IAnalogOutputController
{
    private readonly II2cBus i2cBus;
    private readonly byte i2cAddress;

    /// <summary>
    /// The default I2C address for the peripheral
    /// </summary>
    public byte DefaultI2cAddress => (byte)Addresses.Default;

    /// <summary>
    /// Default I2C Bus Speed to use for communication.
    /// </summary>
    public static I2cBusSpeed DefaultBusSpeed => I2cBusSpeed.Fast;

    /// <summary>
    /// Initializes a new instance of the Mcp4728 class.
    /// </summary>
    /// <param name="bus">The I2C bus.</param>
    /// <param name="address">The I2C bus address of the device.</param>
    public Mcp4728(II2cBus bus, byte address = (byte)Addresses.Default)
    {
        i2cBus = bus;
        i2cAddress = address;
        Pins = new PinDefinitions(this);

    }

    /// <summary>
    /// Valid I2C addresses for the device
    /// </summary>
    public enum Addresses : byte
    {
        /// <summary> Bus address 0x60 </summary>
        Address0 = 0x60,
        /// <summary> Bus address 0x61, which requires programming the address bits on the IC. </summary>
        Address1 = 0x61,
        /// <summary> Bus address 0x62, which requires programming the address bits on the IC. </summary>
        Address2 = 0x62,
        /// <summary> Bus address 0x63, which requires programming the address bits on the IC. </summary>
        Address3 = 0x63,
        /// <summary> Bus address 0x64, which requires programming the address bits on the IC. </summary>
        Address4 = 0x64,
        /// <summary> Bus address 0x65, which requires programming the address bits on the IC. </summary>
        Address5 = 0x65,
        /// <summary> Bus address 0x66, which requires programming the address bits on the IC. </summary>
        Address6 = 0x66,
        /// <summary> Bus address 0x67, which requires programming the address bits on the IC. </summary>
        Address7 = 0x67,
        /// <summary> Bus address by default </summary>
        Default = Address0,
    }

    /// <summary>
    /// The pins
    /// </summary>
    public PinDefinitions Pins { get; }

    /// <summary>
    /// Represents the pin definitions for the MCP4728 DAC.
    /// </summary>
    public class PinDefinitions : IPinDefinitions
    {
        /// <summary>
        /// Analog-digital converter precision
        /// </summary>
        public virtual byte DACPrecisionBits => 12;

        /// <summary>
        /// Collection of pins
        /// </summary>
        public IList<IPin> AllPins { get; } = new List<IPin>();

        /// <summary>
        /// The pin controller
        /// </summary>
        public IPinController? Controller { get; set; }

        /// <summary>
        /// Create a new PinDefinitions object
        /// </summary>
        public PinDefinitions(Mcp4728 mcp)
        {
            Controller = mcp;
        }

        /// <summary>
        /// Channel A pin
        /// </summary>
        public IPin ChannelA => new Pin(
            Controller,
            "A",
            DACChannel.ChannelA,
            new List<IChannelInfo> {
                new AnalogChannelInfo("A", DACPrecisionBits, false, true),
            }
        );

        /// <summary>
        /// Channel B pin
        /// </summary>
        public IPin ChannelB => new Pin(
            Controller,
            "B",
            DACChannel.ChannelB,
            new List<IChannelInfo> {
                new AnalogChannelInfo("B", DACPrecisionBits, false, true),
            }
        );

        /// <summary>
        /// Channel C pin
        /// </summary>
        public IPin ChannelC => new Pin(
            Controller,
            "C",
            DACChannel.ChannelC,
            new List<IChannelInfo> {
                new AnalogChannelInfo("C", DACPrecisionBits, false, true),
            }
        );

        /// <summary>
        /// Channel D pin
        /// </summary>
        public IPin ChannelD => new Pin(
            Controller,
            "D",
            DACChannel.ChannelD,
            new List<IChannelInfo> {
                new AnalogChannelInfo("D", DACPrecisionBits, false, true),
            }
        );

        /// <summary>
        /// Get Enumerator
        /// </summary>
        public IEnumerator<IPin> GetEnumerator() => AllPins.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private enum Commands : byte
    {
        MultiWrite = 0x40,
        SequentialWrite = 0x50,
        /// <summary> Set bits 2..1 to select channel. Set bit 0 to not update outputs immediately. </summary>
        SingleWrite = 0x58,
    }

    /// <summary>
    /// Write output to chip
    /// </summary>
    /// <param name="value12Bit">analog value</param>
    /// <param name="channel">channel number (enumeration)</param>
    /// <param name="settings">combination of channel settings flags</param>
    /// <param name="bufferedInput">true to hold the value until 'update output' command (LDAC pin low)</param>
    internal void WriteOutput(ushort value12Bit, DACChannel channel, ChannelSettings settings, bool bufferedInput)
    {
        Span<byte> data = stackalloc byte[] {
            (byte)((byte)Commands.SingleWrite | ((byte)channel << 1) | (byte)(bufferedInput ? 0x01 : 0x00)),
            (byte)(((value12Bit & 0xff00) >> 8) | (byte)settings),
            (byte)(value12Bit & 0xff)
        };

        i2cBus.Write(i2cAddress, data);
    }

    /// <inheritdoc/>
    public IAnalogOutputPort CreateAnalogOutputPort(IPin pin)
    {
        return CreateAnalogOutputPort(pin, false, false, false);
    }

    /// <summary>
    /// Creates an IAnalogOutputPort on the specified pin, with the specified settings.
    /// </summary>
    /// <param name="pin">The pin on which to create the port</param>
    /// <param name="internalVRef">use the internal 2.048V reference for this analog output.</param>
    /// <param name="gain2x">if using <paramref name="internalVRef"/>, also apply a 2x gain to the output.</param>
    /// <param name="buffered">if true, changes to the output are not applied until the LDAC pin is pulled low.</param>
    /// <param name="vcc">if not using <paramref name="internalVRef"/>, providing the external Vcc voltage will allow <see cref="AnalogOutputPort.VoltageResolution"/> to be correctly calculated. If not provided, 3.3V is assumed.</param>
    public IAnalogOutputPort CreateAnalogOutputPort(IPin pin, bool internalVRef, bool gain2x, bool buffered = false, Voltage? vcc = default)
    {
        if (pin.Controller == null || !pin.Controller.Equals(this))
        {
            throw new ArgumentException("The provided pin must be on this controller");
        }
        var channelInfo = pin.SupportedChannels?.First(c => c is IAnalogChannelInfo) as IAnalogChannelInfo
                                ?? throw new ArgumentException("Pin does not support analog output");

        ChannelSettings settings;
        Voltage voltageResolution, voltageReference;
        switch (internalVRef, gain2x)
        {
            case (internalVRef: true, gain2x: true):
                settings = ChannelSettings.InternalVRef | ChannelSettings.OutputGain2x;
                voltageReference = 2.048.Volts();
                voltageResolution = new Voltage(1, Voltage.UnitType.Millivolts);
                break;
            case (internalVRef: true, gain2x: false):
                settings = ChannelSettings.InternalVRef;
                voltageReference = 2.048.Volts();
                voltageResolution = new Voltage(0.5, Voltage.UnitType.Millivolts);
                break;
            default:
                settings = ChannelSettings.ExternalVRef;
                voltageReference = vcc ?? 3.3.Volts();
                voltageResolution = (voltageReference) / (1 << channelInfo.Precision);
                break;
        }

        return new AnalogOutputPort(
            pin,
            channelInfo,
            this,
            (DACChannel)(pin.Key))
        {
            Buffered = buffered,
            Settings = settings,
            VoltageReference = voltageReference,
            VoltageResolution = voltageResolution,
        };
    }
}