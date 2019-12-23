using System;
using System.Threading;
using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Light
{
    public class Si1145
    {
        #region Member variables and fields

        /// <summary>
        ///     Command bus object used to communicate with the SI1145 sensor.
        /// </summary>
        private II2cPeripheral si1145;

        #endregion Member variables and fields

        #region Fields / Member variables

        /* COMMANDS*/
        readonly byte PARAM_QUERY = 0x80;
        readonly byte PARAM_SET = 0xA0;
        readonly byte NOP = 0x00;
        readonly byte RESET = 0x01;
        readonly byte BUSADDR = 0x02;
        readonly byte PS_FORCE = 0x05;
        readonly byte ALS_FORCE = 0x06;
        readonly byte PSALS_FORCE = 0x07;
        readonly byte PS_PAUSE = 0x09;
        readonly byte ALS_PAUSE = 0x0A;
        readonly byte PSALS_PAUSE = 0xB;
        readonly byte PS_AUTO = 0x0D;
        readonly byte ALS_AUTO = 0x0E;
        readonly byte PSALS_AUTO = 0x0F;
        readonly byte GET_CAL = 0x12;

        /* Parameters */
        readonly byte PARAM_I2CADDR = 0x00;
        readonly byte PARAM_CHLIST = 0x01;
        readonly byte PARAM_CHLIST_ENUV = 0x80;
        readonly byte PARAM_CHLIST_ENAUX = 0x40;
        readonly byte PARAM_CHLIST_ENALSIR = 0x20;
        readonly byte PARAM_CHLIST_ENALSVIS = 0x10;
        readonly byte PARAM_CHLIST_ENPS1 = 0x01;
        readonly byte PARAM_CHLIST_ENPS2 = 0x02;
        readonly byte PARAM_CHLIST_ENPS3 = 0x04;

        readonly byte PARAM_PSLED12SEL = 0x02;
        readonly byte PARAM_PSLED12SEL_PS2NONE = 0x00;
        readonly byte PARAM_PSLED12SEL_PS2LED1 = 0x10;
        readonly byte PARAM_PSLED12SEL_PS2LED2 = 0x20;
        readonly byte PARAM_PSLED12SEL_PS2LED3 = 0x40;
        readonly byte PARAM_PSLED12SEL_PS1NONE = 0x00;
        readonly byte PARAM_PSLED12SEL_PS1LED1 = 0x01;
        readonly byte PARAM_PSLED12SEL_PS1LED2 = 0x02;
        readonly byte PARAM_PSLED12SEL_PS1LED3 = 0x04;

        readonly byte PARAM_PSLED3SEL = 0x03;
        readonly byte PARAM_PSENCODE = 0x05;
        readonly byte PARAM_ALSENCODE = 0x06;

        readonly byte PARAM_PS1ADCMUX = 0x07;
        readonly byte PARAM_PS2ADCMUX = 0x08;
        readonly byte PARAM_PS3ADCMUX = 0x09;
        readonly byte PARAM_PSADCOUNTER = 0x0A;
        readonly byte PARAM_PSADCGAIN = 0x0B;
        readonly byte PARAM_PSADCMISC = 0x0C;
        readonly byte PARAM_PSADCMISC_RANGE = 0x20;
        readonly byte PARAM_PSADCMISC_PSMODE = 0x04;

        readonly byte PARAM_ALSIRADCMUX = 0x0E;
        readonly byte PARAM_AUXADCMUX = 0x0F;

        readonly byte PARAM_ALSVISADCOUNTER = 0x10;
        readonly byte PARAM_ALSVISADCGAIN = 0x11;
        readonly byte PARAM_ALSVISADCMISC = 0x12;
        readonly byte PARAM_ALSVISADCMISC_VISRANGE = 0x20;

        readonly byte PARAM_ALSIRADCOUNTER = 0x1D;
        readonly byte PARAM_ALSIRADCGAIN = 0x1E;
        readonly byte PARAM_ALSIRADCMISC = 0x1F;
        readonly byte PARAM_ALSIRADCMISC_RANGE = 0x20;

        readonly byte PARAM_ADCCOUNTER_511CLK = 0x70;

        readonly byte PARAM_ADCMUX_SMALLIR = 0x00;
        readonly byte PARAM_ADCMUX_LARGEIR = 0x03;


        /* REGISTERS */
        readonly byte REG_PARTID = 0x00;
        readonly byte REG_REVID = 0x01;
        readonly byte REG_SEQID = 0x02;
        readonly byte REG_INTCFG = 0x03;
        readonly byte REG_INTCFG_INTOE = 0x01;
        readonly byte REG_INTCFG_INTMODE = 0x02;
        readonly byte REG_IRQEN = 0x04;
        readonly byte REG_IRQEN_ALSEVERYSAMPLE = 0x01;
        readonly byte REG_IRQEN_PS1EVERYSAMPLE = 0x04;
        readonly byte REG_IRQEN_PS2EVERYSAMPLE = 0x08;
        readonly byte REG_IRQEN_PS3EVERYSAMPLE = 0x10;

        readonly byte REG_IRQMODE1 = 0x05;
        readonly byte REG_IRQMODE2 = 0x06;

        readonly byte REG_HWKEY = 0x07;
        readonly byte REG_MEASRATE0 = 0x08;
        readonly byte REG_MEASRATE1 = 0x09;
        readonly byte REG_PSRATE = 0x0A;
        readonly byte REG_PSLED21 = 0x0F;
        readonly byte REG_PSLED3 = 0x10;
        readonly byte REG_UCOEFF0 = 0x13;
        readonly byte REG_UCOEFF1 = 0x14;
        readonly byte REG_UCOEFF2 = 0x15;
        readonly byte REG_UCOEFF3 = 0x16;
        readonly byte REG_PARAMWR = 0x17;
        readonly byte REG_COMMAND = 0x18;
        readonly byte REG_RESPONSE = 0x20;
        readonly byte REG_IRQSTAT = 0x21;
        readonly byte REG_IRQSTAT_ALS = 0x01;

        readonly byte REG_ALSVISDATA0 = 0x22;
        readonly byte REG_ALSVISDATA1 = 0x23;
        readonly byte REG_ALSIRDATA0 = 0x24;
        readonly byte REG_ALSIRDATA1 = 0x25;
        readonly byte REG_PS1DATA0 = 0x26;
        readonly byte REG_PS1DATA1 = 0x27;
        readonly byte REG_PS2DATA0 = 0x28;
        readonly byte REG_PS2DATA1 = 0x29;
        readonly byte REG_PS3DATA0 = 0x2A;
        readonly byte REG_PS3DATA1 = 0x2B;
        readonly byte REG_UVINDEX0 = 0x2C;
        readonly byte REG_UVINDEX1 = 0x2D;
        readonly byte REG_PARAMRD = 0x2E;
        readonly byte REG_CHIPSTAT = 0x30;


        #endregion Fields / Member variables

        #region Constructors

        /// <summary>
        ///     Default constructor (private to prevent this being called).
        /// </summary>
        private Si1145() { }

        /// <summary>
        ///     Create a new SI1145 sensor object.
        /// </summary>
        /// <param name="address">Address of the chip on the I2C bus (default to 0x60).</param>
        /// <param name="i2cBus">I2cBus (default to 400 KHz).</param>
        public Si1145(II2cBus i2cBus, byte address = 0x60)
        {
            si1145 = new I2cPeripheral(i2cBus, address);

            if (si1145.ReadRegister(REG_PARTID) != 0x45)
            {
                throw new Exception("Invalid part ID");
            }

            Console.WriteLine("Found Si1145");

            Initialize();
        }

        #endregion Constructors

        #region Methods

        public double GetUltraViolet()
        {
            byte[] data = si1145.ReadRegisters(0x2C, 2);
            int result = (data[1] << 8) | data[0];
            return result / 100.0;
        }

        public double GetIfrared()
        {
            byte[] data = si1145.ReadRegisters(0x22, 2);
            int result = (data[1] << 8) | data[0];
            return result;
        }

        public double GetVisible()
        {
            byte[] data = si1145.ReadRegisters(0x24, 2);
            int result = (data[1] << 8) | data[0];
            return result;
        }

        public double GetProximity()
        {
            byte[] data = si1145.ReadRegisters(0x26, 2);
            int result = (data[1] << 8) | data[0];
            return result;
        }

        private void Initialize()
        {
            Reset();

            // enable UVindex measurement coefficients!
            Write8(REG_UCOEFF0, 0x29);
            Write8(REG_UCOEFF1, 0x89);
            Write8(REG_UCOEFF2, 0x02);
            Write8(REG_UCOEFF3, 0x00);

            // enable UV sensor
            WriteParam(PARAM_CHLIST, PARAM_CHLIST_ENUV |
                PARAM_CHLIST_ENALSIR | PARAM_CHLIST_ENALSVIS |
                PARAM_CHLIST_ENPS1);

            // enable interrupt on every sample
            Write8(REG_INTCFG, REG_INTCFG_INTOE);
            Write8(REG_IRQEN, REG_IRQEN_ALSEVERYSAMPLE);

            // program LED current
            Write8(REG_PSLED21, 0x03); // 20mA for LED 1 only
            WriteParam(PARAM_PS1ADCMUX, PARAM_ADCMUX_LARGEIR);
            // prox sensor #1 uses LED #1
            WriteParam(PARAM_PSLED12SEL, PARAM_PSLED12SEL_PS1LED1);
            // fastest clocks, clock div 1
            WriteParam(PARAM_PSADCGAIN, 0);
            // take 511 clocks to measure
            WriteParam(PARAM_PSADCOUNTER, PARAM_ADCCOUNTER_511CLK);
            // in prox mode, high range
            WriteParam(PARAM_PSADCMISC, PARAM_PSADCMISC_RANGE | PARAM_PSADCMISC_PSMODE);

            WriteParam(PARAM_ALSIRADCMUX, PARAM_ADCMUX_SMALLIR);
            // fastest clocks, clock div 1
            WriteParam(PARAM_ALSIRADCGAIN, 0);
            // take 511 clocks to measure
            WriteParam(PARAM_ALSIRADCOUNTER, PARAM_ADCCOUNTER_511CLK);
            // in high range mode
            WriteParam(PARAM_ALSIRADCMISC, PARAM_ALSIRADCMISC_RANGE);


            // fastest clocks, clock div 1
            WriteParam(PARAM_ALSVISADCGAIN, 0);
            // take 511 clocks to measure
            WriteParam(PARAM_ALSVISADCOUNTER, PARAM_ADCCOUNTER_511CLK);
            // in high range mode (not normal signal)
            WriteParam(PARAM_ALSVISADCMISC, PARAM_ALSVISADCMISC_VISRANGE);

            // measurement rate for auto
            Write8(REG_MEASRATE0, 0xFF); // 255 * 31.25uS = 8ms

            // auto run
            Write8(REG_COMMAND, PSALS_AUTO);
        }

        private byte WriteParam(byte param, int value)
        {
            Write8(REG_PARAMWR, (byte)value);
            Write8(REG_COMMAND, (byte)(param | PARAM_SET));

            return si1145.ReadRegister(REG_PARAMRD);
        }

        private void Write8(byte reg, byte val)
        {
            si1145.WriteByte(reg);
            si1145.WriteByte(val);
        }

        private byte ReadParam(byte param)
        {
            Write8(REG_COMMAND, (byte)(param | PARAM_QUERY));
            return si1145.ReadRegister(REG_PARAMRD);
        }

        private void Reset ()
        {
            Write8(REG_MEASRATE0, 0);
            Write8(REG_MEASRATE1, 0);
            Write8(REG_IRQEN, 0);
            Write8(REG_IRQMODE1, 0);
            Write8(REG_IRQMODE2, 0);
            Write8(REG_INTCFG, 0);
            Write8(REG_IRQSTAT, 0xFF);

            Write8(REG_COMMAND, RESET);

            Thread.Sleep(10);

            Write8(REG_HWKEY, 0x17);

            Thread.Sleep(10);
        }

        #endregion
    }
}