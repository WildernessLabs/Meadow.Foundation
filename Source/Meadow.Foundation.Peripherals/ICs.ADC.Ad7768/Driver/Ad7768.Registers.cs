namespace Meadow.Foundation.ICs.ADC
{
    public partial class Ad7768
    {
        internal enum Mask : byte
        {
            SleepMode = 1 << 7,
            MCLK = 0x03 << 0,
            CRC_SEL = 0x03 << 2,
            PWR_MODE = 0x03 << 5,
            DCLK_DIV = 0x03 << 0,
            OneShot = 1 << 4,
            DecimationRate = 0x07 << 0,
        }

        internal enum SleepMode : byte
        {
            Active = 0,
            Sleep = 1
        }

        internal enum PowerMode : byte
        {
            Eco = 0,
            Median = 2,
            Fast = 3
        }

        internal enum MCLKDivisor : byte
        {
            Div32 = 0,
            Div8 = 2,
            Div4 = 3
        }

        internal enum DCLKDivisor : byte
        {
            Div8 = 0,
            Div4 = 1,
            Div2 = 2,
            Div1 = 3,
        }

        internal enum ConversionType : byte
        {
            Standard = 0,
            OneShot = 1
        }

        internal enum ChannelState : byte
        {
            Enabled = 0,
            StandBy = 1
        }

        internal enum ChannelMode : byte
        {
            A = 0,
            B = 1
        }

        internal enum CrcSelection : byte
        {
            None = 0,
            CRC_4,
            CRC_16,
            CRC_16_2ND
        }

        internal enum FilterType : byte
        {
            Wideband,
            Sinc
        }

        internal enum DecimationRate : byte
        {
            X32,
            X64,
            X128,
            X256,
            X512,
            X1024,
            X1024_2ND,
            X1024_3RD
        }

        internal enum Registers : byte
        {
            CH_STANDBY = 0x00,
            CH_MODE_A = 0x01,
            CH_MODE_B = 0x02,
            CH_MODE_SEL = 0x03,
            PWR_MODE = 0x04,
            GENERAL_CFG = 0x05,
            DATA_CTRL = 0x06,
            INTERFACE_CFG = 0x07,
            BIST_CTRL = 0x08,
            DEV_STATUS = 0x09,
            REV_ID = 0x0A,
            DEV_ID_MSB = 0x0B,
            DEV_ID_LSB = 0x0C,
            SW_REV_ID = 0x0D,
            GPIO_CTRL = 0x0E,
            GPIO_WR_DATA = 0x0F,
            GPIO_RD_DATA = 0x10,
            PRECHARGE_BUF_1 = 0x11,
            PRECHARGE_BUF_2 = 0x12,
            POS_REF_BUF = 0x13,
            NEG_REF_BUF = 0x14,
            //CH_OFFSET_1(ch)		=	(0x1E + (ch) * 3)           ,
            //CH_OFFSET_2(ch)		=	(0x1F + (ch) * 3)           ,
            //CH_OFFSET_3(ch)		=	(0x20 + (ch) * 3)           ,
            //CH_GAIN_1(ch)		=	(0x36 + (ch) * 3)           ,
            //CH_GAIN_2(ch)		=	(0x37 + (ch) * 3)           ,
            //CH_GAIN_3(ch)		=	(0x38 + (ch) * 3)           ,
            //CH_SYNC_OFFSET(ch)	=	(0x4E + (ch) * 3)           ,
            DIAG_METER_RX = 0x56,
            DIAG_CTRL = 0x57,
            DIAG_MOD_DELAY_CTRL = 0x58,
            DIAG_CHOP_CTRL = 0x59,
        }
    }
}