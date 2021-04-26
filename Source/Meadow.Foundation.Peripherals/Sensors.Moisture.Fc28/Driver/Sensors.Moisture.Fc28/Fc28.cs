using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Moisture;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Moisture
{
    /// <summary>
    /// FC-28-D Soil Hygrometer Detection Module + Soil Moisture Sensor    
    /// </summary>
    public class Fc28 : 
        FilterableChangeObservable<CompositeChangeResult<ScalarDouble>, ScalarDouble?>, 
        IMoistureSensor
    {
        /// <summary>
        /// Raised when a new sensor reading has been made. To enable, call StartUpdating().
        /// </summary>        
        public event EventHandler<CompositeChangeResult<ScalarDouble>> HumidityUpdated = delegate { };

        // internal thread lock
        private object _lock = new object();
        private CancellationTokenSource SamplingTokenSource;

        /// <summary>
        /// Gets a value indicating whether the sensor is currently in a sampling
        /// loop. Call StartSampling() to spin up the sampling process.
        /// </summary>
        /// <value><c>true</c> if sampling; otherwise, <c>false</c>.</value>
        public bool IsSampling { get; protected set; } = false;

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
        public ScalarDouble Moisture { get; private set; }

        /// <summary>
        /// Voltage value of most dry soil 
        /// </summary>
        public double MinimumVoltageCalibration { get; set; }

        /// <summary>
        /// Voltage value of most moist soil
        /// </summary>
        public double MaximumVoltageCalibration { get; set; }

        /// <summary>
        /// Creates a FC28 soil moisture sensor object with the especified analog pin, digital pin and IO device.
        /// </summary>
        /// <param name="analogPort"></param>
        /// <param name="digitalPort"></param>
        public Fc28(
            IMeadowDevice device,
            IPin analogPin,
            IPin digitalPin,
            double minimumVoltageCalibration = 0f,
            double maximumVoltageCalibration = 3.3f) :
            this(device.CreateAnalogInputPort(analogPin), device.CreateDigitalOutputPort(digitalPin), minimumVoltageCalibration, maximumVoltageCalibration)
        { }

        /// <summary>
        /// Creates a FC28 soil moisture sensor object with the especified analog pin and digital pin.
        /// </summary>
        /// <param name="analogPort"></param>
        /// <param name="digitalPort"></param>
        public Fc28(
            IAnalogInputPort analogPort,
            IDigitalOutputPort digitalPort,
            double minimumVoltageCalibration = 0f,
            double maximumVoltageCalibration = 3.3f)
        {
            AnalogInputPort = analogPort;
            DigitalPort = digitalPort;
            MinimumVoltageCalibration = minimumVoltageCalibration;
            MaximumVoltageCalibration = maximumVoltageCalibration;
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
            ScalarDouble previousMoisture = Moisture;

            DigitalPort.State = true;
            float voltage = await AnalogInputPort.Read(sampleCount, sampleInterval);
            DigitalPort.State = false;
            
            Moisture = VoltageToMoisture(voltage);
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
            // thread safety
            lock (_lock) {
                if (IsSampling)
                    return;
                IsSampling = true;

                SamplingTokenSource = new CancellationTokenSource();
                CancellationToken ct = SamplingTokenSource.Token;

                ScalarDouble oldConditions;
                CompositeChangeResult<ScalarDouble> result;
                Task.Factory.StartNew(async () => 
                {
                    while (true) {
                        // TODO: someone please review; is this the correct
                        // place to do this?
                        // check for cancel (doing this here instead of 
                        // while(!ct.IsCancellationRequested), so we can perform 
                        // cleanup
                        if (ct.IsCancellationRequested) {
                            // do task clean up here
                            observers.ForEach(x => x.OnCompleted());
                            break;
                        }
                        // capture history
                        oldConditions = Moisture;

                        // read                        
                        await Read(sampleCount, sampleIntervalDuration);

                        // build a new result with the old and new conditions
                        result = new CompositeChangeResult<ScalarDouble>(Moisture, oldConditions);

                        // let everyone know
                        RaiseChangedAndNotify(result);

                        // sleep for the appropriate interval
                        await Task.Delay(standbyDuration);
                    }
                }, SamplingTokenSource.Token);
            }
        }

        /// <summary>
        /// Stops sampling the sensor.
        /// </summary>
        public void StopUpdating()
        {
            lock (_lock) 
            {
                if (!IsSampling) { return; }

                if (SamplingTokenSource != null) 
                {
                    SamplingTokenSource.Cancel();
                }

                IsSampling = false;
            }
        }

        protected void RaiseChangedAndNotify(CompositeChangeResult<ScalarDouble> changeResult)
        {
            HumidityUpdated?.Invoke(this, changeResult);
            base.NotifyObservers(changeResult);
        }

        protected ScalarDouble VoltageToMoisture(double voltage)
        {
            if (MinimumVoltageCalibration > MaximumVoltageCalibration) 
            {
                return new ScalarDouble(1f - Map(voltage, MaximumVoltageCalibration, MinimumVoltageCalibration, 0d, 1.0d));
            }

            return new ScalarDouble(1f - Map(voltage, MinimumVoltageCalibration, MaximumVoltageCalibration, 0d, 1.0d));
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