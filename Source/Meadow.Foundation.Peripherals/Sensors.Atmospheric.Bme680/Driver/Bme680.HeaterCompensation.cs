using System.Collections.Generic;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Bme680
    {
        protected class HeaterCompensation
        {
            public HeaterCompensation(IReadOnlyList<byte> heaterCompData)
            {
                HeatValue = heaterCompData[0];

                HeatRange = (byte)((heaterCompData[2] & 0x30) >> 4);

                RangeSwError = (byte)((heaterCompData[4] & 0xF0) >> 4);
            }

            public byte HeatRange { get; } //0x04

            public byte RangeSwError { get; } //0x02

            public byte HeatValue { get; } //0x00
        }
    }
}