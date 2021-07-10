using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Sensors.HallEffect
{
    /// <summary>
    /// Allows use of Hall effect sensor for counting magnetic field changes,
    /// For example for counting wheel rotations to get distance traveled,
    /// Or magnetic field blocking objects passing between sensor and magnet
    /// </summary>
    class HallEffectCounter
    {
        IDigitalInputPort port;
        private int counter = 0;
        //-1 so it wont fire LimitReached unnecessary
        private int targetCount = -1;
        //Allows to get current count since last reset
        public int Counter { get => counter; }

        EventHandler LimitReached;
        EventHandler<int> CountChanged;

        public HallEffectCounter(IDigitalInputPort p)
        {
            port = p;
            port.Changed += Port_Changed;
        }

        /// <summary>
        /// Fires apropriate events
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Args</param>
        private void Port_Changed(object sender, DigitalInputPortEventArgs e)
        {
            counter++;
            if (CountChanged != null) OnCountChanged();
            if (counter == targetCount && LimitReached != null) OnLimitReached();
        }

        /// <summary>
        /// Resets counter
        /// </summary>
        private void ResetCount()
        {
            counter = 0;
        }

        /// <summary>
        /// Fires when count changes, informs about current count
        /// </summary>
        private void OnCountChanged()
        {
            CountChanged?.Invoke(this, counter);
        }

        /// <summary>
        /// Fires when count reaches preset target
        /// </summary>
        private void OnLimitReached()
        {
            LimitReached?.Invoke(this, null);
        }

        /// <summary>
        /// Sets target to -1 so OnLimitReached won't be invoked
        /// </summary>
        public void DisableTarget()
        {
            targetCount = -1;
        }

        /// <summary>
        /// Sets new target and resets counter to start counting towards it
        /// </summary>
        /// <param name="target">Count to reach</param>
        public void SetTarget(int target)
        {
            ResetCount();
            targetCount = target;
        }

        /// <summary>
        /// Allows other classes to register methods to be invoked when count changes 
        /// </summary>
        /// <param name="handler">Method to be invoked when count changes</param>
        public void RegisterForCount(EventHandler<int> handler)
        {
            CountChanged += handler;
        }

        /// <summary>
        /// Allows other classes to register methods to be invoked when target count is reached
        /// </summary>
        /// <param name="handler">Method to be invoked when target count reached</param>
        public void RegisterForLimitReached(EventHandler handler)
        {
            LimitReached += handler;
        }

        /// <summary>
        /// Allows other classes to register methods to be invoked when target count is reached.
        /// Allows also to pass new target count at the same time
        /// </summary>
        /// <param name="handler">Method to be invoked when target count reached</param>
        /// <param name="target">Count to reach</param>
        public void RegisterForLimitReached(EventHandler handler, int target)
        {
            SetTarget(target);
            LimitReached += handler;
        }
    }
}
