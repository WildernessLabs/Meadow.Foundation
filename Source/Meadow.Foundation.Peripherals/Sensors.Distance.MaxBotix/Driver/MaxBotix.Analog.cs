using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Distance
{
    public partial class MaxBotix
    {
        readonly IAnalogInputPort analogInputPort;

        /// <summary>
        /// Creates a new MaxBotix object communicating over analog
        /// </summary>
        /// <param name="analogInputPort">The port for the analog input pin</param>
        /// <param name="sensor">The distance sensor type</param>
        public MaxBotix(IAnalogInputPort analogInputPort, SensorType sensor)
        {
            analogInputPort = analogInputPort;

            communication = CommunicationType.Analog;
            sensorType = sensor;

            AnalogInitialize();
        }

        /// <summary>
        /// Creates a new MaxBotix object communicating over analog
        /// </summary>
        /// <param name="sensor">The distance sensor type</param>
        /// <param name="analogInputPin">The analog input pin</param>
        /// <param name="sampleCount">The sample count for reading</param>
        /// <param name="sampleInterval">The sample interval</param>
        /// <param name="voltage">The reference voltage</param>
        public MaxBotix(SensorType sensor,
            IPin analogInputPin,
            int sampleCount = 5,
            TimeSpan? sampleInterval = null,
            Voltage? voltage = null) :
            this(analogInputPin.CreateAnalogInputPort(sampleCount, sampleInterval ?? TimeSpan.FromMilliseconds(40), voltage ?? new Voltage(3.3)), sensor)
        { }

        void AnalogInitialize()
        {
            // wire up our observable
            // have to convert from voltage to length units for our consumers
            // this is where the magic is: this allows us to extend the IObservable
            // pattern through the sensor driver
            analogInputPort.Subscribe
            (
                IAnalogInputPort.CreateObserver(
                    async result =>
                    {
                        // create a new change result from the new value
                        ChangeResult<Length> changeResult = new ChangeResult<Length>()
                        {
                            New = await ReadSensorAnalog(),
                            Old = Distance,
                        };
                        // save state
                        Distance = changeResult.New;
                        // notify
                        RaiseEventsAndNotify(changeResult);
                    }
                )
           );
        }


        async Task<Length> ReadSensorAnalog()
        {
            var volts = (await analogInputPort.Read()).Volts;
            Length.UnitType unit = GetUnitsForSensor(sensorType);

            return sensorType switch
            {
                SensorType.LV => new Length(volts * 512.0 / VCC, unit), //inches
                SensorType.XL => new Length(volts * 1024.0 / VCC, unit), //cm
                SensorType.XLLongRange => new Length(volts * 512.0 / VCC, unit), //cm
                SensorType.HR5Meter => new Length(volts * 5120.0 / VCC, unit), //mm
                SensorType.HR10Meter => new Length(volts * 10240.0 / VCC, unit), //mm
                _ => new Length(volts * 5120.0 / VCC, Units.Length.UnitType.Millimeters) // most common for unknown
            };
        }
    }
}