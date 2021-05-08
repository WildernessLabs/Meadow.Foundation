using System;
using System.Collections.Generic;
using System.Threading;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Weather;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Weather
{
    /// <summary>
    /// Driver for a "switching" anememoter (wind speed gauge) that has an
    /// internal switch that is triggered during every revolution.
    /// </summary>
    public partial class SwitchingAnemometer :
        FilterableChangeObservableBase<Speed>, IAnemometer
    {
        //==== events
        /// <summary>
        /// Raised when the speed of the wind changes.
        /// </summary>
        public event EventHandler<IChangeResult<Speed>> WindSpeedUpdated = delegate { };

        //==== internals
        IDigitalInputPort inputPort;
        bool running = false;
        int standbyDuration;
        int overSampleCount;
        System.Timers.Timer? noWindTimer;
        List<DigitalInputPortChangeResult>? samples;

        // Turn on for debug output
        bool debug = false;

        //==== properties
        /// <summary>
        /// The last recored wind speed.
        /// </summary>
        public Speed? WindSpeed { get; protected set; }

        /// <summary>
        /// Calibration for how fast the wind speed is when the switch is hit
        /// once per second. Used to calculate the wind speed based on the time
        /// duration between switch events. Default is `2.4kmh`.
        /// </summary>
        public float KmhPerSwitchPerSecond { get; set; } = 2.4f;

        /// <summary>
        /// Creates a new `SwitchingAnemometer` using the specific digital input
        /// on the device.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="digitalInputPin"></param>
        public SwitchingAnemometer(IDigitalInputController device, IPin digitalInputPin)
            : this(device.CreateDigitalInputPort(
                digitalInputPin, InterruptMode.EdgeFalling,
                ResistorMode.InternalPullUp, 20, 20))
        {
        }

        /// <summary>
        /// Creates a new switching anemometer using the specific `IDigitalInputPort`.
        /// </summary>
        /// <param name="inputPort"></param>
        public SwitchingAnemometer(IDigitalInputPort inputPort)
        {
            this.inputPort = inputPort;
            this.Init();
        }

        protected void Init()
        {
        }

        protected void SubscribeToinputPortEvents()
        {
            inputPort.Changed += HandleInputPortChange;
        }

        protected void UnsubscribeToInputPortEvents()
        {
            inputPort.Changed -= HandleInputPortChange;
        }

        protected void HandleInputPortChange(object sender, DigitalInputPortChangeResult result)
        {
            if (!running) { return; }

            if(samples == null) { samples = new List<DigitalInputPortChangeResult>(); }

            // reset our nowind timer, since a sample has come in. note that the API is awkward
            noWindTimer?.Stop();
            noWindTimer?.Start();

            // we need at least two readings to get a valid speed, since the
            // speed is based on duration between clicks, we need at least two
            // clicks to measure duration.
            samples.Add(result);
            if (debug) { Console.WriteLine($"result #[{samples.Count}] new: [{result.New}], old: [{result.Old}]"); }

            // if we don't have two samples, move on
            if (samples.Count < 1) { return; }

            // if we've reached our sample count
            if (samples.Count >= overSampleCount) {
                float speedSum = 0f;
                float oversampledSpeed = 0f;
                // skip the first sample, which won't have a valid reading
                for (int i = 1; i < samples.Count; i++ ){
                    //if (debug) { Console.WriteLine($"Sample [{i}] speed: [{SwitchIntervalToKmh(samples[i].Delta)}]; duration: {samples[i].Delta}, newTime: {samples[i].New}, oldTime: {samples[i].Old}"); }
                    speedSum += SwitchIntervalToKmh(samples[i].Delta.Value);
                }
                oversampledSpeed = speedSum / samples.Count - 1;

                // clear our samples
                this.samples.Clear();

                RaiseUpdated(new Speed(oversampledSpeed, Speed.UnitType.KilometersPerHour));

                // if we need to wait before taking another sample set, 
                if (this.standbyDuration > 0) {
                    this.UnsubscribeToInputPortEvents();
                    Thread.Sleep(standbyDuration);
                    this.SubscribeToinputPortEvents();
                }
            }
        }

        // TODO: refactor this to match other sensors, e.g. RaiseUpdated(IChangeResult<Speed> result)
        protected void RaiseUpdated(Speed newSpeed)
        {
            //AnemometerChangeResult result = new AnemometerChangeResult() {
            //    Old = this.LastRecordedWindSpeed,
            //    New = newSpeed
            //};
            ChangeResult<Speed> result = new ChangeResult<Speed>() {
                Old = this.WindSpeed,
                New = newSpeed
            };
            // capture last recorded now that we have a new result
            this.WindSpeed = newSpeed;

            //if (debug) {
            //    Console.WriteLine($"1) Result.Old: {result.Old}, New: {result.New}");
            //    Console.WriteLine($"2) Delta: {result.Delta}");
            //}

            WindSpeedUpdated?.Invoke(this, result);
            base.NotifyObservers(result);
        }

        /// <summary>
        /// A wind speed of 2.4km/h causes the switch to close once per second.
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        protected float SwitchIntervalToKmh(TimeSpan interval)
        {
            // A wind speed of 2.4km/h causes the switch to close once per second.
            return this.KmhPerSwitchPerSecond / ((float)interval.TotalMilliseconds / 1000f);
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
        /// <param name="standbyDuration">The time, in milliseconds, to wait
        /// between listening for events. This value determines how often
        /// `Changed` events are raised and `IObservable` consumers are notified.</param>
        /// <param name="noWindTimeout">The time, in milliseconds in which to
        /// wait for events from the anemometer. If no events come in by the time
        /// this elapses, then an event of `0` wind will be raised.</param>
        public void StartUpdating(
            int sampleCount = 5,
            int standbyDuration = 500,
            int noWindTimeout = 4000)
        {
            if(standbyDuration < 0) { throw new ArgumentException("`StandbyDuration` must be greater than or equal to `0`."); }
            if(sampleCount < 2) { sampleCount = 2; }
            if(running) { return; }

            this.overSampleCount = sampleCount;
            this.standbyDuration = standbyDuration;

            running = true;

            // start a timer that we can use to raise a zero wind event in the case
            // that we're not getting input events (because there is no wind)
            noWindTimer = new System.Timers.Timer(noWindTimeout);
            noWindTimer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) => {
                if (debug) { Console.WriteLine("No wind timer elapsed."); }
                // if not running, clear the timer and bail out.
                if (!running) {
                    noWindTimer.Stop();
                    return;
                }
                // if there aren't enough samples to make a reading
                if (samples == null || samples.Count <= overSampleCount ) {
                    // raise the wind updated event with `0` wind speed
                    RaiseUpdated(0);
                    // sleep for the standby duration
                    if (standbyDuration > 0) {
                        if (debug) { Console.WriteLine("Sleeping for a bit."); }
                        Thread.Sleep(standbyDuration);
                        if (debug) { Console.WriteLine("Woke up."); }
                    }

                    if (debug) { Console.WriteLine($"timer enabled? {noWindTimer.Enabled}"); }

                    // if still running, start the timer again
                    if (running) {
                        if (debug) { Console.WriteLine("restarting timer."); }
                        noWindTimer.Start();
                    }
                }
            };
            noWindTimer.Start();

            SubscribeToinputPortEvents();
        }

        /// <summary>
        /// Stops the driver from raising wind speed events.
        /// </summary>
        public void StopUpdating()
        {
            if(running) {
                UnsubscribeToInputPortEvents();
            }
            if (noWindTimer != null && noWindTimer.Enabled) {
                noWindTimer.Stop();
            }

            // state machine
            running = false;
        }

    }
}
