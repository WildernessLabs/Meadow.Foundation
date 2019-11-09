using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Moisture;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Moisture
{
    /// <summary>
    /// FC-28-D Soil Hygrometer Detection Module + Soil Moisture Sensor    
    /// </summary>
    public class FC28 : FilterableObservableBase<FloatChangeResult, float>, IMoistureSensor
    {
        /// <summary>
        /// Raised when a new sensor reading has been made. To enable, call StartUpdating().
        /// </summary>
        public event EventHandler<FloatChangeResult> Updated = delegate { };

        #region Properties

        /// <summary>
        /// Returns the analog input port
        /// </summary>
        public IAnalogInputPort AnalogInputPort { get; protected set; }

        /// <summary>
        /// Returns the digital output port
        /// </summary>
        public IDigitalOutputPort DigitalPort { get; protected set; }

        /// <summary>
        /// Last value read from the moisture sensor.
        /// </summary>
        public float Moisture { get; private set; }

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
        private FC28() { }

        /// <summary>
        /// Creates a FC28 soil moisture sensor object with the especified analog pin, digital pin and IO device.
        /// </summary>
        /// <param name="analogPort"></param>
        /// <param name="digitalPort"></param>
        public FC28(
            IIODevice device, 
            IPin analogPin, 
            IPin digitalPin, 
            float minimumVoltageCalibration = 0f, 
            float maximumVoltageCalibration = 3.3f) : 
            this (device.CreateAnalogInputPort(analogPin), device.CreateDigitalOutputPort(digitalPin), minimumVoltageCalibration, maximumVoltageCalibration) { }

        /// <summary>
        /// Creates a FC28 soil moisture sensor object with the especified analog pin and digital pin.
        /// </summary>
        /// <param name="analogPort"></param>
        /// <param name="digitalPort"></param>
        public FC28(
            IAnalogInputPort analogPort, 
            IDigitalOutputPort digitalPort, 
            float minimumVoltageCalibration = 0f, 
            float maximumVoltageCalibration = 3.3f)
        {
            AnalogInputPort  = analogPort;
            DigitalPort = digitalPort;
            MinimumVoltageCalibration = minimumVoltageCalibration;
            MaximumVoltageCalibration = maximumVoltageCalibration;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Convenience method to get the current soil moisture. For frequent reads, use
        /// StartUpdating() and StopUpdating().
        /// </summary>
        /// <param name="sampleCount">The number of sample readings to take. 
        /// Must be greater than 0.</param>
        /// <param name="sampleInterval">The interval, in milliseconds, between
        /// sample readings.</param>
        /// <returns></returns>
        public async Task<float> Read(int sampleCount = 10, int sampleInterval = 40)
        {
            DigitalPort.State = true;
            float voltage = await AnalogInputPort.Read(sampleCount, sampleInterval);
            DigitalPort.State = false;

            // convert and save to our temp property for later retrieval
            Moisture = VoltageToMoisture(voltage);
            return Moisture;
        }

        /// <summary>
        /// Starts continuously sampling the sensor.
        ///
        /// This method also starts raising `Updated` events and IObservable
        /// subscribers getting notified. Use the `standbyDuration` parameter
        /// to specify how often events and notifications are raised/sent.
        /// </summary>
        /// <param name="sampleCount">How many samples to take during a given
        /// reading. These are automatically averaged to reduce noise.</param>
        /// <param name="sampleIntervalDuration">The time, in milliseconds,
        /// to wait in between samples during a reading.</param>
        /// <param name="standbyDuration">The time, in milliseconds, to wait
        /// between sets of sample readings. This value determines how often
        /// `Updated` events are raised and `IObservable` consumers are notified.</param>
        public void StartUpdating(
            int sampleCount = 10,
            int sampleIntervalDuration = 40,
            int standbyDuration = 1000)
        {
            DigitalPort.State = true;
            AnalogInputPort.StartSampling(sampleCount, sampleIntervalDuration, standbyDuration);
        }

        /// <summary>
        /// Stops sampling the sensor.
        /// </summary>
        public void StopUpdating()
        {
            AnalogInputPort.StopSampling();
            DigitalPort.State = false;
        }

        protected void RaiseChangedAndNotify(FloatChangeResult changeResult)
        {
            Updated?.Invoke(this, changeResult);
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