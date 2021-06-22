using System;
namespace Meadow.Foundation.Sensors.Light
{
    public partial class Si1145
    {
        protected static class Registers
        {
            /* REGISTERS */
            public static readonly byte REG_PARTID = 0x00;
            public static readonly byte REG_REVID = 0x01;
            public static readonly byte REG_SEQID = 0x02;
            public static readonly byte REG_INTCFG = 0x03;
            public static readonly byte REG_INTCFG_INTOE = 0x01;
            public static readonly byte REG_INTCFG_INTMODE = 0x02;
            public static readonly byte REG_IRQEN = 0x04;
            public static readonly byte REG_IRQEN_ALSEVERYSAMPLE = 0x01;
            public static readonly byte REG_IRQEN_PS1EVERYSAMPLE = 0x04;
            public static readonly byte REG_IRQEN_PS2EVERYSAMPLE = 0x08;
            public static readonly byte REG_IRQEN_PS3EVERYSAMPLE = 0x10;

            public static readonly byte REG_IRQMODE1 = 0x05;
            public static readonly byte REG_IRQMODE2 = 0x06;

            public static readonly byte REG_HWKEY = 0x07;
            public static readonly byte REG_MEASRATE0 = 0x08;
            public static readonly byte REG_MEASRATE1 = 0x09;
            public static readonly byte REG_PSRATE = 0x0A;
            public static readonly byte REG_PSLED21 = 0x0F;
            public static readonly byte REG_PSLED3 = 0x10;
            public static readonly byte REG_UCOEFF0 = 0x13;
            public static readonly byte REG_UCOEFF1 = 0x14;
            public static readonly byte REG_UCOEFF2 = 0x15;
            public static readonly byte REG_UCOEFF3 = 0x16;
            public static readonly byte REG_PARAMWR = 0x17;
            public static readonly byte REG_COMMAND = 0x18;
            public static readonly byte REG_RESPONSE = 0x20;
            public static readonly byte REG_IRQSTAT = 0x21;
            public static readonly byte REG_IRQSTAT_ALS = 0x01;

            public static readonly byte REG_ALSVISDATA0 = 0x22;
            public static readonly byte REG_ALSVISDATA1 = 0x23;
            public static readonly byte REG_ALSIRDATA0 = 0x24;
            public static readonly byte REG_ALSIRDATA1 = 0x25;
            public static readonly byte REG_PS1DATA0 = 0x26;
            public static readonly byte REG_PS1DATA1 = 0x27;
            public static readonly byte REG_PS2DATA0 = 0x28;
            public static readonly byte REG_PS2DATA1 = 0x29;
            public static readonly byte REG_PS3DATA0 = 0x2A;
            public static readonly byte REG_PS3DATA1 = 0x2B;
            public static readonly byte REG_UVINDEX0 = 0x2C;
            public static readonly byte REG_UVINDEX1 = 0x2D;
            public static readonly byte REG_PARAMRD = 0x2E;
            public static readonly byte REG_CHIPSTAT = 0x30;
        }
    }
}
