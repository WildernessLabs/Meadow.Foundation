using Meadow.Hardware;
using System;
using System.Linq;

namespace Meadow.Foundation.ICs.IOExpanders;

public partial class Pca9685
{
    /// <summary>
    /// Represents a digital output port for the PCA9685 I/O expander.
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
        /// <remarks>
        /// Setting the state to <c>true</c> turns the output on, and setting it to <c>false</c> turns it off.
        /// Updates the PWM signal on the PCA9685 accordingly.
        /// </remarks>
        public bool State
        {
            get => state;
            set
            {
                if (value == state) { return; }

                state = value;

                if (state)
                {
                    controller.SetPwm(portNumber, 4096, 0); // Set output high
                }
                else
                {
                    controller.SetPwm(portNumber, 0, 4096); // Set output low
                }
            }
        }
        private bool state;

        /// <summary>
        /// Gets the digital channel information for the port.
        /// </summary>
        public IDigitalChannelInfo Channel { get; }

        /// <summary>
        /// Gets the pin associated with the digital output port.
        /// </summary>
        public IPin Pin { get; }

        private readonly Pca9685 controller;
        private readonly byte portNumber;

        /// <summary>
        /// Initializes a new instance of the <see cref="DigitalOutputPort"/> class.
        /// </summary>
        /// <param name="controller">The PCA9685 controller instance that manages the port.</param>
        /// <param name="pin">The pin associated with the digital output port.</param>
        /// <param name="initialState">The initial state of the digital output port.</param>
        internal DigitalOutputPort(Pca9685 controller, IPin pin, bool initialState)
        {
            InitialState = initialState;
            State = initialState;
            Pin = pin;

            this.controller = controller;
            Channel = (IDigitalChannelInfo)pin.SupportedChannels.First(c => c is IDigitalChannelInfo);

            portNumber = (byte)pin.Key;
        }

        /// <summary>
        /// Disposes of the digital output port.
        /// </summary>
        /// <remarks>
        /// This method is currently a no-op, but it is provided to implement <see cref="IDisposable"/>.
        /// </remarks>
        public void Dispose()
        {
        }
    }
}
