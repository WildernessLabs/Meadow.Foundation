using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using System;

namespace Meadow.Foundation.Sensors.Temperature
{
    /// <summary>
    /// Thermistor temperature sensor object
    /// </summary>
    public abstract class Thermistor : SamplingSensorBase<Units.Temperature>, ITemperatureSensor
    {
        protected IAnalogInputPort AnalogInput { get; }
        public abstract Resistance ThermistorResistanceAt25C { get; }
        public virtual Units.Temperature NominalTemperature => new Units.Temperature(25, Units.Temperature.UnitType.Celsius);

        protected Thermistor(IAnalogInputPort analogInput)
        {
            this.AnalogInput = analogInput;
        }

        /// <summary>
        /// The temperature from the last reading.
        /// </summary>
        public Units.Temperature? Temperature { get; protected set; }

        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated;
    }
}