namespace Meadow.Foundation.Sensors.Atmospheric
{
    partial class Bme68x
    {
        /// <summary>
        /// Control registers for the BME68x
        /// </summary>
        internal enum Registers : byte
        {
            RESET = 0xE0,
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