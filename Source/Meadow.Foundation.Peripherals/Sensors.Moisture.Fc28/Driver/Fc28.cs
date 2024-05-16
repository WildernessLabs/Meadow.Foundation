using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Moisture;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Moisture
{
    /// <summary>
    /// FC-28-D Soil Hygrometer Detection Module + Soil Moisture Sensor
    /// </summary>
    public class Fc28 : PollingSensorBase<double>, IMoistureSensor, IDisposable
    {
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
        public double? Moisture => Conditions;

        /// <summary>
        /// Voltage value of most dry soil - default is 0 volts
        /// </summary>
        public Voltage MinimumVoltageCalibration { get; set; } = new Voltage(0);

        /// <summary>
        /// Voltage value of most moist soil - default of 3.3V
        /// </summary>
        public Voltage MaximumVoltageCalibration { get; set; } = new Voltage(3.3);

        /// <summary>
        /// Is the object disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Did we create the port(s) used by the peripheral
        /// </summary>
        readonly bool createdPorts = false;

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
            createdPorts = true;
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
            return VoltageToMoisture(voltage);
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

        ///<inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the object
        /// </summary>
        /// <param name="disposing">Is disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing && createdPorts)
                {
                    AnalogInputPort?.StopUpdating();
                    AnalogInputPort?.Dispose();
                    DigitalOutputPort?.Dispose();
                }

                IsDisposed = true;
            }
        }
    }
}