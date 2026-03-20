using Meadow.Hardware;
using Meadow.Units;
using System.Threading.Tasks;

namespace Meadow.Foundation.IOExpanders;

public abstract partial class T3xxx
{
    /// <summary>
    /// Represents a current input port on a Temco Controls T3 module.
    /// Provides functionality to read current measurements from analog input channels.
    /// </summary>
    public class CurrentInputPort : ICurrentInputPort
    {
        private readonly T3xxx _module;
        private readonly ushort _valueRegister;

        /// <summary>
        /// Gets the pin associated with this current input port.
        /// </summary>
        /// <value>The IPin representing the physical connection for this current input.</value>
        public IPin Pin { get; }

        /// <summary>
        /// Initializes a new instance of the CurrentInputPort class.
        /// </summary>
        /// <param name="module">The parent T3xxx module that owns this port.</param>
        /// <param name="pin">The pin associated with this current input port.</param>
        /// <param name="valueRegister">The Modbus register address containing the current reading.</param>
        internal CurrentInputPort(
            T3xxx module,
            IPin pin,
            ushort valueRegister)
        {
            _module = module;
            Pin = pin;
            _valueRegister = valueRegister;
        }

        /// <summary>
        /// Asynchronously reads the current value from the input port.
        /// The raw register value is converted from centiamp units to current.
        /// </summary>
        /// <returns>A ValueTask containing the measured current value.</returns>
        public async ValueTask<Current> Read()
        {
            var register = await _module.ReadHoldingRegister(_valueRegister);

            return new Current(register / 100d, Current.UnitType.Milliamps);
        }
    }
}
