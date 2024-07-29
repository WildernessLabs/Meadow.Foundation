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
    public abstract class Thermistor : PollingSensorBase<Units.Temperature>, ITemperatureSensor
    {
        /// <summary>
        /// The analog input port used to determine output voltage of the voltage divider circuit
        /// </summary>
        protected IAnalogInputPort AnalogInputPort { get; }
        /// <summary>
        /// The nominal resistance of the thermistor (e.g. 10kOhm for a 10k thermistor)
        /// </summary>
        public abstract Resistance NominalResistance { get; }
        /// <summary>
        /// The nominal temperature for the nominal resistance, typically 25C
        /// </summary>
        public virtual Units.Temperature NominalTemperature => new Units.Temperature(25, Units.Temperature.UnitType.Celsius);

        /// <summary>
        /// Creates a new Thermistor object using the provided analog input
        /// </summary>
        /// <param name="analogInput">The analog input reading the thermistor voltage divider output</param>
        protected Thermistor(IAnalogInputPort analogInput)
        {
            AnalogInputPort = analogInput;
            AnalogInputPort.StartUpdating();
        }

        /// <inheritdoc/>
        public override void StartUpdating(TimeSpan? updateInterval = null)
        {
            lock (samplingLock)
            {
                if (IsSampling) { return; }
                IsSampling = true;
                AnalogInputPort.StartUpdating(updateInterval);
            }

            base.StartUpdating(updateInterval);
        }

        /// <inheritdoc/>
        public override void StopUpdating()
        {
            lock (samplingLock)
            {
                if (!IsSampling) { return; }
                IsSampling = false;
                AnalogInputPort.StopUpdating();
            }

            base.StopUpdating();
        }

        /// <summary>
        /// The temperature from the last reading
        /// </summary>
        public Units.Temperature? Temperature => Conditions;
    }
}