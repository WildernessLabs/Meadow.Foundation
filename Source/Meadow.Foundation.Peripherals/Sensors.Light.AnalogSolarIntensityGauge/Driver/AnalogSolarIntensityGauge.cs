using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Light;
using Meadow.Units;
using System;
using System.Threading.Tasks;
using VU = Meadow.Units.Voltage.UnitType;

namespace Meadow.Foundation.Sensors.Light
{
    /// <summary>
    /// Driver to measure solar panel input
    /// </summary>
    public class AnalogSolarIntensityGauge : SamplingSensorBase<float>, ISolarIntensityGauge, IDisposable
    {
        readonly IAnalogInputPort analogInputPort;

        /// <summary>
        /// Minimum voltage reference
        /// </summary>
        public Voltage MinVoltageReference { get; protected set; } = new Voltage(0, VU.Volts);

        /// <summary>
        /// Maximum voltage reference
        /// </summary>
        public Voltage MaxVoltageReference { get; protected set; } = new Voltage(3.3, VU.Volts);

        /// <summary>
        /// Gets percentage of solar intensity, from `0` to `1.0`, with `1.0` being
        /// the `MaxVoltageReference`, and `0` being the `MinVoltageReference`.
        /// </summary>
        public float? SolarIntensity { get; protected set; }

        /// <summary>
        /// Is the object disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Did we create the port(s) used by the peripheral
        /// </summary>
        readonly bool createdPort = false;

        /// <summary>
        /// Creates a new instance of an analog solar intensity driver.
        /// </summary>
        /// <param name="analogPin">Analog pin the temperature sensor is connected to</param>
        /// <param name="minVoltageReference">The minimum voltage expected when the solar panel isn't receiving light - Default is 0</param>
        /// <param name="maxVoltageReference">The maximum voltage expected when the solar panel is in full sun. Default is 3.3V</param>
        /// <param name="updateInterval">The time to wait
        /// between sets of sample readings. This value determines how often
        /// `Changed` events are raised and `IObservable` consumers are notified</param>
        /// <param name="sampleCount">How many samples to take during a given reading. These are automatically averaged to reduce noise</param>
        /// <param name="sampleInterval">The time to wait in between samples during a reading</param>
        public AnalogSolarIntensityGauge(
            IPin analogPin,
            Voltage? minVoltageReference = null,
            Voltage? maxVoltageReference = null,
            TimeSpan? updateInterval = null,
            int sampleCount = 5,
            TimeSpan? sampleInterval = null)
             : this(analogPin.CreateAnalogInputPort(
                                                 sampleCount,
                                                 sampleInterval ?? new TimeSpan(0, 0, 0, 40),
                                                 maxVoltageReference ?? new Voltage(3.3)),
                   minVoltageReference, maxVoltageReference)
        {
            createdPort = true;
            base.UpdateInterval = updateInterval ?? new TimeSpan(0, 0, 10);
        }

        /// <summary>
        /// Creates a new instance of an analog solar intensity driver.
        /// </summary>
        /// <param name="analogPort">The `IAnalogInputPort` connected to the solar panel.</param>
        /// <param name="minVoltageReference">The minimum voltage expected when the solar panel isn't receiving light. Default is 0.</param>
        /// <param name="maxVoltageReference">The maximum voltage expected when the solar panel is in full sun. Default is 3.3V.</param>
        public AnalogSolarIntensityGauge(
            IAnalogInputPort analogPort,
            Voltage? minVoltageReference = null, Voltage? maxVoltageReference = null)
        {
            if (minVoltageReference is { } minV) { MinVoltageReference = minV; }
            if (maxVoltageReference is { } maxV) { MaxVoltageReference = maxV; }

            // TODO: input port validation
            analogInputPort = analogPort;
            Initialize();
        }

        /// <summary>
        /// Initialize the sensor
        /// </summary>
        protected void Initialize()
        {
            var observer = IAnalogInputPort.CreateObserver(
                handler: result =>
                {
                    ChangeResult<float> changeResult = new ChangeResult<float>()
                    {
                        New = ConvertVoltageToIntensity(result.New),
                        Old = SolarIntensity
                    };
                    SolarIntensity = changeResult.New;
                    RaiseEventsAndNotify(changeResult);
                },
                null);
            analogInputPort.Subscribe(observer);
        }

        /// <summary>
        /// Reads data from the sensor
        /// </summary>
        /// <returns>The latest sensor reading</returns>
        protected override async Task<float> ReadSensor()
        {
            Voltage voltage = await analogInputPort.Read();

            var newSolarIntensity = ConvertVoltageToIntensity(voltage);

            SolarIntensity = newSolarIntensity;

            return newSolarIntensity;
        }

        /// <summary>
        /// Starts continuously sampling the sensor
        ///
        /// This method also starts raising `SolarIntensityUpdated` events and IObservable
        /// subscribers getting notified. Use the `readIntervalDuration` parameter
        /// to specify how often events and notifications are raised/sent.
        /// </summary>
        /// <param name="updateInterval">A `TimeSpan` that specifies how long to
        /// wait between readings. This value influences how often `*Updated`
        /// events are raised and `IObservable` consumers are notified.
        ///  </param>
        public override void StartUpdating(TimeSpan? updateInterval)
        {
            analogInputPort.StartUpdating(updateInterval);
        }

        /// <summary>
        /// Stops sampling the solar intensity
        /// </summary>
        public override void StopUpdating()
        {
            analogInputPort.StopUpdating();
        }

        /// <summary>
        /// Converts a voltage reading to a solar intensity percentage, taking into
        /// account the minimum and maximum expected values.
        /// </summary>
        /// <param name="voltage"></param>
        /// <returns>`0.0` - `1.0`</returns>
        protected float ConvertVoltageToIntensity(Voltage voltage)
        {
            return (float)voltage.Volts.Map(MinVoltageReference.Volts, MaxVoltageReference.Volts, 0.0, 1.0);
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
                    analogInputPort?.StopUpdating();
                    analogInputPort?.Dispose();
                }

                IsDisposed = true;
            }
        }
    }
}