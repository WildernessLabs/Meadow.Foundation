using Meadow.Hardware;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.ICs.DAC;

public abstract partial class Mcp492x
{
    /// <summary>
    /// Represents an analog output port for interfacing with the MCP492x DAC.
    /// </summary>
    public class AnalogOutputPort : IAnalogOutputPort
    {
        private readonly Mcp492x _controller;
        private readonly Channel _channel;

        /// <inheritdoc/>
        public IAnalogChannelInfo Channel { get; }
        /// <inheritdoc/>
        public IPin Pin { get; }

        /// <summary>
        /// Gets or sets the Gain for the port
        /// </summary>
        public Gain Gain { get; set; }

        /// <summary>
        /// Gets or sets whether the port has a buffered input
        /// </summary>
        public bool BufferedInput { get; set; }

        internal AnalogOutputPort(IPin pin, IAnalogChannelInfo channelDefinition, Mcp492x controller, Channel hardwareChannel)
        {
            _controller = controller;
            _channel = hardwareChannel;

            Channel = channelDefinition;
            Pin = pin;

            Gain = Gain.Gain1x;
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

            _controller.WriteOutput((ushort)digitalValue, _channel, Gain, BufferedInput, true);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Turns off the output, setting the pin to high impedance
        /// </summary>
        public void HighZ()
        {
            _controller.WriteOutput(0, _channel, Gain, false, false);
        }
    }
}