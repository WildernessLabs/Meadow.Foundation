using System;
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Apds9960
    {
        /// <summary>
        /// Gesture directions and distance
        /// </summary>
        public enum Direction
        {
            /// <summary>
            /// No direction detected
            /// </summary>
            NONE,
            /// <summary>
            /// Left
            /// </summary>
            LEFT,
            /// <summary>
            /// Right
            /// </summary>
            RIGHT,
            /// <summary>
            /// Up
            /// </summary>
            UP,
            /// <summary>
            /// Down
            /// </summary>
            DOWN,
            /// <summary>
            /// Near
            /// </summary>
            NEAR,
            /// <summary>
            /// Far
            /// </summary>
            FAR,
            /// <summary>
            /// All
            /// </summary>
            ALL
        };
    }
}