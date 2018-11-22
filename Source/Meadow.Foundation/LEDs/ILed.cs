using System;
namespace Meadow.Foundation.LEDs
{
    /// <summary>
    /// Defines a simple Light Emitting Diode (LED).
    /// </summary>
    public interface ILed
    {
        /// <summary>
        /// The IDigitalOutputPort that the LED is connected to.
        /// </summary>
        /// <value>The digital out.</value>
        IDigitalOutputPort Port { get; }
        bool IsOn { get; set; }
    }
}
