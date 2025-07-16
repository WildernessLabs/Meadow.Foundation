using Meadow.Hardware;
using Meadow.Units;
using System.Threading.Tasks;

namespace Meadow.Foundation.IOExpanders;

public abstract partial class T3xxx
{
    /// <summary>
    /// Represents a voltage output port on a Temco Controls T3 module.
    /// Provides functionality to set voltage output values on analog output channels.
    /// </summary>
    public class VoltageOutputPort : IVoltageOutputPort
    {
        private readonly T3xxx _module;
        private readonly ushort _outputRegister;

        /// <summary>
        /// Gets the pin associated with this voltage output port.
        /// </summary>
        /// <value>The IPin representing the physical connection for this voltage output.</value>
        public IPin Pin { get; }

        /// <summary>
        /// Initializes a new instance of the VoltageOutputPort class and configures the output channel.
        /// Sets the channel to manual mode and configures the voltage range to 0-10V.
        /// </summary>
        /// <param name="module">The parent T3xxx module that owns this port.</param>
        /// <param name="pin">The pin associated with this voltage output port.</param>
        /// <param name="initialVoltage">The initial voltage value to set on the output.</param>
        /// <param name="autoManualRegister">The register address for setting manual/auto mode.</param>
        /// <param name="rangeRegister">The register address for configuring the output range.</param>
        /// <param name="outputRegister">The register address for setting the output value.</param>
        public VoltageOutputPort(
            T3xxx module,
            IPin pin,
            Voltage initialVoltage,
            ushort autoManualRegister,
            ushort rangeRegister,
            ushort outputRegister)
        {
            _module = module;
            Pin = pin;

            // mode must be set to 1 (manual)
            _module.WriteHoldingRegister(autoManualRegister, 1).GetAwaiter().GetResult();
            _module.WriteHoldingRegister(rangeRegister, (ushort)AnalogOutputRange.Voltage_0_10).GetAwaiter().GetResult();

            _outputRegister = outputRegister;

            SetOutput(initialVoltage);
        }

        /// <summary>
        /// Asynchronously sets the output voltage value for this port.
        /// The voltage value is converted to millivolts before writing to the register.
        /// </summary>
        /// <param name="value">The voltage value to output on this port.</param>
        /// <returns>A Task representing the asynchronous set operation.</returns>
        public Task SetOutput(Voltage value)
        {
            return _module.WriteHoldingRegister(_outputRegister, (ushort)value.Millivolts);
        }
    }
}
