using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using System;

namespace Meadow.Foundation.Sensors.Temperature
{
    /// <summary>
    /// Thermistor temperature sensor object
    /// </summary>
    /// <remarks>
    /// Typical wiring
    /// 
    /// 3.3V >-----[ 10k R ]---+-------------&lt; Analog_in
    ///                        |
    ///                        +---[ TM ]--- &lt; GND
    /// </remarks>
    public abstract class Thermistor : SamplingSensorBase<Units.Temperature>, ITemperatureSensor
    {
        /// <summary>
        /// The analog input eing used to determine output voltage of the voltage divider circuit
        /// </summary>
        protected IAnalogInputPort AnalogInput { get; }
        /// <summary>
        /// The nominal resistance of the thermistor (e.g. 10kOhm for a 10k thermistor)
        /// </summary>
        public abstract Resistance NominalResistance { get; }
        /// <summary>
        /// The nominal temperature for the nominal resistance, typically 25C.
        /// </summary>
        public virtual Units.Temperature NominalTemperature => new Units.Temperature(25, Units.Temperature.UnitType.Celsius);

        /// <summary>
        /// Creates a new Thermistor object using the provided analog input
        /// </summary>
        /// <param name="analogInput">The analog input reading the thermistor voltage divider output</param>
        protected Thermistor(IAnalogInputPort analogInput)
        {
            this.AnalogInput = analogInput;
            this.AnalogInput.StartUpdating();

            base.Updated += (s, e) => TemperatureUpdated?.Invoke(this, e);
        }

        /// <summary>
        /// The temperature from the last reading.
        /// </summary>
        public Units.Temperature? Temperature { get; protected set; }

        /// <summary>
        /// Raised when the temperature is updated
        /// </summary>
        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };
    }
}
