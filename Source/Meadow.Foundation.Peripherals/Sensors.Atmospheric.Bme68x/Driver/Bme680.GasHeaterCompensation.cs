using System.Collections.Generic;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Bme680
    {
        protected class GasHeaterCompensation
        {
            public GasHeaterCompensation(IReadOnlyList<byte> rawCompData)
            {
                Gh1 = rawCompData[37];//35
                Gh2 = rawCompData[34] | (rawCompData[35] << 8); //32,33
                Gh3 = rawCompData[38];//36
            }

            public int Gh1 { get; }
            public int Gh2 { get; }
            public int Gh3 { get; }

            public override string ToString()
            {
                return $"Gh1: {Gh1} Gh2: {Gh2} Gh3: {Gh3}";
            }
        }
    }
}