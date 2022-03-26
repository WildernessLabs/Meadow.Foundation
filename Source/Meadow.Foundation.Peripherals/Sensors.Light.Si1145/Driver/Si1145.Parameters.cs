using System;
namespace Meadow.Foundation.Sensors.Light
{
    public partial class Si1145
    {
        protected static class Parameters
        {
            /* Parameters */
            public static readonly byte PARAM_I2CADDR = 0x00;
            public static readonly byte PARAM_CHLIST = 0x01;
            public static readonly byte PARAM_CHLIST_ENUV = 0x80;
            public static readonly byte PARAM_CHLIST_ENAUX = 0x40;
            public static readonly byte PARAM_CHLIST_ENALSIR = 0x20;
            public static readonly byte PARAM_CHLIST_ENALSVIS = 0x10;
            public static readonly byte PARAM_CHLIST_ENPS1 = 0x01;
            public static readonly byte PARAM_CHLIST_ENPS2 = 0x02;
            public static readonly byte PARAM_CHLIST_ENPS3 = 0x04;

            public static readonly byte PARAM_PSLED12SEL = 0x02;
            public static readonly byte PARAM_PSLED12SEL_PS2NONE = 0x00;
            public static readonly byte PARAM_PSLED12SEL_PS2LED1 = 0x10;
            public static readonly byte PARAM_PSLED12SEL_PS2LED2 = 0x20;
            public static readonly byte PARAM_PSLED12SEL_PS2LED3 = 0x40;
            public static readonly byte PARAM_PSLED12SEL_PS1NONE = 0x00;
            public static readonly byte PARAM_PSLED12SEL_PS1LED1 = 0x01;
            public static readonly byte PARAM_PSLED12SEL_PS1LED2 = 0x02;
            public static readonly byte PARAM_PSLED12SEL_PS1LED3 = 0x04;

            public static readonly byte PARAM_PSLED3SEL = 0x03;
            public static readonly byte PARAM_PSENCODE = 0x05;
            public static readonly byte PARAM_ALSENCODE = 0x06;

            public static readonly byte PARAM_PS1ADCMUX = 0x07;
            public static readonly byte PARAM_PS2ADCMUX = 0x08;
            public static readonly byte PARAM_PS3ADCMUX = 0x09;
            public static readonly byte PARAM_PSADCOUNTER = 0x0A;
            public static readonly byte PARAM_PSADCGAIN = 0x0B;
            public static readonly byte PARAM_PSADCMISC = 0x0C;
            public static readonly byte PARAM_PSADCMISC_RANGE = 0x20;
            public static readonly byte PARAM_PSADCMISC_PSMODE = 0x04;

            public static readonly byte PARAM_ALSIRADCMUX = 0x0E;
            public static readonly byte PARAM_AUXADCMUX = 0x0F;

            public static readonly byte PARAM_ALSVISADCOUNTER = 0x10;
            public static readonly byte PARAM_ALSVISADCGAIN = 0x11;
            public static readonly byte PARAM_ALSVISADCMISC = 0x12;
            public static readonly byte PARAM_ALSVISADCMISC_VISRANGE = 0x20;

            public static readonly byte PARAM_ALSIRADCOUNTER = 0x1D;
            public static readonly byte PARAM_ALSIRADCGAIN = 0x1E;
            public static readonly byte PARAM_ALSIRADCMISC = 0x1F;
            public static readonly byte PARAM_ALSIRADCMISC_RANGE = 0x20;

            public static readonly byte PARAM_ADCCOUNTER_511CLK = 0x70;

            public static readonly byte PARAM_ADCMUX_SMALLIR = 0x00;
            public static readonly byte PARAM_ADCMUX_LARGEIR = 0x03;
        }
    }
}
