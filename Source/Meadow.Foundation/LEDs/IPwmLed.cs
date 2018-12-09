using Meadow.Hardware;

namespace Meadow.Foundation.LEDs
{
    /// <summary>
    /// Defines a Light Emitting Diode (LED) controlled by a pulse-width-modulation
    /// (PWM) signal to limit current.
    /// </summary>
    public interface IPwmLed : ILed
    {
        new IPwmPort Port { get; }
    }
}
