using System;
using Meadow.Foundation.ICs.IOExpanders.Ports;
using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public interface IMcp23x
    {
        event EventHandler<IOExpanderMultiPortInputChangedEventArgs> InputChanged;
        IMcpGpioPorts Ports { get; }

        void ConfigureInputPort(
            IPin pin,
            bool enablePullUp = false,
            InterruptMode interruptMode = InterruptMode.None);

        IDigitalInputPort CreateDigitalInputPort(
            IPin pin,
            InterruptMode interruptMode = InterruptMode.None,
            ResistorMode resistorMode = ResistorMode.Disabled,
            double debounceDuration = 0,
            double glitchDuration = 0);

        /// <summary>
        /// Creates a new DigitalOutputPort using the specified pin and initial state.
        /// </summary>
        /// <param name="pin">The pin number to create the port on.</param>
        /// <param name="initialState">Whether the pin is initially high or low.</param>
        /// <returns></returns>
        IDigitalOutputPort CreateDigitalOutputPort(
            IPin pin,
            bool initialState = false,
            OutputType outputType = OutputType.OpenDrain);

        /// <summary>
        /// Gets the value of a particular pin. If the pin is currently configured
        /// as an output, this will change the configuration.
        /// </summary>
        /// <param name="pin"></param>
        /// <returns></returns>
        bool ReadPin(IPin pin);

        /// <summary>
        /// Sets the direction of a particular port.
        /// </summary>
        /// <param name="pin"></param>
        /// <param name="direction"></param>
        void SetPortDirection(IPin pin, PortDirectionType direction);

        /// <summary>
        /// Sets a particular pin's value. If that pin is not
        /// in output mode, this method will first set its
        /// mode to output.
        /// </summary>
        /// <param name="pin">The pin to write to.</param>
        /// <param name="value">The value to write. True for high, false for low.</param>
        void WritePin(IPin pin, bool value);
    }
}
