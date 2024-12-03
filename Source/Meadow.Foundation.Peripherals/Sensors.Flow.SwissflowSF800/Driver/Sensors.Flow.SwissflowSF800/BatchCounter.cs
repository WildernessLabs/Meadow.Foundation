using System;
using System.Threading;

namespace Meadow.Hardware
{

    public class BatchCounter : Counter
    {
        private readonly TimeSpan _idleTime;
        private readonly Timer _pollingTimer;
        private ulong _lastCount = 0;

        /// <summary>
        /// Create a new instance of <see cref="BatchCounter"/>
        /// </summary>
        /// <param name="pin">The pin to watch for interrupts.</param>
        /// <param name="edge">The edge that singles a count.</param>
        /// <param name="idleTime">
        /// The amount of time that needs to elapse without a count change in
        /// order to assume that the counting has stopped.
        /// </param>
        public BatchCounter(IPin pin, InterruptMode edge, TimeSpan idleTime) : base(pin, edge)
        {
            _idleTime = idleTime;
            // create a timer to watch for changes in the count.
            _pollingTimer = new Timer(TimerTick, this, idleTime, idleTime);
            Enabled = true;
        }

        private void TimerTick(object state)
        {
            bool isStarting = _lastCount == 0 && Count > 0;
            bool isStopping = _lastCount > 0 && Count == _lastCount;
            bool isIdle = _lastCount == 0 && Count == 0;

            // the count has started.
            if (isStarting)
            {
                Resolver.Log.Debug($"Started {Count}");
                _lastCount = Count;
                BatchStarted?.Invoke(this, new BatchStartedEventArgs());
                return;
            }
            else
            // the count has stopped.
            if (isStopping)
            {
                Resolver.Log.Debug($"Stopped {Count}");
                _lastCount = 0;
                BatchCompleted?.Invoke(this, new BatchCompletedEventArgs(Count));
                Reset();
                return;
            }
            else
            // the counter is sitting idle.
            if (isIdle)
            {
                return;
            }
            // the counter is running.
            else
            {
                Resolver.Log.Debug($"Running {Count}");
                _lastCount = Count;
            }
        }

        /// <summary>
        /// Signaled when a counting batch has completed.
        /// </summary>
        public event EventHandler<BatchCompletedEventArgs> BatchCompleted = default!;

        /// <summary>
        /// Signaled when a counting batch has started.
        /// </summary>
        public event EventHandler<BatchStartedEventArgs> BatchStarted = default!;
    }

    public class BatchStartedEventArgs
    {
    }

    public class BatchCompletedEventArgs
    {
        public BatchCompletedEventArgs(ulong count)
        {
            Count = count;
        }

        public ulong Count { get; }
    }
}