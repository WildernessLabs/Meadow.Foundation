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
    /// Driver to measure solar panel input. 
    /// </summary>
    public class AnalogSolarIntensityGauge : SensorBase<float>,
        ISolarIntensityGauge, ISensor
    {
        //==== events
        public event EventHandler<IChangeResult<float>> SolarIntensityUpdated = delegate { };

        //==== internals
        protected IAnalogInputPort analogIn;

        //==== properties
        public Voltage MinVoltageReference { get; protected set; } = new Voltage(0, VU.Volts);
        public Voltage MaxVoltageReference { get; protected set; } = new Voltage(3.3, VU.Volts);

        /// <summary>
        /// Gets percentage of solar intensity, from `0` to `1.0`, with `1.0` being
        /// the `MaxVoltageReference`, and `0` being the `MinVoltageReference`.
        /// </summary>
        public float? SolarIntensity { get; protected set; }

        public AnalogSolarIntensityGauge(IAnalogInputPort analogIn)
        {
            // TODO: input port validation if any (is it constructed all right?)
            this.analogIn = analogIn;
            Init();
        }

        public AnalogSolarIntensityGauge(
            IAnalogInputPort analogIn,
            Voltage minVoltageReference, Voltage maxVoltageReference)
            : this(analogIn)
        {
            this.MinVoltageReference = minVoltageReference;
            this.MaxVoltageReference = maxVoltageReference;
        }

        protected void Init()
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
            analogIn.Subscribe(observer);
        }

        public Task<float> Read(int sampleCount = 5, int sampleIntervalDuration = 40)
        {
            return ReadSensor(sampleCount, sampleIntervalDuration);
        }

        protected override Task<float> ReadSensor()
        {
            return ReadSensor(5, 40);
        }

        protected async Task<float> ReadSensor(int sampleCount = 5, int sampleIntervalDuration = 40)
        {
            // read the voltage
            Voltage voltage = await analogIn.Read(sampleCount, sampleIntervalDuration);

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
        /// <param name="sampleCount">How many samples to take during a given
        /// reading. These are automatically averaged to reduce noise.</param>
        /// <param name="sampleIntervalDuration">The time, in milliseconds,
        /// to wait in between samples during a reading.</param>
        /// <param name="standbyDuration">The time, in milliseconds, to wait
        /// between sets of sample readings. This value determines how often
        /// `Changed` events are raised and `IObservable` consumers are notified.</param>
        public void StartUpdating(
            int sampleCount = 5,
            int sampleIntervalDuration = 40,
            int standbyDuration = 10000)
        {
            analogIn.StartUpdating(sampleCount, sampleIntervalDuration, standbyDuration);
        }

        /// <summary>
        /// Stops sampling the solar intensity.
        /// </summary>
        public void StopUpdating()
        {
            analogIn.StopUpdating();
        }

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
