using Meadow.Hardware;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Environmental
{
    public class AnalogWaterLevel
        : FilterableChangeObservableBase<FloatChangeResult, float>
    {
        /// <summary>
        /// Raised when the value of the reading changes.
        /// </summary>
        public event EventHandler<FloatChangeResult> Updated = delegate { };

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
            ///     Linear change in the sensor output (in millivolts) per 1 mm
            ///     change in temperature.
            /// </summary>
            public int MillivoltsPerMillimeter { get; protected set; } = 1;

            /// <summary>
            ///     Default constructor. Create a new Calibration object with default values
            ///     for the properties.
            /// </summary>
            public Calibration()
            {
            }

            /// <summary>
            ///     Create a new Calibration object using the specified values.
            /// </summary>
            /// <param name="sampleReading">Sample reading from the data sheet.</param>
            /// <param name="millivoltsAtSampleReading">Millivolts output at the sample reading (from the data sheet).</param>
            /// <param name="millivoltsPerMillimeter">Millivolt change per degree centigrade (from the data sheet).</param>
            public Calibration(int millivoltsPerMillimeter)
            {
                MillivoltsPerMillimeter = millivoltsPerMillimeter;
            }
        }

        public IAnalogInputPort AnalogInputPort { get; protected set; }

        /// <summary>
        ///     Millivolts per degree centigrade for the sensor attached to the analog port.
        /// </summary>
        /// <remarks>
        ///     This will be the gradient of the line y = mx + c.
        /// </remarks>
        private readonly int millivoltsPerCentimeter;

        /// <summary>
        ///     Analog port that the temperature sensor is attached to.
        /// </summary>
        /// <value>Analog port connected to the temperature sensor.</value>
        private IAnalogInputPort AnalogPort { get; set; }

        public float WaterLevel { get; protected set; }

        /// <summary>
        ///     Default constructor, private to prevent this being used.
        /// </summary>
        private AnalogWaterLevel()
        {
        }

        /// <summary>
        ///     New instance of the AnalogWaterLevel class.
        /// </summary>
        /// <param name="analogPin">Analog pin the temperature sensor is connected to.</param>
        /// <param name="sensorType">Type of sensor attached to the analog port.</param>
        /// <param name="calibration">Calibration for the analog temperature sensor. Only used if sensorType is set to Custom.</param>
        public AnalogWaterLevel(
            IIODevice device,
            IPin analogPin,
            Calibration calibration = null
            ) : this(device.CreateAnalogInputPort(analogPin), calibration)
        {
        }

        public AnalogWaterLevel(IAnalogInputPort analogInputPort,
                                 Calibration calibration = null)
        {
            AnalogInputPort = analogInputPort;

            //
            //  If the calibration object is null use the defaults for TMP35.
            //
            if (calibration == null) { calibration = new Calibration(); }

            // wire up our observable
            // have to convert from voltage to temp units for our consumers
            // this is where the magic is: this allows us to extend the IObservable
            // pattern through the sensor driver
            AnalogInputPort.Subscribe
            (
                new FilterableChangeObserver<FloatChangeResult, float>(
                    h => {
                        var newWaterLevel = VoltageToWaterLevel(h.New);
                        var oldTemp = VoltageToWaterLevel(h.Old);
                        WaterLevel = newWaterLevel; // save state

                        RaiseEventsAndNotify
                        (
                            new FloatChangeResult(newWaterLevel,oldTemp)
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
        public async Task<float> Read(int sampleCount = 10, int sampleIntervalDuration = 40)
        {
            // read the voltage
            float voltage = await AnalogInputPort.Read(sampleCount, sampleIntervalDuration);
            // convert and save to our temp property for later retreival
            WaterLevel = VoltageToWaterLevel(voltage);
            // return
            return WaterLevel;
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

        protected void RaiseEventsAndNotify(FloatChangeResult changeResult)
        {
            Updated?.Invoke(this, changeResult);
            base.NotifyObservers(changeResult);
        }

        /// <summary>
        /// Converts a voltage value to a level in centimeters, based on the current
        /// calibration values.
        /// </summary>
        /// <param name="voltage"></param>
        /// <returns></returns>
        protected float VoltageToWaterLevel(float voltage)
        {
            return voltage * millivoltsPerCentimeter;
        }
    }
}