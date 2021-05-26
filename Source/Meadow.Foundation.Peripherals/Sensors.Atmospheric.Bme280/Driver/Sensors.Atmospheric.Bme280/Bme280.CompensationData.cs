using System;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Bme280
    {
        /// <summary>
        ///     Compensation data.
        /// </summary>
        protected struct CompensationData
        {
            public ushort T1;
            public short T2;
            public short T3;
            public ushort P1;
            public short P2;
            public short P3;
            public short P4;
            public short P5;
            public short P6;
            public short P7;
            public short P8;
            public short P9;
            public byte H1;
            public short H2;
            public byte H3;
            public short H4;
            public short H5;
            public sbyte H6;
        }
    }
}
