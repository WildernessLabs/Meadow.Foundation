namespace Meadow.Foundation.Sensors.Atmospheric
{
    partial class Bme68x
    {
        /// <summary>
        /// Control registers for the BME68x
        /// </summary>
        internal enum Registers : byte
        {
            RESET = 0xE1,

            H1_LSB = 0xE2,
            H1_MSB = 0xE3,
            H2_LSB = 0xE2,
            H2_MSB = 0xE1,
            H3 = 0xE4,
            H4 = 0xE5,
            H5 = 0xE6,
            H6 = 0xE7,
            H7 = 0xE8,

            T1 = 0xE9,
            T2 = 0x8A,
            T3 = 0x8C,

            P1_LSB = 0x8E,
            P2_LSB = 0x90,
            P3 = 0x92,
            P4_LSB = 0x94,
            P5_LSB = 0x96,
            P6 = 0x99,
            P7 = 0x98,
            P8_LSB = 0x9C,
            P9_LSB = 0x9E,
            P10 = 0xA0,

            GH1 = 0xED,
            GH2 = 0xEB,
            GH3 = 0xEE,

            RES_HEAT_VAL = 0x00,
            RES_HEAT_RANGE = 0x02,
            RANGE_SW_ERR = 0x04,
            STATUS = 0x1D,

            PRESSUREDATA = 0x1F,
            TEMPDATA = 0x22,
            HUMIDITYDATA = 0x25,

            GAS_RES = 0x2C,
            GAS_RANGE = 0x2D,
            RES_HEAT_0 = 0x5A,
            GAS_WAIT_0 = 0x64,

            CTRL_GAS_0 = 0x70,
            CTRL_GAS_1 = 0x71,
            CTRL_HUM = 0x72,
            CTRL_MEAS = 0x74,
            CONFIG = 0x75
        }
    }
}