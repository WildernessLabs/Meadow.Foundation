using System;
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Apds9960
    {
        /* Direction definitions */
        public enum Direction
        {
            NONE,
            LEFT,
            RIGHT,
            UP,
            DOWN,
            NEAR,
            FAR,
            ALL //huh?
        };
    }
}