using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation
{
    /// <summary>
    /// Base class for sensors and other updating classes that want to support
    /// having their updates consumed by observers that can optionally use filters.
    /// </summary>
    /// <typeparam name="UNIT"></typeparam>
    public abstract class SensorBase<UNIT> : ObservableBase<UNIT>
        where UNIT : struct
    {
        //==== events
        public event EventHandler<IChangeResult<UNIT>> Updated = delegate { };

        //==== internals
        protected object samplingLock = new object();
        protected CancellationTokenSource? SamplingTokenSource { get; set; }

        //==== properties
        /// <summary>
        /// The last read conditions.
        /// </summary>
        public UNIT Conditions { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether the sensor is currently in a sampling
        /// loop. Call StartSampling() to spin up the sampling process.
        /// </summary>
        /// <value><c>true</c> if sampling; otherwise, <c>false</c>.</value>
        public bool IsSampling { get; protected set; } = false;

        /// <summary>
        /// A `TimeSpan` that specifies how long to
        /// wait between readings. This value influences how often `*Updated`
        /// events are raised and `IObservable` consumers are notified.
        /// </summary>
        public virtual TimeSpan UpdateInterval { get; protected set; } = TimeSpan.FromSeconds(5);

        //==== ISensor Methods
        protected abstract Task<UNIT> ReadSensor();

        protected virtual void RaiseEventsAndNotify(IChangeResult<UNIT> changeResult)
        {
            Updated.Invoke(this, changeResult);
            base.NotifyObservers(changeResult);
        }

        /// <summary>
        /// Convenience method to get the current sensor readings. For frequent reads, use
        /// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        /// </summary>
        // TODO: `ValueTask`?
        public virtual async Task<UNIT> Read()
        {
            this.Conditions = await ReadSensor();
            return Conditions;
        }
    }
}