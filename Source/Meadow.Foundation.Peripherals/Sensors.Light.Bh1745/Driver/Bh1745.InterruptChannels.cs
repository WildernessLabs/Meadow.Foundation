using System;
namespace Meadow.Foundation.Sensors.Light
{
    public partial class Bh1745
    {
        public enum InterruptChannels : byte
        {
            Red = 0x0,
            Green = 0x1,
            Blue = 0x2,
            Clear = 0x3
        }
    }
}
