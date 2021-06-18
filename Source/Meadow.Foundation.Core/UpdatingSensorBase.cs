using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation
{
    public abstract class UpdatingSensorBase<UNIT>
        : SensorBase<UNIT>
        where UNIT : struct
    {
        public UpdatingSensorBase()
        {
        }

        //==== internals
        protected object _lock = new object();
        private CancellationTokenSource? SamplingTokenSource { get; set; }

        /// <summary>
        /// Starts updating the sensor on an update interval of `5` seconds.
        ///
        /// This method also starts raising `Changed` events and notifying
        /// IObservable subscribers. Use the `updateInterval` parameter
        /// to specify how often events and notifications are raised/sent.
        /// </summary>
        public virtual void StartUpdating()
        {
            StartUpdating(TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// Starts updating the sensor on the updateInterval frequency specified.
        ///
        /// This method also starts raising `Changed` events and notifying
        /// IObservable subscribers. Use the `updateInterval` parameter
        /// to specify how often events and notifications are raised/sent.
        /// </summary>
        /// <param name="updateInterval">A `TimeSpan` that specifies how long to
        /// wait between readings. This value influences how often `*Updated`
        /// events are raised and `IObservable` consumers are notified.</param>
        public virtual void StartUpdating(TimeSpan updateInterval)
        {
            // thread safety
            lock (_lock) {
                if (IsSampling) return;

                IsSampling = true;

                SamplingTokenSource = new CancellationTokenSource();
                CancellationToken ct = SamplingTokenSource.Token;

                UNIT oldConditions;
                ChangeResult<UNIT> result;

                Task.Factory.StartNew(async () => {
                    while (true) {
                        // cleanup
                        if (ct.IsCancellationRequested) {
                            // do task clean up here
                            observers.ForEach(x => x.OnCompleted());
                            IsSampling = false;
                            break;
                        }
                        // capture history
                        oldConditions = Conditions;

                        // read
                        Conditions = await Read();

                        // build a new result with the old and new conditions
                        result = new ChangeResult<UNIT>(Conditions, oldConditions);

                        // let everyone know
                        RaiseEventsAndNotify(result);

                        // sleep for the appropriate interval
                        await Task.Delay(updateInterval);
                    }
                }, SamplingTokenSource.Token);
            }
        }

        /// <summary>
        /// Stops sampling the temperature.
        /// </summary>
        public void StopUpdating()
        {
            lock (_lock) {
                if (!IsSampling) return;

                SamplingTokenSource?.Cancel();

                // state muh-cheen
                IsSampling = false;
            }
        }

    }
}
