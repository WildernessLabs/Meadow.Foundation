using System;

namespace Netduino.Foundation.Sensors.Distance
{
    public class DistanceEventArgs : EventArgs
    {
        public float Distance { get; set; }

        public DistanceEventArgs(float distance)
        {
            Distance = distance;
        }
    }

    public delegate void DistanceDetectedEventHandler(object sender, DistanceEventArgs e);
}