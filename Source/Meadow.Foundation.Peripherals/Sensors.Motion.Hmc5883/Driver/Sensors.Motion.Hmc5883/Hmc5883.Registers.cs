using System;
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Hmc5883
    {
        /// <summary>
        /// Register of HMC5883L
        /// </summary>
        public static class Registers
        {
            public const byte HMC_CONFIG_REG_A_ADDR = 0x00;
            public const byte HMC_CONFIG_REG_B_ADDR = 0x01;
            public const byte HMC_MODE_REG_ADDR = 0x02;
            public const byte HMC_X_MSB_REG_ADDR = 0x03;
            public const byte HMC_Z_MSB_REG_ADDR = 0x05;
            public const byte HMC_Y_MSB_REG_ADDR = 0x07;
            public const byte HMC_STATUS_REG_ADDR = 0x09;
        }
    }
}
