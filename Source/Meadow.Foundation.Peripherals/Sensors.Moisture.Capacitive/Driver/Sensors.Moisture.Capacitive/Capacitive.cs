using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Moisture;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Moisture
{
    /// <summary>
    /// Capacitive Soil Moisture Sensor
    /// </summary>
    public class Capacitive : //FilterableChangeObservableBase<FloatChangeResult, float>, IMoistureSensor
        FilterableChangeObservable<CompositeChangeResult<ScalarDouble>, ScalarDouble?>,
        IMoistureSensor
    {
        /// <summary>
        /// Raised when a new sensor reading has been made. To enable, call StartUpdating().
        /// </summary>        
        public event EventHandler<CompositeChangeResult<ScalarDouble>> HumidityUpdated = delegate { };

        // internal thread lock
        object _lock = new object();

        /// <summary>
        /// Returns the analog input port
        /// </summary>
        public IAnalogInputPort AnalogInputPort { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether the analog input port is currently
        /// sampling the ADC. Call StartSampling() to spin up the sampling process.
        /// </summary>
        /// <value><c>true</c> if sampling; otherwise, <c>false</c>.</value>
        public bool IsSampling { get; protected set; } = false;

        /// <summary>
        /// Last value read from the moisture sensor.
        /// </summary>
        public ScalarDouble Moisture { get; protected set; }

        /// <summary>
        /// Voltage value of most dry soil. Default of `0V`.
        /// </summary>
        public Voltage MinimumVoltageCalibration { get; set; } = new Voltage(0);

        /// <summary>
        /// Voltage value of most moist soil. Default of `3.3V`.
        /// </summary>
        public Voltage MaximumVoltageCalibration { get; set; } = new Voltage(3.3);

        /// <summary>
        /// Creates a Capacitive soil moisture sensor object with the specified analog pin and a IO device.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="analogPin"></param>
        public Capacitive(
            IAnalogInputController device,
            IPin analogPin,
            Voltage? minimumVoltageCalibration,
            Voltage? maximumVoltageCalibration) :
            this(device.CreateAnalogInputPort(analogPin), minimumVoltageCalibration, maximumVoltageCalibration)
        { }

        /// <summary>
        /// Creates a Capacitive soil moisture sensor object with the especified AnalogInputPort.
        /// </summary>
        /// <param name="analogPort"></param>
        public Capacitive(
            IAnalogInputPort analogPort,
            Voltage? minimumVoltageCalibration,
            Voltage? maximumVoltageCalibration)
        {
            AnalogInputPort = analogPort;

            if(minimumVoltageCalibration is { } min) { MinimumVoltageCalibration = min; }
            if(maximumVoltageCalibration is { } max) { MaximumVoltageCalibration = max; }
            //MinimumVoltageCalibration = minimumVoltageCalibration;
            //MaximumVoltageCalibration = maximumVoltageCalibration;

            // wire up our observable
            // have to convert from voltage to temp units for our consumers
            // this is where the magic is: this allows us to extend the IObservable
            // pattern through the sensor driver
            AnalogInputPort.Subscribe
            (
                IAnalogInputPort.CreateObserver(
                    h => {
                        var newMoisture = VoltageToMoisture(h.New);
                        var oldMoisture = VoltageToMoisture(h.Old);
                        Moisture = newMoisture;
                        RaiseChangedAndNotify(
                            new CompositeChangeResult<ScalarDouble>(newMoisture, oldMoisture)
                        );
                    }
                )
           );
        }

        /// <summary>
        /// Convenience method to get the current soil moisture. For frequent reads, use
        /// StartUpdating() and StopUpdating().
        /// </summary>
        /// <param name="sampleCount">The number of sample readings to take. 
        /// Must be greater than 0.</param>
        /// <param name="sampleInterval">The interval, in milliseconds, between
        /// sample readings.</param>
        /// <returns></returns>
        public async Task<CompositeChangeResult<ScalarDouble>> Read(int sampleCount = 10, int sampleInterval = 40)
        {
            // save previous moisture value
            ScalarDouble previousMoisture = Moisture;
            // read the voltage
            Voltage voltage = await AnalogInputPort.Read(sampleCount, sampleInterval);
            // convert and save to our temp property for later retrieval
            Moisture = VoltageToMoisture(voltage);
            // return new and old Moisture values
            return new CompositeChangeResult<ScalarDouble>(Moisture, previousMoisture);
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
            AnalogInputPort.StartSampling(sampleCount, sampleIntervalDuration, standbyDuration);
        }

        /// <summary>
        /// Stops sampling the sensor.
        /// </summary>
        public void StopUpdating()
        {
            AnalogInputPort.StopSampling();
        }

        protected void RaiseChangedAndNotify(CompositeChangeResult<ScalarDouble> changeResult)
        {
            HumidityUpdated?.Invoke(this, changeResult);
            base.NotifyObservers(changeResult);
        }

        protected ScalarDouble VoltageToMoisture(Voltage voltage)
        {
            if (MinimumVoltageCalibration > MaximumVoltageCalibration) {
                return new ScalarDouble(1f - Map(voltage.Volts, MaximumVoltageCalibration.Volts, MinimumVoltageCalibration.Volts, 0f, 1.0f));
            }

            return new ScalarDouble(1f - Map(voltage.Volts, MinimumVoltageCalibration.Volts, MaximumVoltageCalibration.Volts, 0f, 1.0f));
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
        protected double Map(double value, double fromLow, double fromHigh, double toLow, double toHigh)
        {
            return (((toHigh - toLow) * (value - fromLow)) / (fromHigh - fromLow)) - toLow;
        }
    }
}