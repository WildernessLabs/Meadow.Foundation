namespace Meadow.Foundation.Sensors.Hid
{
    public partial class Tsc2004
    {
        public enum Registers : byte
        {
             READ       = 0x01,
             PND0       = 0x02,
             X          = 0x0 << 3,
             Y          = 0x1 << 3,
             Z1         = 0x2 << 3,
             Z2         = 0x3 << 3,
             AUX        = 0x4 << 3,
             TEMP1      = 0x5 << 3,
             TEMP2      = 0x6 << 3,
             STATUS     = 0x7 << 3,
             AUX_HIGH   = 0x8 << 3,
             AUX_LOW    = 0x9 << 3,
             TEMP_HIGH  = 0xA << 3,
             TEMP_LOW   = 0xB << 3,
             CFR0       = 0xC << 3,
             CFR1       = 0xD << 3,
             CFR2       = 0xE << 3,
             CONV_FUNC  = 0xF << 3,
        }

        const ushort _MAX_12BIT = 0x0fff;
        const ushort _RESISTOR_VAL = 280;

        // Control Byte 1
        const byte CMD = (1 << 7);
        const byte CMD_NORMAL = (0x00);
        const byte CMD_STOP = (1 << 0);
        const byte CMD_RESET = (1 << 1);
        const byte CMD_12BIT = (1 << 2);

        // Config Register 0
        const ushort CFR0_PRECHARGE_20US     = (0x00 << 5);
        const ushort CFR0_PRECHARGE_84US     = (0x01 << 5);
        const ushort CFR0_PRECHARGE_276US    = (0x02 << 5);
        const ushort CFR0_PRECHARGE_340US    = (0x03 << 5);
        const ushort CFR0_PRECHARGE_1_044MS  = (0x04 << 5);
        const ushort CFR0_PRECHARGE_1_108MS  = (0x05 << 5);
        const ushort CFR0_PRECHARGE_1_300MS  = (0x06 << 5);
        const ushort CFR0_PRECHARGE_1_364MS  = (0x07 << 5);

        const ushort CFR0_STABTIME_0US       = (0x00 << 8);
        const ushort CFR0_STABTIME_100US     = (0x01 << 8);
        const ushort CFR0_STABTIME_500US     = (0x02 << 8);
        const ushort CFR0_STABTIME_1MS       = (0x03 << 8);
        const ushort CFR0_STABTIME_5MS       = (0x04 << 8);
        const ushort CFR0_STABTIME_10MS      = (0x05 << 8);
        const ushort CFR0_STABTIME_50MS      = (0x06 << 8);
        const ushort CFR0_STABTIME_100MS     = (0x07 << 8);

        const ushort CFR0_CLOCK_4MH0Z        = (0x00 << 11);
        const ushort CFR0_CLOCK_2MHZ         = (0x01 << 11);
        const ushort CFR0_CLOCK_1MHZ         = (0x02 << 11);
                                           
        const ushort CFR0_12BIT              = (1 << 13);
        const ushort CFR0_STATUS             = (1 << 14);
        const ushort CFR0_PENMODE            = (1 << 15);
                                           
        // Config Register 1               
        const byte CFR1_BATCHDELAY_0MS     = 0x00;
        const byte CFR1_BATCHDELAY_1MS     = 0x01;
        const byte CFR1_BATCHDELAY_2MS     = 0x02;
        const byte CFR1_BATCHDELAY_4MS     = 0x03;
        const byte CFR1_BATCHDELAY_10MS    = 0x04;
        const byte CFR1_BATCHDELAY_20MS    = 0x05;
        const byte CFR1_BATCHDELAY_40MS    = 0x06;
        const byte CFR1_BATCHDELAY_100MS   = 0x07;
        // Config Register 2               
        const ushort CFR2_MAVE_Z           = (1 << 2);
        const ushort CFR2_MAVE_Y           = (1 << 3);
        const ushort CFR2_MAVE_X           = (1 << 4);
        const ushort CFR2_AVG_7            = (0x01 << 11);
        const ushort CFR2_MEDIUM_15        = (0x03 << 12);
                                           
        const ushort STATUS_DAV_X            = 0x8000;
        const ushort STATUS_DAV_Y            = 0x4000;
        const ushort STATUS_DAV_Z1           = 0x2000;
        const ushort STATUS_DAV_Z2           = 0x1000;
        const ushort STATUS_DAV_MASK         = (STATUS_DAV_X | STATUS_DAV_Y | STATUS_DAV_Z1 | STATUS_DAV_Z2);
    }
}
