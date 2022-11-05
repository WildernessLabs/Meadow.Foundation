using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Analog input multiplexer abstraction
    /// </summary>
    public interface IAnalogInputMultiplexer
    {
        /// <summary>
        /// The port connected to the Enable pin of the mux (otherwise must be tied low)
        /// </summary>
        IDigitalOutputPort? EnablePort { get; }

        /// <summary>
        /// The analog input connected to the Mux output pin (Z)
        /// </summary>
        IAnalogInputPort Signal { get; }

        /// <summary>
        /// Disables the multiplexer (if an enable port was provided)
        /// </summary>
        void Disable();
        
        /// <summary>
        /// Enables the multiplexer (if an enable port was provided)
        /// </summary>
        void Enable();

        /// <summary>
        /// Sets the channel input that will be routed to the mux Signal output
        /// </summary>
        /// <param name="channel"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        void SetInputChannel(int channel);
    }
}