using System;
namespace Meadow.Foundation.Sensors.Temperature
{
    public partial class Lm75
    {
        /// <summary>
        /// LM75 Registers
        /// </summary>
        enum Registers : byte
        {
            LM_TEMP = 0x00,
            LM_CONFIG = 0x01,
            LM_THYST = 0x02,
            LM_TOS = 0x03
        }
    }
}
