namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Lis3mdl
    {
        // See Datasheet for full details.
        /// <summary>
        /// Identification. Should always return 0x3D.
        /// </summary>
        private const byte WHO_AM_I = 0x0F;     
        /// <summary>
        /// TEMP_EN, OM1, OM0, DO2, DO1, DO0, FAST_ODR, ST
        /// </summary>
        private const byte CTRL_REG1 = 0x20;    
        /// <summary>
        /// 0, FS1, FS0, 0, REBOOT, SOFT_RST, 0, 0
        /// </summary>
        private const byte CTRL_REG2 = 0x21;    
        /// <summary>
        /// 0, 0, LP, 0, 0, SIM, MD1, MD0
        /// </summary>
        private const byte CTRL_REG3 = 0x22;    
        /// <summary>
        /// 0, 0, 0, 0, OMZ1, OMZ0, BLE, 0
        /// </summary>
        private const byte CTRL_REG4 = 0x23;    
        /// <summary>
        /// FAST_READ, BDU, 0, 0, 0, 0, 0, 0
        /// </summary>
        private const byte CTRL_REG5 = 0x24;    
        /// <summary>
        /// beginning of X, Y, and Z sensor outputs in signed 16 bit registers (little endian when BLE bit is 0 (default))
        /// </summary>
        private const byte OUT_X_L = 0x28;
        /// <summary>
        /// beginning of Temperature output in signed 16 bit register (little endian when BLE bit is 0 (default))
        /// </summary>
        private const byte TEMP_OUT_L = 0x2E;   
        /// <summary>
        /// Interrupt configuration: XIEN, YIEN, ZIEN, 0, 1, IEA, LIR, IEN
        /// </summary>
        private const byte INT_CFG = 0x30;      
        /// <summary>
        /// Interrupt source: PTH_X, PTH_Y, PTH_Z, NTH_X, NTH_Y, NTH_Z, MROI, INT
        /// </summary>
        private const byte INT_SRC = 0x31;
        /// <summary>
        /// beginning of Interrupt Threshold unsigned 15 bit register (little endian when BLE bit is 0 (default))
        /// </summary>
        private const byte INT_THS_L = 0x31;    
    }
}