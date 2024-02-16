using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Light;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Light
{
    /// <summary>
    /// Represents an analog light sensor
    /// </summary>
    public partial class AnalogLightSensor
        : SamplingSensorBase<Illuminance>, ILightSensor, IDisposable
    {
        /// <summary>
        /// Analog port connected to sensor
        /// </summary>
        protected IAnalogInputPort AnalogInputPort { get; }

        /// <summary>
        /// Illuminance sensor calibration
        /// </summary>
        public Calibration LuminanceCalibration { get; protected set; }

        /// <summary>
        /// Current illuminance value read by sensor
        /// </summary>
        public Illuminance? Illuminance => illuminance;
        private Illuminance illuminance;

        /// <summary>
        /// Is the object disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Did we create the port(s) used by the peripheral
        /// </summary>
        readonly bool createdPort = false;

        /// <summary>
        /// New instance of the AnalogLightSensor class.
        /// </summary>
        /// <param name="analogPin">Analog pin the sensor is connected to.</param>
        /// <param name="calibration">Calibration for the analog sensor.</param> 
        /// <param name="sampleCount">How many samples to take during a given
        /// reading. These are automatically averaged to reduce noise.</param>
        /// <param name="sampleInterval">The time, in milliseconds,
        /// to wait in between samples during a reading.</param>
        public AnalogLightSensor(
            IPin analogPin,
            Calibration? calibration = null,
            int sampleCount = 5, TimeSpan? sampleInterval = null)
                : this(analogPin.CreateAnalogInputPort(sampleCount, sampleInterval ?? new TimeSpan(0, 0, 40), new Voltage(3.3)), calibration)
        {
            createdPort = true;
        }

        /// <summary>
        /// New instance of the AnalogLightSensor class.
        /// </summary>
        /// <param name="analogInputPort">Analog port the sensor is connected to.</param>
        /// <param name="calibration">Calibration for the analog sensor.</param> 
        public AnalogLightSensor(IAnalogInputPort analogInputPort,
                                 Calibration? calibration = null)
        {
            AnalogInputPort = analogInputPort;

            LuminanceCalibration = calibration ?? new Calibration();

            AnalogInputPort.Subscribe
            (
                IAnalogInputPort.CreateObserver(
                    h =>
                    {
                        var oldLuminance = illuminance;

                        var newLuminance = VoltageToLuminance(h.New);
                        illuminance = newLuminance;

                        RaiseEventsAndNotify(
                            new ChangeResult<Illuminance>(newLuminance, oldLuminance)
                        );
                    }
                )
           );
        }

        /// <summary>
        /// Convenience method to get the current luminance. For frequent reads, use
        /// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        /// </summary>
        protected override async Task<Illuminance> ReadSensor()
        {
            Voltage voltage = await AnalogInputPort.Read();
            return illuminance = VoltageToLuminance(voltage);
        }

        /// <summary>
        /// Starts continuously sampling the sensor.
        ///
        /// This method also starts raising `Changed` events and IObservable
        /// subscribers getting notified. Use the `readIntervalDuration` parameter
        /// to specify how often events and notifications are raised/sent.
        /// </summary>
        /// <param name="updateInterval">A `TimeSpan` that specifies how long to
        /// wait between readings. This value influences how often `*Updated`
        /// events are raised and `IObservable` consumers are notified.
        /// The default is 5 seconds.</param>
        public override void StartUpdating(TimeSpan? updateInterval)
        {
            AnalogInputPort.StartUpdating(updateInterval);
        }

        /// <summary>
        /// Stops sampling the temperature.
        /// </summary>
        public override void StopUpdating()
        {
            AnalogInputPort.StopUpdating();
        }

        /// <summary>
        /// Converts a voltage value to a level in centimeters, based on the current
        /// calibration values.
        /// </summary>
        /// <param name="voltage"></param>
        /// <returns></returns>
        protected Illuminance VoltageToLuminance(Voltage voltage)
        {
            if (voltage <= LuminanceCalibration.VoltsAtZero)
            {
                return new Illuminance(0);
            }
            return new Illuminance((voltage.Volts - LuminanceCalibration.VoltsAtZero.Volts) / LuminanceCalibration.VoltsPerLuminance.Volts);
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
                if (disposing && createdPort)
                {
                    AnalogInputPort?.Dispose();
                }

                IsDisposed = true;
            }
        }
    }
}