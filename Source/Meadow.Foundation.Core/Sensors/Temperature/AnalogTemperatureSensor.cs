using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Peripherals.Sensors.Temperature;
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
    /// Sensor              Millivolts at 25C    Millivolts per degree C   VoltageOffset
    /// TMP35, LM35, LM45       250                     10                      0
    /// TMP36, LM50             750                     10                      0.5
    /// TMP37                   500                     20                      0
    /// </remarks>
    public class AnalogTemperature
        : FilterableChangeObservableBase<AtmosphericConditionChangeResult, AtmosphericConditions>,
        ITemperatureSensor
    {
        /// <summary>
        /// Raised when the value of the reading changes.
        /// </summary>
        public event EventHandler<AtmosphericConditionChangeResult> Updated = delegate { };

        /// <summary>
        ///     Calibration class for new sensor types.  This allows new sensors
        ///     to be used with this class.
        /// </summary>
        /// <remarks>
        ///     The default settings for this object are correct for the TMP35.
        /// </remarks>
        public class Calibration
        {
            /// <summary>
            ///     Sample reading as specified in the product data sheet.
            ///     Measured in degrees Centigrade.
            /// </summary>
            public int SampleReading { get; protected set; } = 25;

            /// <summary>
            ///     Millivolt reading the sensor will generate when the sensor
            ///     is at the Samplereading temperature.  This value can be
            ///     obtained from the data sheet. 
            /// </summary>
            public int MillivoltsAtSampleReading { get; protected set; } = 250;

            /// <summary>
            ///     Linear change in the sensor output (in millivolts) per 1 degree C
            ///     change in temperature.
            /// </summary>
            public int MillivoltsPerDegreeCentigrade { get; protected set; } = 10;

            /// <summary>
            ///     Default constructor.  Create a new Calibration object with default values
            ///     for the properties.
            /// </summary>
            public Calibration()
            {
            }

            /// <summary>
            ///     Create a new Calibration object using the specified values.
            /// </summary>
            /// <param name="degreesCelciusSampleReading">Sample reading from the data sheet.</param>
            /// <param name="millivoltsAtSampleReading">Millivolts output at the sample reading (from the data sheet).</param>
            /// <param name="millivoltsPerDegreeCentigrade">Millivolt change per degree centigrade (from the data sheet).</param>
            /// <param name="millivoltsOffset">Millovolts offset (from the data sheet).</param>
            public Calibration(int degreesCelciusSampleReading,
                               int millivoltsAtSampleReading,
                               int millivoltsPerDegreeCentigrade)
            {
                SampleReading = degreesCelciusSampleReading;
                MillivoltsAtSampleReading = millivoltsAtSampleReading;
                MillivoltsPerDegreeCentigrade = millivoltsPerDegreeCentigrade;
            }
        }

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

        public IAnalogInputPort AnalogInputPort { get; protected set; }

        /// <summary>
        ///     Temperature in degrees centigrade.
        /// </summary>
        /// <remarks>
        ///     The temperature is given by the following calculation:
        ///     temperature = (reading in millivolts - yIntercept) / millivolts per degree centigrade
        /// </remarks>
        public float Temperature { get; protected set; }

        float ITemperatureSensor.Temperature => Temperature;

        /// <summary>
        ///     Default constructor, private to prevent this being used.
        /// </summary>
        private AnalogTemperature()
        {
        }

        /// <summary>
        ///     New instance of the AnalogTemperatureSensor class.
        /// </summary>
        /// <param name="analogPin">Analog pin the temperature sensor is connected to.</param>
        /// <param name="sensorType">Type of sensor attached to the analog port.</param>
        /// <param name="calibration">Calibration for the analog temperature sensor. Only used if sensorType is set to Custom.</param>
        public AnalogTemperature(
            IIODevice device,
            IPin analogPin,
            KnownSensorType sensorType,
            Calibration calibration = null
            ) : this(device.CreateAnalogInputPort(analogPin), sensorType, calibration)
        {
        }

        public AnalogTemperature(IAnalogInputPort analogInputPort,
                                 KnownSensorType sensorType,
                                 Calibration calibration = null)
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
                new FilterableChangeObserver<FloatChangeResult, float>(
                    h => {
                        var newTemp = VoltageToTemperature(h.New);
                        var oldTemp = VoltageToTemperature(h.Old);
                        Temperature = newTemp; // save state
                        RaiseEventsAndNotify
                        (
                            new AtmosphericConditionChangeResult(
                                new AtmosphericConditions(newTemp, null, null),
                                new AtmosphericConditions(oldTemp, null, null)
                            )
                        );
                    }
                )
           );
        }

        /// <summary>
        /// Convenience method to get the current temperature. For frequent reads, use
        /// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        /// </summary>
        /// <param name="sampleCount">The number of sample readings to take. 
        /// Must be greater than 0. These samples are automatically averaged.</param>
        /// <param name="sampleIntervalDuration">The time, in milliseconds,
        /// to wait in between samples during a reading.</param>
        /// <returns>A float value that's ann average value of all the samples taken.</returns>
        public async Task<AtmosphericConditions> Read(int sampleCount = 10, int sampleIntervalDuration = 40)
        {
            // read the voltage
            float voltage = await AnalogInputPort.Read(sampleCount, sampleIntervalDuration);
            // convert and save to our temp property for later retreival
            Temperature = VoltageToTemperature(voltage);
            // return
            return new AtmosphericConditions(Temperature, null, null);
            //return Temperature;
        }

        /// <summary>
        /// Starts continuously sampling the sensor.
        ///
        /// This method also starts raising `Changed` events and IObservable
        /// subscribers getting notified. Use the `readIntervalDuration` parameter
        /// to specify how often events and notifications are raised/sent.
        /// </summary>
        /// <param name="sampleCount">How many samples to take during a given
        /// reading. These are automatically averaged to reduce noise.</param>
        /// <param name="sampleIntervalDuration">The time, in milliseconds,
        /// to wait in between samples during a reading.</param>
        /// <param name="standbyDuration">The time, in milliseconds, to wait
        /// between sets of sample readings. This value determines how often
        /// `Changed` events are raised and `IObservable` consumers are notified.</param>
        public void StartUpdating(
            int sampleCount = 10,
            int sampleIntervalDuration = 40,
            int standbyDuration = 100)
        {
            AnalogInputPort.StartSampling(sampleCount, sampleIntervalDuration, standbyDuration);
        }

        /// <summary>
        /// Stops sampling the temperature.
        /// </summary>
        public void StopUpdating()
        {
            AnalogInputPort.StopSampling();
        }

        protected void RaiseEventsAndNotify(AtmosphericConditionChangeResult changeResult)
        {
            Updated?.Invoke(this, changeResult);
            base.NotifyObservers(changeResult);
        }

        /// <summary>
        /// Converts a voltage value to a celsius temp, based on the current
        /// calibration values.
        /// </summary>
        /// <param name="voltage"></param>
        /// <returns>temperature in celcius</returns>
        protected float VoltageToTemperature(float voltage)
        {
            return SensorCalibration.SampleReading + (SensorCalibration.MillivoltsAtSampleReading - voltage * 1000) / SensorCalibration.MillivoltsPerDegreeCentigrade;
        }
    }
}