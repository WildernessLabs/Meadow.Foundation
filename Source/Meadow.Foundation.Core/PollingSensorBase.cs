﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation
{
    /// <summary>
    /// Base class that represents a sampling sensor to support the observable pattern
    /// </summary>
    /// <typeparam name="UNIT"></typeparam>
    public abstract class PollingSensorBase<UNIT>
        : SamplingSensorBase<UNIT>
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
        public override void StartUpdating(TimeSpan? updateInterval = null)
        {
            lock (samplingLock)
            {
                if (IsSampling && updateInterval == UpdateInterval) { return; }

                IsSampling = true;

                // if an update interval has been passed in, override the default
                if (updateInterval is { } ui) { base.UpdateInterval = ui; }

                base.SamplingTokenSource = new CancellationTokenSource();
                CancellationToken ct = SamplingTokenSource.Token;

                UNIT oldConditions;
                ChangeResult<UNIT> result;

                var t = new Task(async () =>
                {
                    while (true)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            observers.ForEach(x => x.OnCompleted());
                            IsSampling = false;
                            break;
                        }
                        oldConditions = Conditions;

                        Conditions = await Read();

                        result = new ChangeResult<UNIT>(Conditions, oldConditions);

                        RaiseEventsAndNotify(result);

                        await Task.Delay(UpdateInterval);
                    }
                }, SamplingTokenSource.Token, TaskCreationOptions.LongRunning);
                t.Start();
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

                SamplingTokenSource?.Cancel();

                IsSampling = false;
            }
        }
    }
}