using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Temperature
{
    /// <summary>
    /// Thermistor temperature sensor object
    /// </summary>
    public partial class Thermistor<T> : SamplingSensorBase<Units.Temperature>, ITemperatureSensor
        where T : class, IThermistorType, new()
    {
        private T parameters;
        private IAnalogInputPort analogInput;

        public Thermistor(IAnalogInputPort input)
        {
            parameters = Activator.CreateInstance<T>();
            analogInput = input;
        }

        public Thermistor(IAnalogInputPort input, T parameters)
        {
            this.parameters = parameters;
            analogInput = input;
        }

        /// <summary>
        /// The temperature from the last reading.
        /// </summary>
        public Units.Temperature? Temperature { get; protected set; }

        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated;

        protected override async Task<Units.Temperature> ReadSensor()
        {
            Temperature = await parameters.CalculateTemperature(analogInput);
            return Temperature.Value;
        }
    }
}