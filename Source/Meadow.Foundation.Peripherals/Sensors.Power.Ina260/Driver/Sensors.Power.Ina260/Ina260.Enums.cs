using Meadow.Hardware;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Power
{
    public partial class Ina260
    {
        private enum Register : byte
        {
            Config = 0x00,
            Current = 0x01,
            Voltage = 0x02,
            Power = 0x03,
            MaskEnable = 0x06,
            AlertLimit = 0x07,
            ManufacturerID = 0xFE,
            DieID = 0xFF
        }
    }
}
