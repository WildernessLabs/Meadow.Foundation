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
        FilterableChangeObservableBase<double>, 
        IMoistureSensor
    {
        /// <summary>
        /// Raised when a new sensor reading has been made. To enable, call StartUpdating().
        /// </summary>        
        public event EventHandler<IChangeResult<double>> HumidityUpdated = delegate { };

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
        public double? Moisture { get; private set; }

        /// <summary>
        /// Voltage value of most dry soil. Default of `0V`.
        /// </summary>
        public Voltage MinimumVoltageCalibration { get; set; } = new Voltage(0);

        /// <summary>
        /// Voltage value of most moist soil. Default of `3.3V`.
        /// </summary>
        public Voltage MaximumVoltageCalibration { get; set; } = new Voltage(3.3);

        /// <summary>
        /// Creates a FC28 soil moisture sensor object with the especified analog pin, digital pin and IO device.
        /// </summary>
        /// <param name="analogPort"></param>
        /// <param name="digitalPort"></param>
        public Fc28(
            IMeadowDevice device,
            IPin analogPin,
            IPin digitalPin,
            Voltage? minimumVoltageCalibration,
            Voltage? maximumVoltageCalibration) :
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
            Voltage? minimumVoltageCalibration,
            Voltage? maximumVoltageCalibration)
        {
            AnalogInputPort = analogPort;
            DigitalPort = digitalPort;
            if (minimumVoltageCalibration is { } min) { MinimumVoltageCalibration = min; }
            if (maximumVoltageCalibration is { } max) { MaximumVoltageCalibration = max; }
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
        public async Task<ChangeResult<double>> Read(int sampleCount = 10, int sampleInterval = 40)
        {
            double? oldMoisture = Moisture;

            DigitalPort.State = true;
            Voltage voltage = await AnalogInputPort.Read(sampleCount, sampleInterval);
            DigitalPort.State = false;

            var newMoisture = VoltageToMoisture(voltage);
            Moisture = newMoisture;
            return new ChangeResult<double>(newMoisture, oldMoisture);
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

                double? oldConditions;
                ChangeResult<double> result;
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
                        result = new ChangeResult<double>(Moisture.Value, oldConditions);

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

        protected void RaiseChangedAndNotify(IChangeResult<double> changeResult)
        {
            HumidityUpdated?.Invoke(this, changeResult);
            base.NotifyObservers(changeResult);
        }

        protected double VoltageToMoisture(Voltage voltage)
        {
            if (MinimumVoltageCalibration > MaximumVoltageCalibration) 
            {
                return (1f - voltage.Volts.Map(MaximumVoltageCalibration.Volts, MinimumVoltageCalibration.Volts, 0d, 1.0d));
            }

            return (1f - voltage.Volts.Map(MinimumVoltageCalibration.Volts, MaximumVoltageCalibration.Volts, 0d, 1.0d));
        }
    }
}