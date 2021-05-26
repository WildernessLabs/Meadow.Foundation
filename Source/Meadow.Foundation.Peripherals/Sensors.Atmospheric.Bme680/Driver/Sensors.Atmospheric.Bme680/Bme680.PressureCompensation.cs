using System.Collections.Generic;
using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Bme680
    {
        public class PressureCompensation
        {
            public PressureCompensation(IReadOnlyList<byte> rawCompensationData)
            {
                P1 = rawCompensationData[5] | (rawCompensationData[6] << 8);
                P2 = rawCompensationData[7] | (rawCompensationData[8] << 8);
                P3 = rawCompensationData[9];
                P4 = rawCompensationData[11] | (rawCompensationData[12] << 8);
                P5 = rawCompensationData[13] | (rawCompensationData[14] << 8);
                // Yes, I know these are reversed, that's how the data sheet has them
                P6 = rawCompensationData[16];
                P7 = rawCompensationData[15];
                P8 = rawCompensationData[19] | (rawCompensationData[20] << 8);
                P9 = rawCompensationData[21] | (rawCompensationData[22] << 8);
                P10 = rawCompensationData[23];
            }

            public int P1 { get; }
            public int P2 { get; }
            public int P3 { get; }
            public int P4 { get; }
            public int P5 { get; }
            public int P6 { get; }
            public int P7 { get; }
            public int P8 { get; }
            public int P9 { get; }
            public int P10 { get; }

            public override string ToString()
            {
                return $"P1: {P1} P2: {P2} P3: {P3} P4: {P4} P5: {P5} P6: {P6} P7: {P7} P8: {P8} P9: {P9} P10: {P10}";
            }
        }
    }
}