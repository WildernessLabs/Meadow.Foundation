using System;

namespace Meadow.Foundation.Sensors.Rotary
{
    /// <summary>
    /// </summary>
    public class RotaryTurnedEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public RotationDirection Direction { get; set; }

        /// <summary>
        ///     Constructor for SensorVectorEventArgs objects.
        /// </summary>
        /// <param name="lastValue">Last sensor value sent through the eventing mechanism.</param>
        /// <param name="currentValue">Current sensor reading.</param>
        public RotaryTurnedEventArgs(RotationDirection direction)
        {
            Direction = direction;
        }
    }

    /// <summary>
    ///     Delegate for the events that will return a RotaryTurnedEventHandler object.
    /// </summary>
    /// <param name="sender">Object sending the notification.</param>
    /// <param name="e">RotaryTurnedEventArgs object containing the data for the application.</param>
    public delegate void RotaryTurnedEventHandler(object sender, RotaryTurnedEventArgs e);
}