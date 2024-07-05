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
        /// <summary> A Output Channel </summary>
        ChannelA = 0,
        /// <summary> B Output Channel </summary>
        ChannelB = 1,
        /// <summary> C Output Channel </summary>
        ChannelC = 2,
        /// <summary> D Output Channel </summary>
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
        /// Maximum Output value that can be used with this <see cref="IAnalogOutputPort"/>
        /// </summary>
        public uint MaxOutputValue => (uint)(1 << Channel.Precision) - 1;

        /// <summary>
        /// Voltage reference of the <see cref="IAnalogOutputPort"/>
        /// </summary>
        public Units.Voltage VoltageReference { get; internal set; }

        /// <summary>
        /// Voltage resolution of the <see cref="IAnalogOutputPort"/>
        /// </summary>
        public Units.Voltage VoltageResolution { get; internal set; }

        /// <summary>
        /// Gets or sets the Channel Settings for the port
        /// </summary>
        public ChannelSettings Settings { get; set; }

        /// <summary>
        /// Gets or sets whether the port has a buffered value (Defers update until commanded)
        /// </summary>
        public bool Buffered { get; set; }

        internal AnalogOutputPort(IPin pin, IAnalogChannelInfo channelDefinition, Mcp4728 controller, DACChannel hardwareChannel)
        {
            _controller = controller;
            _channel = hardwareChannel;

            Channel = channelDefinition;
            Pin = pin;

            Buffered = false;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // nothing to do
        }

        /// <inheritdoc/>
        public Task GenerateOutput(uint digitalValue)
        {
            if (digitalValue > MaxOutputValue)
            {
                throw new ArgumentOutOfRangeException($"Port supports a maximum of {Channel.Precision} bits");
            }

            _controller.WriteOutput((ushort)digitalValue, _channel, Settings, Buffered);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Instructs the IAnalogOutputPort to generate an analog output signal corresponding to the provided voltage value
        /// </summary>
        /// <param name="voltageValue">The target voltage output, which will be automatically adjusted to the nearest selectable value.</param>
        public Task GenerateOutput(Voltage voltageValue)
        {
            var digitalValue = Math.Round(voltageValue.Volts / VoltageResolution.Volts);
            if (digitalValue < 0)
                throw new ArgumentOutOfRangeException(nameof(voltageValue), "Port does not support negative output voltages.");
            if (digitalValue > MaxOutputValue)
                throw new ArgumentOutOfRangeException(nameof(voltageValue), voltageValue.Volts, "Port does not support requested output voltage.");

            return GenerateOutput((uint)digitalValue);
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