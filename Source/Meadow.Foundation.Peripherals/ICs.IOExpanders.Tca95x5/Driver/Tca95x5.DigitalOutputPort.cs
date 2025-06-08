using Meadow.Hardware;
using System.Linq;

namespace Meadow.Foundation.ICs.IOExpanders;

public partial class Tca95x5
{
    /// <summary>
    /// Represents a digital output port for the TCA9535 I/O expander.
    /// </summary>
    public class DigitalOutputPort : IDigitalOutputPort
    {
        /// <summary>
        /// Gets the initial state of the digital output port.
        /// </summary>
        public bool InitialState { get; }

        /// <summary>
        /// Gets or sets the current state of the digital output port.
        /// </summary>
        public bool State
        {
            get => controller.GetState(portNumber);
            set => controller.SetState(portNumber, value);
        }

        private readonly bool state;

        /// <summary>
        /// Gets the digital channel information for the port.
        /// </summary>
        public IDigitalChannelInfo Channel { get; }

        /// <summary>
        /// Gets the pin associated with the digital output port.
        /// </summary>
        public IPin Pin { get; }

        private readonly Tca95x5 controller;
        private readonly byte portNumber;

        /// <summary>
        /// Initializes a new instance of the <see cref="DigitalOutputPort"/> class.
        /// </summary>
        /// <param name="controller">The TCA9535 controller instance that manages the port.</param>
        /// <param name="pin">The pin associated with the digital output port.</param>
        /// <param name="initialState">The initial state of the digital output port.</param>
        internal DigitalOutputPort(Tca95x5 controller, IPin pin, bool initialState)
        {
            InitialState = initialState;
            Pin = pin;
            this.controller = controller;
            Channel = (IDigitalChannelInfo)pin.SupportedChannels.First(c => c is IDigitalChannelInfo);
            portNumber = (byte)pin.Key;
        }

        /// <summary>
        /// Disposes of the digital output port.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
