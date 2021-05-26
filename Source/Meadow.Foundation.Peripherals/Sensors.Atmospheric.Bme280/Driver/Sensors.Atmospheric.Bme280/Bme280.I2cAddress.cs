using System;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Bme280
    {
        public enum I2cAddress : byte
        {
            Adddress0x76 = 0x76,
            Adddress0x77 = 0x77
        }
    }
}
