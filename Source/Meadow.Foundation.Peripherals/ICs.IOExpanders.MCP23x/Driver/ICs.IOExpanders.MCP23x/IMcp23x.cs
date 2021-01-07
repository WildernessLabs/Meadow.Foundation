using System;
using Meadow.Foundation.ICs.IOExpanders.Ports;
using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public interface IMcp23x
    {
        /// <summary>
        /// Event that is fired whenever a configured input pin on any has an interrupt triggered.
        /// Multiple input pins may be triggered at once.
        /// </summary>
        /// <remarks>
        /// If multiple interrupts are configured, it will be more efficient to instead attach to each port's
        /// <see cref="IMcpGpioPort.InputChanged" />.
        /// </remarks>
        event EventHandler<IOExpanderMultiPortInputChangedEventArgs> InputChanged;

        /// <summary>
        /// The collection of GPIO ports on this device.
        /// </summary>
        IMcpGpioPorts Ports { get; }

        /// <summary>
        /// Configure an input port. If the port is currently an output port, it will be changed to an input port.
        /// </summary>
        /// <param name="pin">The pin to configure.</param>
        /// <param name="interruptMode">
        /// The interrupt mode to use. The device must have an interrupt configured in order to
        /// use interrupts.
        /// </param>
        /// <param name="resistorMode">
        /// Whether or not to use an internal (weak) PullUp resistor or not. PullDown resistor is not
        /// supported.
        /// </param>
        void ConfigureInputPort(
            IPin pin,
            InterruptMode interruptMode = InterruptMode.None,
            ResistorMode resistorMode = ResistorMode.Disabled);

        /// <summary>
        /// Creates a new <see cref="IDigitalInputPort" /> using the specified pin.
        /// </summary>
        /// <param name="pin">The pin to create the port on.</param>
        /// <param name="interruptMode">
        /// The interrupt mode to use. The device must have an interrupt configured in order to
        /// use interrupts.
        /// </param>
        /// <param name="resistorMode">
        /// Whether or not to use an internal (weak) PullUp resistor or not. PullDown resistor is not
        /// supported.
        /// </param>
        /// <param name="debounceDuration">The debounce duration, currently not used.</param>
        /// <param name="glitchDuration">The glitch duration, currently not used.</param>
        /// <returns>The new <see cref="IDigitalInputPort" /></returns>
        IDigitalInputPort CreateDigitalInputPort(
            IPin pin,
            InterruptMode interruptMode = InterruptMode.None,
            ResistorMode resistorMode = ResistorMode.Disabled,
            double debounceDuration = 0,
            double glitchDuration = 0);

        /// <summary>
        /// Creates a new <see cref="IDigitalOutputPort" /> using the specified pin and initial state.
        /// </summary>
        /// <param name="pin">The pin to create the port on.</param>
        /// <param name="initialState">Whether the pin is initially high or low.</param>
        /// <param name="outputType">Sets the output type. Currently this does not change anything on the MCP device itself.</param>
        /// <returns>The new <see cref="IDigitalOutputPort" /></returns>
        IDigitalOutputPort CreateDigitalOutputPort(
            IPin pin,
            bool initialState = false,
            OutputType outputType = OutputType.OpenDrain);

        /// <summary>
        /// Gets the value of a particular pin. If the pin is currently configured
        /// as an output, this will change the configuration.
        /// </summary>
        /// <param name="pin">The pin to read</param>
        /// <returns>The pin value. True for high, false for low.</returns>
        bool ReadPin(IPin pin);

        /// <summary>
        /// Reset a pin to it's initial input state.
        /// </summary>
        /// <param name="pin">The pin to reset</param>
        void ResetPin(IPin pin);

        /// <summary>
        /// Sets the direction of a particular port.
        /// </summary>
        /// <param name="pin">The pin to set</param>
        /// <param name="direction">The direction to set the port to.</param>
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
