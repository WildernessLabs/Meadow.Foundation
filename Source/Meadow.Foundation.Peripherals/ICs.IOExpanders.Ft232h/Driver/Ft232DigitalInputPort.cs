using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represents a digital input port implementation for the FT232 bus.
    /// </summary>
    public sealed class Ft232DigitalInputPort : DigitalInterruptPortBase
    {
        private IFt232Bus _bus;

        /// <summary>
        /// Instantiates a <see cref="Ft232DigitalInputPort"/>.
        /// </summary>
        /// <param name="pin">The pin connected to the input port.</param>
        /// <param name="info">The digital channel info associated with the pin.</param>
        /// <param name="bus">The FT232 bus instance.</param>
        internal Ft232DigitalInputPort(IPin pin, IDigitalChannelInfo info, IFt232Bus bus)
            : base(pin, info)
        {
            _bus = bus;
        }

        /// <summary>
        /// Gets the current state of the input port.
        /// </summary>
        /// <returns>The current state of the input port.</returns>
        public override bool State
        {
            get
            {
                // reads all 8 pis at once
                Native.Mpsse.FT_ReadGPIO(_bus.Handle, out byte state);
                // the pin key is the mask
                return (state & (byte)Pin.Key) != 0;
            }
        }

        /// <summary>
        /// Gets or sets the resistor mode of the input port. 
        /// </summary>
        /// <exception cref="NotSupportedException">The FT232 does not support internal resistors.</exception>
        public override ResistorMode Resistor
        {
            get => ResistorMode.Disabled;
            set => throw new NotSupportedException("The FT232 does not support internal resistors");
        }

        /// <summary>
        /// Gets or sets the debounce duration of the input port.
        /// </summary>
        /// <exception cref="NotSupportedException">The FT232 does not support debounce.</exception>
        public override TimeSpan DebounceDuration
        {
            get => TimeSpan.Zero;
            set => throw new NotSupportedException("The FT232 does not support debounce");
        }

        /// <summary>
        /// Gets or sets the glitch duration of the input port.
        /// </summary>
        /// <exception cref="NotSupportedException">The FT232 does not support glitch filtering.</exception>
        public override TimeSpan GlitchDuration
        {
            get => TimeSpan.Zero;
            set => throw new NotSupportedException("The FT232 does not support glitch filtering");
        }
    }
}