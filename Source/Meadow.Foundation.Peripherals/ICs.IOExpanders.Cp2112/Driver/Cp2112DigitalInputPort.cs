using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represents a digital input port implementation for the FT232 bus.
    /// </summary>
    public sealed class Cp2112DigitalInputPort : DigitalInputPortBase
    {
        private Cp2112 _device;

        /// <summary>
        /// Instantiates a <see cref="Cp2112DigitalInputPort"/>.
        /// </summary>
        /// <param name="pin">The pin connected to the input port.</param>
        /// <param name="info">The digital channel info associated with the pin.</param>
        /// <param name="device">The CP2112 device instance.</param>
        internal Cp2112DigitalInputPort(IPin pin, IDigitalChannelInfo info, Cp2112 device)
            : base(pin, info)
        {
            _device = device;
        }

        /// <summary>
        /// Gets the current state of the input port.
        /// </summary>
        /// <returns>The current state of the input port.</returns>
        public override bool State
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets or sets the resistor mode of the input port. 
        /// </summary>
        /// <exception cref="NotSupportedException">The CP2112 does not support internal resistors.</exception>
        public override ResistorMode Resistor
        {
            get => ResistorMode.Disabled;
            set => throw new NotSupportedException("The CP2112 does not support internal resistors");
        }
    }
}