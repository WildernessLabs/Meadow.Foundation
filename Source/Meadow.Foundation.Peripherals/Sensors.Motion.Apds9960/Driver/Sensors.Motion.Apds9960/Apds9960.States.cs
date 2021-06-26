using System;
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Apds9960
    {
        /* State definitions */
        public enum States
        {
            NA_STATE,
            NEAR_STATE,
            FAR_STATE,
            ALL_STATE
        };
    }
}