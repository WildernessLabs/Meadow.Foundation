using System;
namespace Meadow.Foundation.Sensors.Light
{
    public partial class Bh1745
    {
        public enum InterruptTypes : byte
        {
            ToggleMeasurementEnd = 0x0,
            UpdateMeasurementEnd = 0x1,
            UpdateConsecutiveX4 = 0x2,
            UpdateConsecutiveX8 = 0x3
        }
    }
}
