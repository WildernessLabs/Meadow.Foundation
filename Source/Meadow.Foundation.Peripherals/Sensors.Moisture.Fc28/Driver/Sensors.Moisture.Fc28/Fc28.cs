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
    public class Fc28 : SensorBase<double>, IMoistureSensor
    {
        /// <summary>
        /// Raised when a new sensor reading has been made. To enable, call StartUpdating().
        /// </summary>        
        public event EventHandler<IChangeResult<double>> HumidityUpdated = delegate { };

        /// <summary>
        /// Returns the analog input port
        /// </summary>
        protected IAnalogInputPort AnalogInputPort { get; }

        /// <summary>
        /// Returns the digital output port
        /// </summary>
        protected IDigitalOutputPort DigitalPort { get; }

        /// <summary>
        /// Last value read from the moisture sensor.
        /// </summary>
        public double? Moisture { get; private set; } = double.NaN;

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
            IMeadowDevice device, IPin analogPin, IPin digitalPin,
            Voltage? minimumVoltageCalibration, Voltage? maximumVoltageCalibration,
            int updateIntervalMs = 1000,
            int sampleCount = 5, int sampleIntervalMs = 40)
                : this(device.CreateAnalogInputPort(analogPin, updateIntervalMs, sampleCount, sampleIntervalMs),
                      device.CreateDigitalOutputPort(digitalPin), minimumVoltageCalibration, maximumVoltageCalibration)
        { }

        /// <summary>
        /// Creates a FC28 soil moisture sensor object with the especified analog pin and digital pin.
        /// </summary>
        /// <param name="analogPort"></param>
        /// <param name="digitalPort"></param>
        public Fc28(
            IAnalogInputPort analogPort, IDigitalOutputPort digitalPort,
            Voltage? minimumVoltageCalibration, Voltage? maximumVoltageCalibration)
        {
            AnalogInputPort = analogPort;
            DigitalPort = digitalPort;
            if (minimumVoltageCalibration is { } min) { MinimumVoltageCalibration = min; }
            if (maximumVoltageCalibration is { } max) { MaximumVoltageCalibration = max; }
        }

        protected override async Task<double> ReadSensor()
        {
            DigitalPort.State = true;
            Voltage voltage = await AnalogInputPort.Read();
            DigitalPort.State = false;
            return(VoltageToMoisture(voltage));
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
        public void StartUpdating()
        {
            // thread safety
            lock (samplingLock) {
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
                        if (ct.IsCancellationRequested) {
                            // do task clean up here
                            observers.ForEach(x => x.OnCompleted());
                            break;
                        }
                        // capture history
                        oldConditions = Moisture;

                        // read                        
                        await Read();

                        // build a new result with the old and new conditions
                        result = new ChangeResult<double>(Moisture.Value, oldConditions);

                        // let everyone know
                        RaiseChangedAndNotify(result);

                        // sleep for the appropriate interval
                        await Task.Delay(base.UpdateInterval);
                    }
                }, SamplingTokenSource.Token);
            }
        }

        /// <summary>
        /// Stops sampling the sensor.
        /// </summary>
        public void StopUpdating()
        {
            lock (samplingLock) 
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