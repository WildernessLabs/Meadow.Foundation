using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Bme680
    {
        internal class HumidityCompensation
        {
            public HumidityCompensation(byte[] rawCompData1)
            {
                H1 = (rawCompData1[26] & 0x0f) | (rawCompData1[27] << 4);
                H2 = (rawCompData1[26] & 0x0f) | (rawCompData1[25] << 4);
                H3 = rawCompData1[28];
                H4 = rawCompData1[29];
                H5 = rawCompData1[30];
                H6 = rawCompData1[31];
                H7 = rawCompData1[32];
            }
            public int H1 { get; }
            public int H2 { get; }
            public int H3 { get; }
            public int H4 { get; }
            public int H5 { get; }
            public int H6 { get; }
            public int H7 { get; }

            public override string ToString()
            {
                return $"H1: {H1} H2: {H2} H3: {H3} H4: {H4} H5: {H5} H6: {H6} H7: {H7}";
            }
        }
    }
}