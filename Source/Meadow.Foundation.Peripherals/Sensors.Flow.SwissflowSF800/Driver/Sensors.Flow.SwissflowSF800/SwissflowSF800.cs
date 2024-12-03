using Meadow.Hardware;
using Meadow.Units;
using System;


namespace Meadow.Foundation.Sensors.Flow
{
    /// <summary>
    /// Driver to represent a Swissflow SF800 low pressure flow meter.
    /// http://www.swissflow.com/sf800.html
    /// </summary>
    /// <remarks>
    /// The SF800 is generally used in two different scenarios:
    /// 1. Signalling for intermittent flow and the total volume during the flow event.
    /// 2. Providing data on the flow rate of the liquid passing through the meter.
    /// 
    /// This implementation focuses on the first scenario for intermittent flow and metering volume.
    /// </remarks>
    public class SwissflowSF800
    {
        private const ushort _constIdleTime = 500;
        private const ushort _constKFactor = 5600;

        private BatchCounter _pulseCounter;
        private ushort _kFactor;

        /// <summary>
        /// Create a new instance of <see cref="SwissflowSf800"/>
        /// </summary>
        /// <param name="interruptPin">The pin where the flow meter is connected.</param>
        /// <param name="idleTime">The amount of time, in milliseconds, the meter should sit idle before assuming flow has stopped.</param>
        /// <param name="kFactor">The meter produces pulses as liquid is flowing. The KFactor determines the number of pulses per liter.</param>
        public SwissflowSF800(IPin interruptPin, ushort idleTime = _constIdleTime, ushort kFactor = _constKFactor)
        {
            _kFactor = kFactor;
            _pulseCounter = new BatchCounter(interruptPin, InterruptMode.EdgeFalling, TimeSpan.FromMilliseconds(idleTime));
            _pulseCounter.BatchStarted += PulseCounter_BatchStarted;
            _pulseCounter.BatchCompleted += PulseCounter_BatchCompleted;
        }

        private void PulseCounter_BatchCompleted(object sender, BatchCompletedEventArgs e)
        {
            double liters = e.Count / (float)_kFactor;
            Volume volume = new Volume(liters, Volume.UnitType.Liters);

            FlowStopped?.Invoke(this, new FlowStoppedEventArgs(DateTime.Now, volume));
        }

        private void PulseCounter_BatchStarted(object sender, BatchStartedEventArgs e)
        {
            FlowStarted?.Invoke(this, new FlowStartedEventArgs(DateTime.Now));
        }

        /// <summary>
        /// Event to signal that the flow has started.
        /// </summary>
        public event EventHandler<FlowStartedEventArgs> FlowStarted = default!;

        /// <summary>
        /// Event to signal that the flow has stopped.
        /// </summary>
        public event EventHandler<FlowStoppedEventArgs> FlowStopped = default!;
    }

    public class FlowEventArgs : EventArgs
    {
        public FlowEventArgs(DateTime eventTime)
        {
            EventTime = eventTime;
        }

        /// <summary>
        /// The time of the event.
        /// </summary>
        public DateTime EventTime { get; }
        
    }

    public class FlowStoppedEventArgs : FlowEventArgs
    {
        public FlowStoppedEventArgs(DateTime eventTime, Volume volume) : base(eventTime)
        {
            Volume = volume;
        }

        /// <summary>
        /// Volume of the flow occurrence originally measured in Liters
        /// </summary>
        public Volume Volume { get; }
    }

    public class FlowStartedEventArgs : FlowEventArgs
    {
        public FlowStartedEventArgs(DateTime eventTime) : base(eventTime)
        {
        }
    }
}
