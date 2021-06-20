using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using System;
using System.Threading.Tasks;
using Meadow.Units;

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
    /// Sensor              Millivolts at 25C    Millivolts per degree C   VoltageOffset
    /// TMP35, LM35, LM45       250                     10                      0
    /// TMP36, LM50             750                     10                      0.5
    /// TMP37                   500                     20                      0
    /// </remarks>
    public partial class AnalogTemperature : SensorBase<Units.Temperature>, ITemperatureSensor
    {
        /// <summary>
        /// Raised when the value of the reading changes.
        /// </summary>
        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };

        //==== internals
        protected IAnalogInputPort AnalogInputPort { get; }
        protected int sampleCount = 5;
        protected int sampleIntervalMs = 40;

        //==== properties
        /// <summary>
        ///     Type of temperature sensor.
        /// </summary>
        public enum KnownSensorType
        {
            Custom,
            TMP35,
            TMP36,
            TMP37,
            LM35,
            LM45,
            LM50
        }

        public Calibration SensorCalibration { get; set; }

        /// <summary>
        ///     Temperature in degrees centigrade.
        /// </summary>
        /// <remarks>
        ///     The temperature is given by the following calculation:
        ///     temperature = (reading in millivolts - yIntercept) / millivolts per degree centigrade
        /// </remarks>
        public Units.Temperature? Temperature { get; protected set; }

        /// <summary>
        /// Creates a new instance of the AnalogTemperature class.
        /// </summary>
        /// <param name="device">The `IAnalogInputController` to create the port on.</param>
        /// <param name="analogPin">Analog pin the temperature sensor is connected to.</param>
        /// <param name="sensorType">Type of sensor attached to the analog port.</param>
        /// <param name="calibration">Calibration for the analog temperature sensor. Only used if sensorType is set to Custom.</param>
        /// <param name="updateIntervalMs">The time, in milliseconds, to wait
        /// between sets of sample readings. This value determines how often
        /// `Changed` events are raised and `IObservable` consumers are notified.</param>
        /// <param name="sampleCount">How many samples to take during a given
        /// reading. These are automatically averaged to reduce noise.</param>
        /// <param name="sampleIntervalMs">The time, in milliseconds,
        /// to wait in between samples during a reading.</param>
        public AnalogTemperature(
            IAnalogInputController device, IPin analogPin,
            KnownSensorType sensorType, Calibration? calibration = null,
            int updateIntervalMs = 1000,
            int sampleCount = 5, int sampleIntervalMs = 40)
                : this(device.CreateAnalogInputPort(analogPin, updateIntervalMs, sampleCount, sampleIntervalMs),
                      sensorType, calibration)
        {
            base.UpdateInterval = TimeSpan.FromMilliseconds(updateIntervalMs);
            this.sampleCount = sampleCount;
            this.sampleIntervalMs = sampleIntervalMs;
        }

        public AnalogTemperature(IAnalogInputPort analogInputPort,
                                 KnownSensorType sensorType,
                                 Calibration? calibration = null)
        {
            AnalogInputPort = analogInputPort;

            switch (sensorType) {
                case KnownSensorType.TMP35:
                case KnownSensorType.LM35:
                case KnownSensorType.LM45:
                    calibration = new Calibration(
                        degreesCelciusSampleReading: 25,
                        millivoltsAtSampleReading: 250,
                        millivoltsPerDegreeCentigrade: 10);
                    break;
                case KnownSensorType.LM50:
                case KnownSensorType.TMP36:
                    calibration = new Calibration(
                        degreesCelciusSampleReading: 25,
                        millivoltsAtSampleReading: 750,
                        millivoltsPerDegreeCentigrade: 10);
                    break;
                case KnownSensorType.TMP37:
                    calibration = new Calibration(
                        degreesCelciusSampleReading: 25,
                        millivoltsAtSampleReading: 750,
                        millivoltsPerDegreeCentigrade: 10);
                    break;
                case KnownSensorType.Custom:
                    //user provided calibration
                    if(calibration == null)
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
                    result => {
                        // create a new change result from the new value
                        ChangeResult<Units.Temperature> changeResult = new ChangeResult<Units.Temperature>() {
                            New = VoltageToTemperature(result.New),
                            Old = Temperature
                        };
                        // save state
                        Temperature = changeResult.New;
                        // notify
                        RaiseEventsAndNotify(changeResult);
                    }
                )
           );
        }

        /// <summary>
        /// Convenience method to get the current temperature. For frequent reads, use
        /// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        /// </summary>
        /// <returns>A float value that's ann average value of all the samples taken.</returns>
        protected override async Task<Units.Temperature> ReadSensor()
        {
            // read the voltage
            Voltage voltage = await AnalogInputPort.Read();

            // convert the voltage
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
        public void StartUpdating()
        {
            // thread safety
            lock (samplingLock) {
                if (IsSampling) return;
                IsSampling = true;
                AnalogInputPort.StartUpdating();
            }
        }

        /// <summary>
        /// Stops sampling the temperature.
        /// </summary>
        public void StopUpdating()
        {
            lock (samplingLock) {
                if (!IsSampling) return;
                base.IsSampling = false;
                AnalogInputPort.StopUpdating();
            }
        }

        protected void RaiseEventsAndNotify(ChangeResult<Units.Temperature> changeResult)
        {
            TemperatureUpdated?.Invoke(this, changeResult);
            base.NotifyObservers(changeResult);
        }

        /// <summary>
        /// Converts a voltage value to a celsius temp, based on the current
        /// calibration values.
        /// </summary>
        /// <param name="voltage"></param>
        /// <returns>temperature in celcius</returns>
        protected Units.Temperature VoltageToTemperature(Voltage voltage)
        {
            return new Units.Temperature(
                SensorCalibration.SampleReading
                +
                (voltage.Millivolts - SensorCalibration.MillivoltsAtSampleReading)
                /
                SensorCalibration.MillivoltsPerDegreeCentigrade,
                Units.Temperature.UnitType.Celsius);
        }
    }
}