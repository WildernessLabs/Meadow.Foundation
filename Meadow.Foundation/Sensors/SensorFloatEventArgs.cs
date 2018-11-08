using System;

namespace Meadow.Foundation.Sensors
{
    /// <summary>
    ///     Class to be used when sending floating point sensor data to the application
    ///     through the eventing mechanism.
    /// </summary>
    public class SensorFloatEventArgs : EventArgs
    {
        /// <summary>
        ///     Last value read from the sensor AND sent to the user in a notification.
        /// </summary>
        public float LastNotifiedValue { get; private set; }

        /// <summary>
        ///     Current sensor reading.
        /// </summary>
        public float CurrentValue { get; private set; }

        /// <summary>
        ///     Constructor for SensorFloatEventArgs objects.
        /// </summary>
        /// <param name="lastValue">Last sensor value sent through the eventing mechanism.</param>
        /// <param name="currentValue">Current sensor reading.</param>
        public SensorFloatEventArgs (float lastValue, float currentValue)
        {
            LastNotifiedValue = lastValue;
            CurrentValue = currentValue;
        }
    }

    /// <summary>
    ///     Delegate for the events that will return a SensorFloatEventArgs object.
    /// </summary>
    /// <param name="sender">Object sending the notification.</param>
    /// <param name="e">SensorFloatEventArgs object containing the data for the application.</param>
    public delegate void SensorFloatEventHandler(object sender, SensorFloatEventArgs e);
}
