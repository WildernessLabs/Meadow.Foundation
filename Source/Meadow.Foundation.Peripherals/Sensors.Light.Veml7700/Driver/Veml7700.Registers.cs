using System;

namespace Meadow.Foundation.Sensors.Light
{
    public partial class Veml7700
    {
        [Flags]
        private enum Registers : byte
        {
            AlsConf0 = 0x00,
            AlsWH = 0x01,
            AlsWL = 0x02,
            PowerSaving = 0x03,
            Als = 0x04,
            White = 0x05,
            AlsInt = 0x06
        }
    }
}
