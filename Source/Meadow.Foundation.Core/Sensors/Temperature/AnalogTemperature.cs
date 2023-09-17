using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Temperature
{
    /// <summary>
    /// Provide the ability to read the temperature from the following sensors:
    /// - TMP35 / 36 / 37
    /// - LM35 / 45
    /// </summary>
    /// <remarks>
    /// <i>AnalogTemperature</i> provides a method of reading the temperature from
    /// linear analog temperature sensors. There are a number of these sensors available
    /// including the commonly available TMP and LM series.
    /// Sensors of this type obey the following equation:
    /// y = mx + c
    /// where y is the reading in millivolts, m is the gradient (number of millivolts per
    /// degree centigrade and C is the point where the line would intercept the y axis.
    /// The <i>SensorType</i> enum defines the list of sensors with default settings in the
    /// library.  Unsupported sensors that use the same linear algorithm can be constructed
    /// by setting the sensor type to <i>SensorType.Custom</i> and providing the settings for
    /// the linear calculations.
    /// The default sensors have the following settings:
    /// Sensor              Millivolts at 25C    Millivolts per degree C  
    /// TMP35, LM35, LM45       250                     10                    
    /// TMP23x, TMP36, LM50     750                     10                   
    /// TMP37                   500                     20                
    /// TMP236                  887.5                   19.5                    
    /// </remarks>
    public partial class AnalogTemperature : SamplingSensorBase<Units.Temperature>, ITemperatureSensor
    {
        /// <summary>
        /// Raised when the value of the reading changes.
        /// </summary>
        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };

        ///<Summary>
        /// AnalogInputPort connected to temperature sensor
        ///</Summary>
        protected IAnalogInputPort AnalogInputPort { get; }

        /// <summary>
        /// Type of temperature sensor.
        /// </summary>
        public enum KnownSensorType
        {
            ///<Summary>
            /// Custom temperature sensor
            ///</Summary>
            Custom,
            ///<Summary>
            /// TMP235 temperature sensor
            ///</Summary>
            TMP235,
            ///<Summary>
            /// TMP236 temperature sensor
            ///</Summary>
            TMP236,
            ///<Summary>
            /// TMP35 temperature sensor
            ///</Summary>
            TMP35,
            ///<Summary>
            /// TMP36 temperature sensor
            ///</Summary>
            TMP36,
            ///<Summary>
            /// TMP37 temperature sensor
            ///</Summary>
            TMP37,
            ///<Summary>
            /// LM35 temperature sensor
            ///</Summary>
            LM35,
            ///<Summary>
            /// LM45 temperature sensor
            ///</Summary>
            LM45,
            ///<Summary>
            /// LM50 temperature sensor
            ///</Summary>
            LM50,
        }

        ///<Summary>
        /// SensorCalibration property for temperature sensor
        ///</Summary>
        public Calibration SensorCalibration { get; set; }

        /// <summary>
        /// Current Temperature
        /// </summary>
        public Units.Temperature? Temperature { get; protected set; }

        /// <summary>
        /// Creates a new instance of the AnalogTemperature class.
        /// </summary>
        /// <param name="analogPin">Analog pin the temperature sensor is connected to</param>
        /// <param name="sensorType">Type of sensor attached to the analog port</param>
        /// <param name="calibration">Calibration for the analog temperature sensor - used if sensorType is set to Custom</param>
        /// <param name="sampleCount">How many samples to take during a given
        /// reading. These are automatically averaged to reduce noise</param>
        /// <param name="sampleInterval">The time between sample readings</param>
        public AnalogTemperature(
            IPin analogPin,
            KnownSensorType sensorType,
            Calibration? calibration = null,
            int sampleCount = 5,
            TimeSpan? sampleInterval = null)
                : this(
                      analogPin.CreateAnalogInputPort(sampleCount, sampleInterval ?? TimeSpan.FromMilliseconds(40), new Voltage(3.3, Voltage.UnitType.Volts)),
                      sensorType, calibration)
        { }

        /// <summary>
        /// Creates a new instance of the AnalogTemperature class.
        /// </summary>
        /// <param name="analogInputPort">The `IAnalogInputPort` connected to the sensor.</param>
        /// <param name="sensorType">Type of sensor attached to the analog port.</param>
        /// <param name="calibration">Calibration for the analog temperature sensor. Only used if sensorType is set to Custom.</param>
        public AnalogTemperature(IAnalogInputPort analogInputPort,
                                 KnownSensorType sensorType,
                                 Calibration? calibration = null)
        {
            AnalogInputPort = analogInputPort;

            switch (sensorType)
            {
                case KnownSensorType.TMP35:
                case KnownSensorType.LM35:
                case KnownSensorType.LM45:
                    calibration = new Calibration(
                        degreesCelsiusSampleReading: 25,
                        millivoltsAtSampleReading: 250,
                        millivoltsPerDegreeCentigrade: 10);
                    break;
                case KnownSensorType.LM50:
                case KnownSensorType.TMP235:
                case KnownSensorType.TMP36:
                    calibration = new Calibration(
                        degreesCelsiusSampleReading: 25,
                        millivoltsAtSampleReading: 750,
                        millivoltsPerDegreeCentigrade: 10);
                    break;
                case KnownSensorType.TMP37:
                    calibration = new Calibration(
                        degreesCelsiusSampleReading: 25,
                        millivoltsAtSampleReading: 500,
                        millivoltsPerDegreeCentigrade: 20);
                    break;
                case KnownSensorType.TMP236:
                    calibration = new Calibration(
                        degreesCelsiusSampleReading: 25,
                        millivoltsAtSampleReading: 887.5,
                        millivoltsPerDegreeCentigrade: 19.5);
                    break;
                case KnownSensorType.Custom:
                    //user provided calibration
                    if (calibration == null)
                    {
                        throw new ArgumentNullException("Custom Calibration sensor type requires a Calibration parameter");
                    }
                    break;
                default:
                    calibration = new Calibration();
                    break;
            }

            SensorCalibration = calibration;

            // wire up our observable
            // have to convert from voltage to temp units for our consumers
            // this is where the magic is: this allows us to extend the IObservable
            // pattern through the sensor driver
            AnalogInputPort.Subscribe
            (
                IAnalogInputPort.CreateObserver(
                    result =>
                    {
                        // create a new change result from the new value
                        ChangeResult<Units.Temperature> changeResult = new ChangeResult<Units.Temperature>()
                        {
                            New = VoltageToTemperature(result.New),
                            Old = Temperature
                        };
                        Temperature = changeResult.New;
                        RaiseEventsAndNotify(changeResult);
                    }
                )
            );
        }

        /// <summary>
        /// Convenience method to get the current temperature. For frequent reads, use
        /// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        /// </summary>
        /// <returns>The temperature averages of the given sample count</returns>
        protected override async Task<Units.Temperature> ReadSensor()
        {
            var voltage = await AnalogInputPort.Read();
            var newTemp = VoltageToTemperature(voltage);
            Temperature = newTemp;
            return newTemp;
        }

        /// <summary>
        /// Starts continuously sampling the sensor.
        ///
        /// This method also starts raising `Changed` events and IObservable
        /// subscribers getting notified. Use the `readIntervalDuration` parameter
        /// to specify how often events and notifications are raised/sent.
        /// </summary>
        /// <param name="updateInterval">A `TimeSpan` that specifies how long to
        /// wait between readings. This value influences how often `*Updated`
        /// events are raised and `IObservable` consumers are notified.
        ///</param>
        public override void StartUpdating(TimeSpan? updateInterval)
        {
            lock (samplingLock)
            {
                if (IsSampling) { return; }
                IsSampling = true;
                AnalogInputPort.StartUpdating(updateInterval);
            }
        }

        /// <summary>
        /// Stops sampling the temperature
        /// </summary>
        public override void StopUpdating()
        {
            lock (samplingLock)
            {
                if (!IsSampling) { return; }
                IsSampling = false;
                AnalogInputPort.StopUpdating();
            }
        }

        /// <summary>
        /// Method to notify subscribers to TemperatureUpdated event handler
        /// </summary>
        /// <param name="changeResult"></param>
        protected override void RaiseEventsAndNotify(IChangeResult<Units.Temperature> changeResult)
        {
            TemperatureUpdated?.Invoke(this, changeResult);
            base.RaiseEventsAndNotify(changeResult);
        }

        /// <summary>
        /// Converts voltage to Temperature
        /// </summary>
        /// <param name="voltage"></param>
        /// <returns>Temperature</returns>
        protected Units.Temperature VoltageToTemperature(Voltage voltage)
        {
            return new Units.Temperature(SensorCalibration.SampleReading +
                (voltage.Millivolts - SensorCalibration.MillivoltsAtSampleReading) / SensorCalibration.MillivoltsPerDegreeCentigrade,
                Units.Temperature.UnitType.Celsius);
        }
    }
}