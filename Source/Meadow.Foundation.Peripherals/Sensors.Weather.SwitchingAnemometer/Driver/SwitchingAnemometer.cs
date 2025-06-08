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
    /// Driver for a "switching" anemometer (wind speed gauge) that has an
    /// internal switch that is triggered during every revolution.
    /// </summary>
    public class SwitchingAnemometer : PollingSensorBase<Speed>, IAnemometer, IDisposable
    {
        /// <summary>
        /// The current wind speed
        /// </summary>
        public Speed? WindSpeed => Conditions;

        /// <summary>
        /// Time to wait if no events come in to register a zero speed wind
        /// </summary>
        /// <remarks>
        /// This value determines the minimum wind speed that can be measured.
        /// i.e. minimum speed is KmhPerSwitchPerSecond / NoWindTimeout(seconds)
        /// e.g. (2.4 km/hr/s) / (4s) = 600 m/s 
        /// <see cref="KmhPerSwitchPerSecond"/>
        /// </remarks>
        public TimeSpan NoWindTimeout { get; set; } = TimeSpan.FromSeconds(4);

        /// <summary>
        /// Define the theoretical maximum speed that can be measured. Higher speeds are ignored. 
        /// </summary>
        /// <remarks>
        /// Category 5 Hurricane is > 300 km/hr. 
        /// </remarks>
        public Speed? MaxSpeed { get; set; } = new Speed(500, SU.KilometersPerHour);


        /// <summary>
        /// Time to capture samples for a one time Read if IsSampling is false
        /// </summary>
        public TimeSpan OneTimeReadDuration { get; set; } = TimeSpan.FromSeconds(1);

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

        private int sampleCount = 3;

        /// <summary>
        /// Calibration for how fast the wind speed is when the switch is hit
        /// once per second. Used to calculate the wind speed based on the time
        /// duration between switch events. Default is `2.4kmh`.
        /// </summary>
        public double KmhPerSwitchPerSecond { get; set; } = 2.4;

        private readonly IDigitalInterruptPort inputPort;
        private bool running = false;
        private readonly Queue<DigitalPortResult> samples = new();

        /// <summary>
        /// Is the object disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Did we create the port(s) used by the peripheral
        /// </summary>
        private readonly bool createdPort = false;

        /// <summary>
        /// Creates a new `SwitchingAnemometer` using the specific digital input
        /// on the device.
        /// </summary>
        /// <param name="digitalInputPin"></param>
        /// <remarks>
        /// Debounce will limit upper speed to approximately half of (2.4/0.002) = 1200 km/hr.
        /// i.e. +/- 600 km/hr assuming events can be raised that quickly. 
        /// </remarks>
        public SwitchingAnemometer(IPin digitalInputPin)
            : this(digitalInputPin.CreateDigitalInterruptPort(InterruptMode.EdgeFalling,
                                                            ResistorMode.InternalPullUp,
                                                            TimeSpan.FromMilliseconds(2),
                                                            TimeSpan.FromMilliseconds(0)))
        {
            createdPort = true;
        }

        /// <summary>
        /// Creates a new switching anemometer using the specific `IDigitalInputPort`.
        /// </summary>
        /// <param name="inputPort"></param>
        public SwitchingAnemometer(IDigitalInterruptPort inputPort)
        {
            this.inputPort = inputPort;
        }

        private void SubscribeToInputPortEvents() => inputPort.Changed += HandleInputPortChange;

        private void UnsubscribeToInputPortEvents() => inputPort.Changed -= HandleInputPortChange;

        private void HandleInputPortChange(object sender, DigitalPortResult result)
        {
            if (!running) { return; }

            lock (samples)
            {
                samples?.Enqueue(result);

                if (samples?.Count > sampleCount)
                {
                    samples.Dequeue();
                }
            }
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

        /// <summary>
        /// Reads data from the sensor
        /// </summary>
        /// <returns>The latest sensor reading</returns>
        protected override async Task<Speed> ReadSensor()
        {
            if (IsSampling == false)
            {
                StartUpdating();
                await Task.Delay(OneTimeReadDuration);
                StopUpdating();
            }

            lock (samples)
            {
                int count = 0;

                if (samples?.Count > 0 && samples?.Peek().Delta > NoWindTimeout)
                {   //we've exceeded the no wind interval time 
                    samples?.Clear(); //will force a zero reading
                    return new Speed(0);
                }

                // do we have enough samples to calculate a result?
                if (samples?.Count >= SampleCount)
                {
                    double speedSum = 0f;

                    // sum up the speeds
                    foreach (var sample in samples)
                    {
                        // Check delta is not null and reasonable. (0 ms would be infinite speed. What is a reasonable maximum speed?)
                        if (sample.Delta is { Milliseconds: > 0 } delta)
                        {
                            double speed = SwitchIntervalToKmh(delta);

                            // skip speeds that are unreasonably high
                            if (MaxSpeed?.KilometersPerHour >= speed)
                            {
                                speedSum += speed;
                                count++;

                                //Resolver.Log.Info($"count {count} : delta={delta} speed={speed:N4} speedSum={speedSum:N4} sample={sample} ");
                            }
                        }
                    }

                    // do we have enough samples to report an observation?
                    if (count >= SampleCount)
                    {
                        // average the speeds
                        double oversampledSpeed = speedSum / count;
                        return new Speed(oversampledSpeed, SU.KilometersPerHour);
                    }
                }
            }

            //otherwise return 0 speed
            return new Speed(0);
        }

        /// <summary>
        /// A wind speed of 2.4km/h causes the switch to close once per second
        /// </summary>
        /// <param name="interval">The interval between signals</param>
        /// <returns></returns>
        protected double SwitchIntervalToKmh(TimeSpan interval)
        {
            return KmhPerSwitchPerSecond / (double)interval.TotalSeconds;
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
                    inputPort?.Dispose();
                }

                IsDisposed = true;
            }
        }
    }
}