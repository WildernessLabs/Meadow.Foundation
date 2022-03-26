using System;
namespace Meadow.Foundation.Sensors.Light
{
    public partial class Bh1745
    {
        public enum LatchBehaviorTypes : byte
        {
            LatchUntilReadOrInitialized = 0,
            LatchEachMeasurement = 1
        }
    }
}
