using System;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Bme280
    {
        public enum ChipType : byte
        {
            BMP = 0x58,
            BME = 0x60
        }
    }
}