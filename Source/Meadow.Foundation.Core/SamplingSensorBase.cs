using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation
{
    /// <summary>
    /// Base class that represents a sampling sensor to support the observable pattern
    /// </summary>
    /// <typeparam name="UNIT"></typeparam>
    public abstract class SamplingSensorBase<UNIT>
        : SensorBase<UNIT>
        where UNIT : struct
    {
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
        public virtual void StartUpdating(TimeSpan? updateInterval = null)
        {
            // thread safety
            lock (samplingLock) {
                if (IsSampling) return;

                IsSampling = true;

                // if an update interval has been passed in, override the default
                if(updateInterval is { } ui) { base.UpdateInterval = ui; }

                base.SamplingTokenSource = new CancellationTokenSource();
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
                        await Task.Delay(UpdateInterval);
                    }
                }, SamplingTokenSource.Token);
            }
        }

        /// <summary>
        /// Stops sampling the sensor.
        /// </summary>
        public virtual void StopUpdating()
        {
            lock (samplingLock) {
                if (!IsSampling) return;

                SamplingTokenSource?.Cancel();

                // state muh-cheen
                IsSampling = false;
            }
        }

    }
}
