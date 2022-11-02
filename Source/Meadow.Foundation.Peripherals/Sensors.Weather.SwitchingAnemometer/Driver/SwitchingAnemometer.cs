using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Weather;
using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SU = Meadow.Units.Speed.UnitType;

namespace Meadow.Foundation.Sensors.Weather
{
    /// <summary>
    /// Driver for a "switching" anememoter (wind speed gauge) that has an
    /// internal switch that is triggered during every revolution.
    /// </summary>
    public partial class SwitchingAnemometer : SamplingSensorBase<Speed>, IAnemometer
    {
        /// <summary>
        /// Raised when the speed of the wind changes
        /// </summary>
        public event EventHandler<IChangeResult<Speed>> WindSpeedUpdated = delegate { };

        /// <summary>
        /// The current wind speed
        /// </summary>
        public Speed? WindSpeed { get; protected set; }

        /// <summary>
        /// Time to wait if no events come in to register a zero speed wind
        /// </summary>
        public TimeSpan NoWindTimeout { get; set; } = TimeSpan.FromSeconds(4);

        /// <summary>
        /// Number of samples to take for a reading
        /// </summary>
        public int SampleCount
        {
            get => sampleCount;
            set
            {
                if (value < 2) { throw new ArgumentException("Sample count must be 2 or more."); }
                sampleCount = value;
            }
        }
        int sampleCount = 3;

        /// <summary>
        /// Calibration for how fast the wind speed is when the switch is hit
        /// once per second. Used to calculate the wind speed based on the time
        /// duration between switch events. Default is `2.4kmh`.
        /// </summary>
        public float KmhPerSwitchPerSecond { get; set; } = 2.4f;

        readonly IDigitalInputPort inputPort;
        bool running = false;

        readonly Queue<DigitalPortResult>? samples;

        /// <summary>
        /// Creates a new `SwitchingAnemometer` using the specific digital input
        /// on the device.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="digitalInputPin"></param>
        public SwitchingAnemometer(IDigitalInputController device, IPin digitalInputPin)
            : this(device.CreateDigitalInputPort(
                digitalInputPin, InterruptMode.EdgeFalling,
                ResistorMode.InternalPullUp, TimeSpan.FromMilliseconds(2), TimeSpan.FromMilliseconds(0)))
        { }

        /// <summary>
        /// Creates a new switching anemometer using the specific `IDigitalInputPort`.
        /// </summary>
        /// <param name="inputPort"></param>
        public SwitchingAnemometer(IDigitalInputPort inputPort)
        {
            this.inputPort = inputPort;

            samples = new Queue<DigitalPortResult>();
        }

        protected void SubscribeToInputPortEvents() => inputPort.Changed += HandleInputPortChange;

        protected void UnsubscribeToInputPortEvents() => inputPort.Changed -= HandleInputPortChange;

        protected void HandleInputPortChange(object sender, DigitalPortResult result)
        {
            if (!running) { return; }

            samples?.Enqueue(result);

            if(samples?.Count > sampleCount)
            {   
                samples.Dequeue();
            }
        }

        protected override void RaiseEventsAndNotify(IChangeResult<Speed> changeResult)
        {
            WindSpeedUpdated?.Invoke(this, changeResult);
            base.NotifyObservers(changeResult);
        }

        /// <summary>
        /// Starts continuously sampling the sensor
        ///
        /// This method also starts raising `Updated` events and IObservable
        /// subscribers getting notified. Use the `standbyDuration` parameter
        /// to specify how often events and notifications are raised/sent.
        /// </summary>
        public override void StartUpdating(TimeSpan? updateInterval = null)
        {
            if (!running)
            {
                running = true;

                SubscribeToInputPortEvents();
                base.StartUpdating(updateInterval);
            }
        }

        /// <summary>
        /// Stops the driver from raising wind speed events
        /// </summary>
        public override void StopUpdating()
        {
            base.StopUpdating();
         
            if (running)
            {
                UnsubscribeToInputPortEvents();
            }
           
            running = false;
        }

        protected override Task<Speed> ReadSensor()
        {
            if(samples?.Count > 0 && (DateTime.Now - samples?.Peek().New.Time > NoWindTimeout))
            {   //we've exceeded the no wind interval time 
                samples?.Clear(); //will force a zero reading
            }
            // if we've reached our sample count
            else if (samples?.Count >= SampleCount)
            {
                float speedSum = 0f;

                // sum up the speeds
                foreach (var sample in samples)
                {   // skip the first (old will be null)
                    if (sample.Old is { } old)
                    {
                        speedSum += SwitchIntervalToKmh(sample.New.Time - old.Time);
                    }
                }

                // average the speeds
                float oversampledSpeed = speedSum / (samples.Count -1);

                return Task.FromResult(new Speed(oversampledSpeed, SU.KilometersPerHour));
            }

            //otherwise return 0 speed
            return Task.FromResult(new Speed(0));
        }

        /// <summary>
        /// A wind speed of 2.4km/h causes the switch to close once per second
        /// </summary>
        /// <param name="interval">The interval between signals</param>
        /// <returns></returns>
        protected float SwitchIntervalToKmh(TimeSpan interval)
        {
            return KmhPerSwitchPerSecond / (float)interval.TotalSeconds;
        }
    }
}