using System;
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Apds9960
    {
        public static class Registers
        {
            /* APDS-9960 register addresses */
            public const byte APDS9960_ENABLE = 0x80;
            public const byte APDS9960_ATIME = 0x81;
            public const byte APDS9960_WTIME = 0x83;
            public const byte APDS9960_AILTL = 0x84;
            public const byte APDS9960_AILTH = 0x85;
            public const byte APDS9960_AIHTL = 0x86;
            public const byte APDS9960_AIHTH = 0x87;
            public const byte APDS9960_PILT = 0x89;
            public const byte APDS9960_PIHT = 0x8B;
            public const byte APDS9960_PERS = 0x8C;
            public const byte APDS9960_CONFIG1 = 0x8D;
            public const byte APDS9960_PPULSE = 0x8E;
            public const byte APDS9960_CONTROL = 0x8F;
            public const byte APDS9960_CONFIG2 = 0x90;
            public const byte APDS9960_ID = 0x92;
            public const byte APDS9960_STATUS = 0x93;
            public const byte APDS9960_CDATAL = 0x94;
            public const byte APDS9960_CDATAH = 0x95;
            public const byte APDS9960_RDATAL = 0x96;
            public const byte APDS9960_RDATAH = 0x97;
            public const byte APDS9960_GDATAL = 0x98;
            public const byte APDS9960_GDATAH = 0x99;
            public const byte APDS9960_BDATAL = 0x9A;
            public const byte APDS9960_BDATAH = 0x9B;
            public const byte APDS9960_PDATA = 0x9C;
            public const byte APDS9960_POFFSET_UR = 0x9D;
            public const byte APDS9960_POFFSET_DL = 0x9E;
            public const byte APDS9960_CONFIG3 = 0x9F;
            public const byte APDS9960_GPENTH = 0xA0;
            public const byte APDS9960_GEXTH = 0xA1;
            public const byte APDS9960_GCONF1 = 0xA2;
            public const byte APDS9960_GCONF2 = 0xA3;
            public const byte APDS9960_GOFFSET_U = 0xA4;
            public const byte APDS9960_GOFFSET_D = 0xA5;
            public const byte APDS9960_GOFFSET_L = 0xA7;
            public const byte APDS9960_GOFFSET_R = 0xA9;
            public const byte APDS9960_GPULSE = 0xA6;
            public const byte APDS9960_GCONF3 = 0xAA;
            public const byte APDS9960_GCONF4 = 0xAB;
            public const byte APDS9960_GFLVL = 0xAE;
            public const byte APDS9960_GSTATUS = 0xAF;
            public const byte APDS9960_IFORCE = 0xE4;
            public const byte APDS9960_PICLEAR = 0xE5;
            public const byte APDS9960_CICLEAR = 0xE6;
            public const byte APDS9960_AICLEAR = 0xE7;
            public const byte APDS9960_GFIFO_U = 0xFC;
            public const byte APDS9960_GFIFO_D = 0xFD;
            public const byte APDS9960_GFIFO_L = 0xFE;
            public const byte APDS9960_GFIFO_R = 0xFF;
        }
    }
}