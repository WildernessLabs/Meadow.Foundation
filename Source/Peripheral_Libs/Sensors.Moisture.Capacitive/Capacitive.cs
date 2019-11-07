using System;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Moisture;
//using Meadow.Foundation.Sensors;

namespace Meadow.Foundation.Sensors.Moisture
{
    /// <summary>
    /// Capacitive Soil Moisture Sensor
    /// </summary>
    public class Capacitive : FilterableObservableBase<FloatChangeResult, float>, IMoistureSensor
    {
        /// <summary>
        /// Raised when the value of the reading changes.
        /// </summary>
        public event EventHandler<FloatChangeResult> Changed = delegate { };

        #region Properties

        /// <summary>
        /// Returns the analog input port
        /// </summary>
        public IAnalogInputPort AnalogInputPort { get; protected set; }

        /// <summary>
        /// Last value read from the moisture sensor.
        /// </summary>
        public float Moisture { get; protected set; }

        /// <summary>
        /// Voltage value of most dry soil 
        /// </summary>
        public float MinimumVoltageCalibration { get; set; }

        /// <summary>
        /// Voltage value of most moist soil
        /// </summary>
        public float MaximumVoltageCalibration { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor is private to prevent it being called.
        /// </summary>
        private Capacitive() { }

        /// <summary>
        /// Creates a Capacitive soil moisture sensor object with the specified analog pin and a IO device.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="analogPin"></param>
        public Capacitive(IIODevice device, IPin analogPin, float minimumVoltageCalibration = 0f, float maximumVoltageCalibration = 3.3f)
            : this(device.CreateAnalogInputPort(analogPin), minimumVoltageCalibration, maximumVoltageCalibration) {
        }

        /// <summary>
        /// Creates a Capacitive soil moisture sensor object with the especified AnalogInputPort.
        /// </summary>
        /// <param name="analogPort"></param>
        public Capacitive(IAnalogInputPort analogPort, float minimumVoltageCalibration = 0f, float maximumVoltageCalibration = 3.3f)
        {
            AnalogInputPort = analogPort;
            MinimumVoltageCalibration = minimumVoltageCalibration;
            MaximumVoltageCalibration = maximumVoltageCalibration;

            // wire up our observable
            // have to convert from voltage to temp units for our consumers
            // this is where the magic is: this allows us to extend the IObservable
            // pattern through the sensor driver
            AnalogInputPort.Subscribe(
                new FilterableObserver<FloatChangeResult, float>(
                    h => {
                        var newMoisture = VoltageToMoisture(h.New);
                        var oldMoisture = VoltageToMoisture(h.Old);
                        Moisture = newMoisture; // save state
                        RaiseChangedAndNotify(new FloatChangeResult(
                            newMoisture,
                            oldMoisture));
                    })
                );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Convenience method to get the current soil moisture. For frequent reads, use
        /// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        /// </summary>
        /// <param name="sampleCount">The number of sample readings to take. 
        /// must be greater than 0.</param>
        /// <param name="sampleInterval">The interval, in milliseconds, between
        /// sample readings.</param>
        /// <returns></returns>
        public async Task<float> Read(int sampleCount = 10, int sampleInterval = 40)
        {
            // read the voltage
            float voltage = await AnalogInputPort.Read(sampleCount, sampleInterval);
            // convert and save to our temp property for later retrieval
            Moisture = VoltageToMoisture(voltage);
            // return
            return Moisture;
        }

        /// <summary>
        /// Starts continuously sampling the temperature. Also triggers the
        /// events to fire, and IObservable subscribers to get notified.
        /// </summary>
        /// <param name="sampleCount"></param>
        /// <param name="sampleIntervalDuration"></param>
        /// <param name="sampleSleepDuration"></param>
        public void StartUpdating(
            int sampleCount = 10,
            int sampleIntervalDuration = 40,
            int sampleSleepDuration = 0)
        {
            AnalogInputPort.StartSampling(sampleCount, sampleIntervalDuration, sampleSleepDuration);
        }

        /// <summary>
        /// Stops sampling the temperature.
        /// </summary>
        public void StopUpdating()
        {
            AnalogInputPort.StopSampling();
        }

        protected void RaiseChangedAndNotify(FloatChangeResult changeResult)
        {
            Changed?.Invoke(this, changeResult);
            base.NotifyObservers(changeResult);
        }

        protected float VoltageToMoisture(float voltage) 
        {
            if (MinimumVoltageCalibration > MaximumVoltageCalibration)
            {
                return 1f - Map(voltage, MaximumVoltageCalibration, MinimumVoltageCalibration, 0f, 1.0f);
            }

            return 1f - Map(voltage, MinimumVoltageCalibration, MaximumVoltageCalibration, 0f, 1.0f);
        }

        /// <summary>
        /// Re-maps a value from one range (fromLow - fromHigh) to another (toLow - toHigh).
        /// </summary>
        /// <param name="value"></param>
        /// <param name="fromLow"></param>
        /// <param name="fromHigh"></param>
        /// <param name="toLow"></param>
        /// <param name="toHigh"></param>
        /// <returns></returns>
        protected float Map(float value, float fromLow, float fromHigh, float toLow, float toHigh)
        {
            return (((toHigh - toLow) * (value - fromLow)) / (fromHigh - fromLow)) - toLow;
        }

        #endregion
    }
}