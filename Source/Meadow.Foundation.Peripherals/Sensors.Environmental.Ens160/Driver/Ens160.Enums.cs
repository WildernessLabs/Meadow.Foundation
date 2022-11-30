namespace Meadow.Foundation.Sensors.Environmental
{
    partial class Ens160
    {
        /// <summary>
        /// Valid I2C addresses for the sensor
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x52
            /// ADDR is low
            /// </summary>
            Address_0x52 = 0x52,
            /// <summary>
            /// Bus address 0x53
            /// ADDR is high
            /// </summary>
            Address_0x53 = 0x53,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x52
        }

        /// <summary>
        /// Ens160 commands
        /// </summary>
        enum Registers : ushort
        {
            REG_PART_ID         = 0x00,		// 2 byte register
            REG_OPMODE			= 0x10,
            REG_CONFIG			= 0x11,
            REG_COMMAND			= 0x12,
            REG_TEMP_IN			= 0x13,
            REG_RH_IN			= 0x15,
            REG_DATA_STATUS		= 0x20,
            REG_DATA_AQI		= 0x21,
            REG_DATA_TVOC		= 0x22,
            REG_DATA_ECO2		= 0x24,			
            REG_DATA_BL			= 0x28,
            REG_DATA_T			= 0x30,
            REG_DATA_RH			= 0x32,
            REG_DATA_MISR		= 0x38,
            REG_GPR_WRITE_0		= 0x40,
            REG_GPR_WRITE_1     = 0x41,
            REG_GPR_WRITE_2		= 0x42,
            REG_GPR_WRITE_3		= 0x43,
            REG_GPR_WRITE_4		= 0x44,
            REG_GPR_WRITE_5		= 0x45,
            REG_GPR_WRITE_6		= 0x46,
            REG_GPR_WRITE_7		= 0x47,
            REG_GPR_READ_0		= 0x48,
            REG_GPR_READ_4      = 0x48 + 4,
            REG_GPR_READ_6      = 0x48 + 6,
            REG_GPR_READ_7      = 0x48 + 6,

            //ENS160 data register fields
            COMMAND_NOP			= 0x00,
            COMMAND_CLRGPR		= 0xCC,
            COMMAND_GET_APPVER	= 0x0E, 
            COMMAND_SETTH		= 0x02,
            COMMAND_SETSEQ		= 0xC2,

            OPMODE_RESET		= 0xF0,
            OPMODE_DEP_SLEEP    = 0x00,
            OPMODE_IDLE			= 0x01,
            OPMODE_STD			= 0x02,
            OPMODE_INTERMEDIATE	= 0x03,	
            OPMODE_CUSTOM		= 0xC0,
            OPMODE_D0			= 0xD0,
            OPMODE_D1			= 0xD1,
            OPMODE_BOOTLOADER	= 0xB0,

            BL_CMD_START		= 0x02,
            BL_CMD_ERASE_APP	= 0x04,
            BL_CMD_ERASE_BLINE	= 0x06,
            BL_CMD_WRITE		= 0x08,
            BL_CMD_VERIFY		= 0x0A,
            BL_CMD_GET_BLVER	= 0x0C,
            BL_CMD_GET_APPVER	= 0x0E,
            BL_CMD_EXITBL		= 0x12,

            SEQ_ACK_NOTCOMPLETE	= 0x80,
            SEQ_ACK_COMPLETE	= 0xC0,
        }
    }
}