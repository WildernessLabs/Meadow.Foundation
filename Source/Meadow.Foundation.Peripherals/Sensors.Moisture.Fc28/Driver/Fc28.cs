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
    public class Fc28 : SamplingSensorBase<double>, IMoistureSensor
    {
        /// <summary>
        /// Raised when a new sensor reading has been made. To enable, call StartUpdating().
        /// </summary>
        public event EventHandler<IChangeResult<double>> MoistureUpdated = default!;

        /// <summary>
        /// Returns the analog input port
        /// </summary>
        protected IAnalogInputPort AnalogInputPort { get; }

        /// <summary>
        /// Returns the digital output port
        /// </summary>
        protected IDigitalOutputPort DigitalOutputPort { get; }

        /// <summary>
        /// Last value read from the moisture sensor
        /// </summary>
        public double? Moisture { get; private set; } = double.NaN;

        /// <summary>
        /// Voltage value of most dry soil. Default of `0V`
        /// </summary>
        public Voltage MinimumVoltageCalibration { get; set; } = new Voltage(0);

        /// <summary>
        /// Voltage value of most moist soil. Default of `3.3V`
        /// </summary>
        public Voltage MaximumVoltageCalibration { get; set; } = new Voltage(3.3);

        /// <summary>
        /// Creates a FC28 soil moisture sensor object with the specified analog pin, digital pin and IO device
        /// </summary>
        /// <param name="analogInputPin">Analog input pin connected</param>
        /// <param name="digitalOutputPin">Digital output pin connected</param>
        /// <param name="minimumVoltageCalibration">Minimum Voltage Calibration value</param>
        /// <param name="maximumVoltageCalibration">Maximum Voltage Calibration value</param>
        /// <param name="updateInterval">The time, to wait between sets of sample readings. 
        /// This value determines how often`Changed` events are raised and `IObservable` consumers are notified.</param>
        /// <param name="sampleCount">How many samples to take during a given
        /// reading. These are automatically averaged to reduce noise.</param>
        /// <param name="sampleInterval">The time, to wait in between samples during a reading.</param>
        public Fc28(
            IPin analogInputPin,
            IPin digitalOutputPin,
            Voltage? minimumVoltageCalibration,
            Voltage? maximumVoltageCalibration,
            TimeSpan? updateInterval = null,
            int sampleCount = 5,
            TimeSpan? sampleInterval = null)
                : this(
                    analogInputPin.CreateAnalogInputPort(sampleCount, sampleInterval ?? new TimeSpan(0, 0, 0, 40), new Voltage(3.3)),
                    digitalOutputPin.CreateDigitalOutputPort(),
                    minimumVoltageCalibration,
                    maximumVoltageCalibration)
        {
            UpdateInterval = updateInterval ?? TimeSpan.FromSeconds(5);
        }

        /// <summary>
        /// Creates a FC28 soil moisture sensor object with the specified analog pin and digital pin
        /// </summary>
        /// <param name="analogInputPort">Analog input port connected</param>
        /// <param name="digitalOutputPort">Digital output port connected</param>
        /// <param name="minimumVoltageCalibration">Minimum Voltage Calibration value</param>
        /// <param name="maximumVoltageCalibration">Maximum Voltage Calibration value</param>
        public Fc28(
            IAnalogInputPort analogInputPort,
            IDigitalOutputPort digitalOutputPort,
            Voltage? minimumVoltageCalibration,
            Voltage? maximumVoltageCalibration)
        {
            AnalogInputPort = analogInputPort;
            DigitalOutputPort = digitalOutputPort;
            if (minimumVoltageCalibration is { } min) { MinimumVoltageCalibration = min; }
            if (maximumVoltageCalibration is { } max) { MaximumVoltageCalibration = max; }
        }

        /// <summary>
        /// Reads data from the sensor
        /// </summary>
        /// <returns>The latest sensor reading</returns>
        protected override async Task<double> ReadSensor()
        {
            DigitalOutputPort.State = true;
            var voltage = await AnalogInputPort.Read();
            DigitalOutputPort.State = false;
            return (VoltageToMoisture(voltage));
        }

        /// <summary>
        /// Starts continuously sampling the sensor
        /// </summary>
        public override void StartUpdating(TimeSpan? updateInterval)
        {
            if (updateInterval == null)
            {
                UpdateInterval = TimeSpan.FromSeconds(5);
            }
            else
            {
                UpdateInterval = updateInterval.Value;
            }

            lock (samplingLock)
            {
                if (IsSampling) { return; }

                IsSampling = true;

                SamplingTokenSource = new CancellationTokenSource();
                CancellationToken ct = SamplingTokenSource.Token;

                double? oldConditions;
                ChangeResult<double> result;
                Task.Factory.StartNew(async () =>
                {
                    while (true)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            observers.ForEach(x => x.OnCompleted());
                            break;
                        }
                        oldConditions = Moisture;

                        Moisture = await Read();

                        result = new ChangeResult<double>(Moisture.Value, oldConditions);

                        RaiseChangedAndNotify(result);

                        await Task.Delay(base.UpdateInterval);
                    }
                }, SamplingTokenSource.Token);
            }
        }

        /// <summary>
        /// Stops sampling the sensor
        /// </summary>
        public override void StopUpdating()
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

        /// <summary>
        /// Raise change events for subscribers
        /// </summary>
        /// <param name="changeResult">The change result with the current sensor data</param>
        protected void RaiseChangedAndNotify(IChangeResult<double> changeResult)
        {
            MoistureUpdated?.Invoke(this, changeResult);
            NotifyObservers(changeResult);
        }

        /// <summary>
        /// Converts voltage to moisture value, ranging from 0 (most dry) to 1 (most wet)
        /// </summary>
        /// <param name="voltage"></param>
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