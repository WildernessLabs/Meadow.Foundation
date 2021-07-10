using System;
namespace Meadow.Foundation.Sensors.Light
{
	public partial class Bh1750
	{
        internal enum Commands : byte
        {
            PowerDown = 0b_0000_0000,
            PowerOn = 0b_0000_0001,
            Reset = 0b_0000_0111,
            MeasurementTimeHigh = 0b_0100_0000,
            MeasurementTimeLow = 0b_0110_0000,
        }
    }
}
