using Meadow.Hardware;
using Meadow.Units;
using System.Threading.Tasks;

namespace Meadow.Foundation.IOExpanders;

public abstract partial class T3xxx
{
    /// <summary>
    /// Represents a voltage input port on a Temco Controls T3 module.
    /// Provides functionality to read voltage measurements from analog input channels.
    /// </summary>
    public class VoltageInputPort : IVoltageInputPort
    {
        private readonly T3xxx _module;
        private readonly ushort _valueRegister;

        /// <summary>
        /// Gets the pin associated with this voltage input port.
        /// </summary>
        /// <value>The IPin representing the physical connection for this voltage input.</value>
        public IPin Pin { get; }

        /// <summary>
        /// Initializes a new instance of the VoltageInputPort class.
        /// </summary>
        /// <param name="module">The parent T3xxx module that owns this port.</param>
        /// <param name="pin">The pin associated with this voltage input port.</param>
        /// <param name="valueRegister">The Modbus register address containing the voltage reading.</param>
        internal VoltageInputPort(
            T3xxx module,
            IPin pin,
            ushort valueRegister)
        {
            _module = module;
            Pin = pin;
            _valueRegister = valueRegister;
        }

        /// <summary>
        /// Asynchronously reads the current voltage value from the input port.
        /// The raw register value is converted from centivolt units to voltage.
        /// </summary>
        /// <returns>A ValueTask containing the measured voltage value.</returns>
        public async ValueTask<Voltage> Read()
        {
            var register = await _module.ReadHoldingRegister(_valueRegister);

            return new Voltage(register / 100d);
        }
    }
}
