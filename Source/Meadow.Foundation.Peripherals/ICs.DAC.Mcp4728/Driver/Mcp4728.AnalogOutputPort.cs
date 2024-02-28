using Meadow.Hardware;
using System;
using System.Threading.Tasks;
using Meadow.Units;

namespace Meadow.Foundation.ICs.DAC;

public partial class Mcp4728
{
    /// <summary>
    /// Enumeration for DAC Channel Names
    /// </summary>
    public enum DACChannel : byte
    {
        ChannelA = 0,
        ChannelB = 1,
        ChannelC = 2,
        ChannelD = 3
    }

    /// <summary>
    /// Enumeration for Channel settings
    /// </summary>
    [Flags]
    public enum ChannelSettings : byte
    {
        /// <summary> External VRef </summary>
        ExternalVRef = 0x00,
        /// <summary> Internal VRef 2.048V </summary>
        InternalVRef = 0x80,
        /// <summary> Normal Operation </summary>
        ModeNormal = 0x00,
        /// <summary> PowerDown, 1kOhm load </summary>
        ModePowerDown1k = 0x20,
        /// <summary> PowerDown, 100kOhm load </summary>
        ModePowerDown100k = 0x40,
        /// <summary> PowerDown, 500kOhm load </summary>
        ModePowerDown500k = 0x60,
        /// <summary> 1x gain</summary>
        OutputGain1x = 0x00,
        /// <summary> 2x gain, only applies for Internal VRef  </summary>
        OutputGain2x = 0x10,
    }

    /// <summary>
    /// Represents an analog output port for interfacing with the Mcp4728 DAC.
    /// </summary>
    public class AnalogOutputPort : IAnalogOutputPort
    {
        private readonly Mcp4728 _controller;
        private readonly DACChannel _channel;

        /// <inheritdoc/>
        public IAnalogChannelInfo Channel { get; }

        /// <inheritdoc/>
        public IPin Pin { get; }

        /// <summary>
        /// Voltage resolution of the <see cref="IAnalogOutputPort"/>
        /// </summary>
        public Units.Voltage VoltageLSB { get; internal set; }

        /// <summary>
        /// Gets or sets the Channel Settings for the port
        /// </summary>
        public ChannelSettings Settings { get; set; }

        /// <summary>
        /// Gets or sets whether the port has a buffered input
        /// </summary>
        public bool BufferedInput { get; set; }

        internal AnalogOutputPort(IPin pin, IAnalogChannelInfo channelDefinition, Mcp4728 controller, DACChannel hardwareChannel)
        {
            _controller = controller;
            _channel = hardwareChannel;

            Channel = channelDefinition;
            Pin = pin;

            BufferedInput = false;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // nothing to do
        }

        /// <inheritdoc/>
        public Task GenerateOutput(uint digitalValue)
        {
            if (digitalValue > (1 << Channel.Precision) - 1)
            {
                throw new ArgumentOutOfRangeException($"Port supports a maximum of {Channel.Precision} bits");
            }

            _controller.WriteOutput((ushort)digitalValue, _channel, Settings, BufferedInput);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Instructs the IAnalogOutputPort to generate an analog output signal corresponding to the provided voltage value
        /// </summary>
        /// <param name="voltageValue">The target voltage output, which will be automatically adjusted to the nearest selectable value.</param>
        public Task GenerateOutput(Voltage voltageValue)
        {
            if (voltageValue.Volts < 0)
                throw new ArgumentOutOfRangeException(nameof(voltageValue), "Port does not support negative output voltages.");
            if (voltageValue.Volts > VoltageLSB.Volts * ((1 << Channel.Precision) - 1))
                throw new ArgumentOutOfRangeException(nameof(voltageValue), voltageValue.Volts, "Port does not support requested output voltage.");

            return GenerateOutput((uint)Math.Round(voltageValue.Volts / VoltageLSB.Volts));
        }

        /// <summary>
        /// Turns off the output, setting the pin to high impedance
        /// </summary>
        public void HighZ()
        {
            _controller.WriteOutput(0, _channel, Settings | ChannelSettings.ModePowerDown100k, false);
        }
    }
}