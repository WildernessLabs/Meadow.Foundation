using System;
using System.Collections.Generic;
using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Bme680
    {
        public class TemperatureCompensation
        {
            public TemperatureCompensation(IReadOnlyList<byte> rawCompData)
            {
                T1 = (rawCompData[33]) | (rawCompData[34] << 8);
                T2 = (rawCompData[1]) | (rawCompData[2] << 8);
                T3 = (rawCompData[3]);
            }

            public int T1 { get; }
            public int T2 { get; }
            public int T3 { get; }

            public override string ToString()
            {
                return $"T1: {T1} T2: {T2} T3: {T3}";
            }
        }
    }
}