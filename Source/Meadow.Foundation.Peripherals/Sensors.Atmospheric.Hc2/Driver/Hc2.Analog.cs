using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class HC2
    {
        /// <summary>
        /// Analog port connected to Humidity pin
        /// </summary>
        protected IAnalogInputPort? HumidityInputPort;

        /// <summary>
        /// Analog port connected to Temperature pin
        /// </summary>
        protected IAnalogInputPort? TemperatureInputPort;

        /// <summary>
        /// Creates a new HC2 Humidity Probe using analog inputs
        /// </summary>
        /// <param name="analogInputPortHumidity">The port for the humidity analog input pin</param>
        /// <param name="analogInputPortTemperature">The port for the temperature analog input pin</param>
        public HC2(IAnalogInputPort analogInputPortHumidity, IAnalogInputPort analogInputPortTemperature)
        {
            HumidityInputPort = analogInputPortHumidity;
            TemperatureInputPort = analogInputPortTemperature;

            communicationType = CommunicationType.Analog;

            InitSubscriptions();
        }

        /// <summary>
        /// Creates a new HC2 Humidity Probe using analog inputs
        /// </summary>
        /// <param name="analogInputPinHumidity">The pin for the humidity analog input</param>
        /// <param name="analogInputPinTemperature">The pin for the temperature analog input</param>
        /// <param name="sampleCount">The sample count for reading</param>
        /// <param name="sampleInterval">The sample interval</param>
        /// <param name="voltage">The reference voltage</param>
        public HC2(IPin analogInputPinHumidity,
            IPin analogInputPinTemperature,
            int sampleCount = 5,
            TimeSpan? sampleInterval = null,
            Voltage? voltage = null) :
            this(analogInputPinHumidity.CreateAnalogInputPort(sampleCount, sampleInterval ?? TimeSpan.FromMilliseconds(40), voltage ?? new Voltage(3.3)), analogInputPinTemperature.CreateAnalogInputPort(sampleCount, sampleInterval ?? TimeSpan.FromMilliseconds(40), voltage ?? new Voltage(3.3)))
        { }

        void InitSubscriptions()
        {
            _ = HumidityInputPort?.Subscribe
            (
                IAnalogInputPort.CreateObserver(
                    async result =>
                    {
                        var oldConditions = Conditions;

                        // Read a new Humidity, combine with previous Temperature (if any)
                        var newHumidity = await ReadHumidityAnalog();
                        var newTemperature = Conditions.Temperature;
                        var newConditions = (newHumidity, newTemperature);

                        // save state
                        Conditions = newConditions;

                        // create a new change result from the new value
                        ChangeResult<(Units.RelativeHumidity? Humidity, Units.Temperature? Temperature)> changeResult = new ChangeResult<(Units.RelativeHumidity? Humidity, Units.Temperature? Temperature)>()
                        {
                            New = newConditions,
                            Old = oldConditions,
                        };
                        // notify
                        RaiseEventsAndNotify(changeResult);
                    }
                )
            );
            _ = TemperatureInputPort?.Subscribe
            (
                IAnalogInputPort.CreateObserver(
                    async result =>
                    {
                        var oldConditions = Conditions;

                        // Read a new Temperature, combine with previous Humidity (if any)
                        var newHumidity = Conditions.Humidity;
                        var newTemperature = await ReadTemperatureAnalog();
                        var newConditions = (newHumidity, newTemperature);

                        // save state
                        Conditions = newConditions;

                        // create a new change result from the new value
                        ChangeResult<(Units.RelativeHumidity? Humidity, Units.Temperature? Temperature)> changeResult = new ChangeResult<(Units.RelativeHumidity? Humidity, Units.Temperature? Temperature)>()
                        {
                            New = newConditions,
                            Old = oldConditions,
                        };
                        // notify
                        RaiseEventsAndNotify(changeResult);
                    }
                )
            );
        }

        async Task<(Units.RelativeHumidity? Humidity, Units.Temperature? Temperature)> ReadSensorAnalog()
        {
            var humidity = await ReadHumidityAnalog();
            var temperature = await ReadTemperatureAnalog();
            (Units.RelativeHumidity? Humidity, Units.Temperature? Temperature) conditions = (humidity, temperature);
            return conditions;
        }

        async Task<Units.RelativeHumidity> ReadHumidityAnalog()
        {
            var volts = (await HumidityInputPort.Read()).Volts;
            var result = new Units.RelativeHumidity(volts * 100); // Assumes default scaling for HC2 probes.
            return result;
        }
        async Task<Units.Temperature> ReadTemperatureAnalog()
        {
            var volts = (await TemperatureInputPort.Read()).Volts;
            var result = new Units.Temperature((volts * 100) - 40, Units.Temperature.UnitType.Celsius); // Assumes default scaling for HC2 probes.
            return result;
        }

    }
}