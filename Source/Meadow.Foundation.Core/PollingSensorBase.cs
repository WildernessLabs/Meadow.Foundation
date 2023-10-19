using Meadow.Peripherals.Sensors;
using System;
using System.Threading;

namespace Meadow.Foundation
{
    /// <summary>
    /// Base class that represents a sampling sensor to support the observable pattern
    /// </summary>
    /// <typeparam name="UNIT"></typeparam>
    public abstract class PollingSensorBase<UNIT>
        : SamplingSensorBase<UNIT>, ISamplingSensor<UNIT>, IPollingSensor
        where UNIT : struct
    {
        private UNIT? _previousConditions = null;
        private ISensorMonitor? _sensorMonitor;

        public PollingSensorBase()
        {
            SensorMonitor = new PollingSensorMonitor<UNIT>(this, base.SamplingTokenSource);
        }

        /// <summary>
        /// The monitor being used to poll the sensor value
        /// </summary>
        public ISensorMonitor? SensorMonitor
        {
            get => _sensorMonitor;
            set
            {
                if (_sensorMonitor != null)
                {
                    _sensorMonitor.SampleAvailable -= OnSensorSampleAvailable;
                }
                _sensorMonitor = value;
                if (_sensorMonitor != null)
                {
                    _sensorMonitor.SampleAvailable += OnSensorSampleAvailable;
                }
            }
        }

        private void OnSensorSampleAvailable(object sender, object value)
        {
            if (sender != this) return;

            _previousConditions = Conditions;

            Conditions = (UNIT)value;

            var result = new ChangeResult<UNIT>(Conditions, _previousConditions);

            RaiseEventsAndNotify(result);
        }

        /// <summary>
        /// Starts updating the sensor on the updateInterval frequency specified.
        ///
        /// This method also starts raising `Updated` events and notifying
        /// IObservable subscribers. Use the `updateInterval` parameter
        /// to specify how often events and notifications are raised/sent.
        /// </summary>
        /// <param name="updateInterval">A `TimeSpan` that specifies how long to
        /// wait between readings. This value influences how often `*Updated`
        /// events are raised and `IObservable` consumers are notified.
        /// The default is 5 seconds.</param>
        public override void StartUpdating(TimeSpan? updateInterval = null)
        {
            lock (samplingLock)
            {
                if (IsSampling) { return; }

                IsSampling = true;

                // if an update interval has been passed in, override the default
                if (updateInterval is { } ui) { base.UpdateInterval = ui; }

                base.SamplingTokenSource = new CancellationTokenSource();

                _sensorMonitor?.StartSampling(this);
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

                _sensorMonitor?.StopSampling(this);
                SamplingTokenSource?.Cancel();

                IsSampling = false;
            }
        }
    }
}