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
    public class Capacitive : SamplingSensorBase<double>, IMoistureSensor
    {
        /// <summary>
        /// Raised when a new sensor reading has been made
        /// </summary>
        public event EventHandler<IChangeResult<double>> MoistureUpdated = delegate { };

        /// <summary>
        /// Returns the analog input port
        /// </summary>
        protected IAnalogInputPort AnalogInputPort { get; }

        /// <summary>
        /// Last value read from the moisture sensor
        /// </summary>
        public double? Moisture { get; protected set; }

        /// <summary>
        /// Voltage value of most dry soil - default is 0 volts
        /// </summary>
        public Voltage MinimumVoltageCalibration { get; set; } = new Voltage(0);

        /// <summary>
        /// Voltage value of most moist soil - default of 3.3V
        /// </summary>
        public Voltage MaximumVoltageCalibration { get; set; } = new Voltage(3.3);

        /// <summary>
        /// Creates a Capacitive soil moisture sensor object with the specified analog pin and a IO device
        /// </summary>
        /// <param name="analogInputPin">Analog pin the temperature sensor is connected to</param>
        /// <param name="minimumVoltageCalibration">Minimum calibration voltage</param>
        /// <param name="maximumVoltageCalibration">Maximum calibration voltage</param>
        /// <param name="sampleCount">How many samples to take during a given reading</param>
        /// <param name="sampleInterval">The time, to wait in between samples during a reading</param>
        public Capacitive(
            IPin analogInputPin,
            Voltage? minimumVoltageCalibration,
            Voltage? maximumVoltageCalibration,
            int sampleCount = 5,
            TimeSpan? sampleInterval = null)
                : this(
                    analogInputPin.CreateAnalogInputPort(sampleCount, sampleInterval ?? TimeSpan.FromMilliseconds(40), new Voltage(3.3)),
                    minimumVoltageCalibration,
                    maximumVoltageCalibration)
        { }

        /// <summary>
        /// Creates a Capacitive soil moisture sensor object with the specified AnalogInputPort
        /// </summary>
        /// <param name="analogInputPort">The port for the analog input pin</param>
        /// <param name="minimumVoltageCalibration">Minimum calibration voltage</param>
        /// <param name="maximumVoltageCalibration">Maximum calibration voltage</param>
        public Capacitive(
            IAnalogInputPort analogInputPort,
            Voltage? minimumVoltageCalibration,
            Voltage? maximumVoltageCalibration)
        {
            AnalogInputPort = analogInputPort;

            if (minimumVoltageCalibration is { } min) { MinimumVoltageCalibration = min; }
            if (maximumVoltageCalibration is { } max) { MaximumVoltageCalibration = max; }

            AnalogInputPort.Subscribe
            (
                IAnalogInputPort.CreateObserver(
                    result =>
                    {
                        ChangeResult<double> changeResult = new()
                        {
                            New = VoltageToMoisture(result.New),
                            Old = Moisture
                        };
                        Moisture = changeResult.New;
                        RaiseEventsAndNotify(changeResult);
                    }
                )
           );
        }

        /// <summary>
        /// Reads data from the sensor
        /// </summary>
        /// <returns>The latest sensor reading</returns>
        protected override async Task<double> ReadSensor()
        {
            var voltage = await AnalogInputPort.Read();
            var newMoisture = VoltageToMoisture(voltage);
            Moisture = newMoisture;
            return newMoisture;
        }

        /// <summary>
        /// Starts continuously sampling the sensor
        /// </summary>
        public override void StartUpdating(TimeSpan? updateInterval)
        {
            lock (samplingLock)
            {
                if (IsSampling) { return; }
                IsSampling = true;
                AnalogInputPort.StartUpdating(updateInterval);
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
                IsSampling = false;
                AnalogInputPort.StopUpdating();
            }
        }

        /// <summary>
        /// Raise change events for subscribers
        /// </summary>
        /// <param name="changeResult">The change result with the current sensor data</param>
        protected override void RaiseEventsAndNotify(IChangeResult<double> changeResult)
        {
            MoistureUpdated?.Invoke(this, changeResult);
            base.RaiseEventsAndNotify(changeResult);
        }

        /// <summary>
        /// Converts voltage to moisture value, ranging from 0 (most dry) to 1 (most wet)
        /// </summary>
        /// <param name="voltage"></param>
        protected double VoltageToMoisture(Voltage voltage)
        {
            if (MinimumVoltageCalibration > MaximumVoltageCalibration)
            {
                return (1f - voltage.Volts.Map(MaximumVoltageCalibration.Volts, MinimumVoltageCalibration.Volts, 0f, 1.0f));
            }
            return (1f - voltage.Volts.Map(MinimumVoltageCalibration.Volts, MaximumVoltageCalibration.Volts, 0f, 1.0f));
        }
    }
}