using Meadow.Foundation.Spatial;
using System;

namespace Meadow.Foundation.Sensors
{
    /// <summary>
    ///     Class to be used when sending Vector sensor data to the application
    ///     through the eventing mechanism.
    /// </summary>
    public class SensorVectorEventArgs : EventArgs
    {
        /// <summary>
        ///     Last value read from the sensor AND sent to the user in a notification.
        /// </summary>
        public Vector LastNotifiedValue { get; set; }

        /// <summary>
        ///     Current sensor reading.
        /// </summary>
        public Vector CurrentValue { get; set; }

        /// <summary>
        ///     Constructor for SensorVectorEventArgs objects.
        /// </summary>
        /// <param name="lastValue">Last sensor value sent through the eventing mechanism.</param>
        /// <param name="currentValue">Current sensor reading.</param>
        public SensorVectorEventArgs (Vector lastValue, Vector currentValue)
        {
            LastNotifiedValue = lastValue;
            CurrentValue = currentValue;
        }
    }

    /// <summary>
    ///     Delegate for the events that will return a SensorVectorEventArgs object.
    /// </summary>
    /// <param name="sender">Object sending the notification.</param>
    /// <param name="e">SensorVectorEventArgs object containing the data for the application.</param>
    public delegate void SensorVectorEventHandler(object sender, SensorVectorEventArgs e);
}