using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using VU = Meadow.Units.Voltage.UnitType;

namespace Meadow.Foundation.Sensors.Light
{
    /// <summary>
    /// Driver to measure solar panel input
    /// </summary>
    public class AnalogSolarGauge : SensorBase<float>,
        ISolarIntensityGauge
    {
        /// <summary>
        /// Raised when the solar intensity changes
        /// </summary>
        public event EventHandler<IChangeResult<float>> SolarIntensityUpdated = delegate { };

        IAnalogInputPort analogInputPort;

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
        /// Creates a new instance of an analog solar intensity driver.
        /// </summary>
        /// <param name="device">The `IAnalogInputController` to create the port on</param>
        /// <param name="analogPin">Analog pin the temperature sensor is connected to</param>
        /// <param name="minVoltageReference">The minimum voltage expected when the solar panel isn't receiving light - Default is 0</param>
        /// <param name="maxVoltageReference">The maxmimu voltage expected when the solar panel is in full sun. Default is 3.3V</param>
        /// <param name="updateInterval">The time to wait
        /// between sets of sample readings. This value determines how often
        /// `Changed` events are raised and `IObservable` consumers are notified</param>
        /// <param name="sampleCount">How many samples to take during a given reading. These are automatically averaged to reduce noise</param>
        /// <param name="sampleInterval">The time to wait in between samples during a reading</param>
        public AnalogSolarGauge(
            IAnalogInputController device, 
            IPin analogPin,
            Voltage? minVoltageReference = null, 
            Voltage? maxVoltageReference = null,
            TimeSpan? updateInterval = null,
            int sampleCount = 5, 
            TimeSpan? sampleInterval = null)
             : this(device.CreateAnalogInputPort(analogPin, 
                                                 sampleCount, 
                                                 sampleInterval ?? new TimeSpan(0, 0, 0, 40), 
                                                 maxVoltageReference ?? new Voltage(3.3)),
                   minVoltageReference, maxVoltageReference)
        {
            base.UpdateInterval = updateInterval ?? new TimeSpan(0, 0, 10);
        }

        /// <summary>
        /// Creates a new instance of an analog solar intensity driver.
        /// </summary>
        /// <param name="analogIn">The `IAnalogInputPort` connected to the solar panel.</param>
        /// <param name="minVoltageReference">The minimum voltage expected when the solar panel isn't receiving light. Default is 0.</param>
        /// <param name="maxVoltageReference">The maxmimu voltage expected when the solar panel is in full sun. Default is 3.3V.</param>
        public AnalogSolarGauge(
            IAnalogInputPort analogIn,
            Voltage? minVoltageReference = null, Voltage? maxVoltageReference = null)
        {
            if (minVoltageReference is { } minV) { MinVoltageReference = minV; }
            if (maxVoltageReference is { } maxV) { MaxVoltageReference = maxV; }

            // TODO: input port validation if any (is it constructed all right?)
            analogInputPort = analogIn;
            Initialize();
        }

        /// <summary>
        /// Initialize the sensor
        /// </summary>
        protected void Initialize()
        {
            // wire up our analog input observer
            var observer = IAnalogInputPort.CreateObserver(
                handler: result => {
                    // create a new change result from the new value
                    ChangeResult<float> changeResult = new ChangeResult<float>() {
                        New = ConvertVoltageToIntensity(result.New),
                        Old = SolarIntensity
                    };
                    // save state
                    SolarIntensity = changeResult.New;
                    // notify
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
            // read the voltage
            Voltage voltage = await analogInputPort.Read();

            // convert the voltage
            var newSolarIntensity = ConvertVoltageToIntensity(voltage);

            // save our value
            SolarIntensity = newSolarIntensity;

            return newSolarIntensity;
        }

        /// <summary>
        /// Starts continuously sampling the sensor.
        ///
        /// This method also starts raising `SolarIntensityUpdated` events and IObservable
        /// subscribers getting notified. Use the `readIntervalDuration` parameter
        /// to specify how often events and notifications are raised/sent.
        /// </summary>
        /// <param name="updateInterval">A `TimeSpan` that specifies how long to
        /// wait between readings. This value influences how often `*Updated`
        /// events are raised and `IObservable` consumers are notified.
        /// The default is 5 seconds.</param>
        public void StartUpdating(TimeSpan updateInterval)
        {
            analogInputPort.StartUpdating(updateInterval);
        }

        /// <summary>
        /// Stops sampling the solar intensity.
        /// </summary>
        public void StopUpdating()
        {
            analogInputPort.StopUpdating();
        }

        /// <summary>
        /// Raise events for subcribers and notify of value changes
        /// </summary>
        /// <param name="changeResult">The updated sensor data</param>
        protected override void RaiseEventsAndNotify(IChangeResult<float> changeResult)
        {
            this.SolarIntensityUpdated?.Invoke(this, changeResult);
            base.RaiseEventsAndNotify(changeResult);
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
    }
}