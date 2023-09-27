using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Light
{
    /// <summary>
    /// Represents a SiLabs Proximity, UV, and ambient light sensor
    /// </summary>
    public partial class Si1145
        : ByteCommsSensorBase<(Illuminance? VisibleLight, double? UltravioletIndex, Illuminance? Infrared)>,
        II2cPeripheral
    {
        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        /// <summary>
        /// Create a new SI1145 sensor object
        /// </summary>
        /// <param name="i2cBus">I2cBus (default to 400 KHz)</param>
        public Si1145(II2cBus i2cBus) : base(i2cBus, (byte)Addresses.Default)
        {
            if (BusComms.ReadRegister(Registers.REG_PARTID) != 0x45)
            {
                throw new Exception("Invalid part ID");
            }
            Initialize();
        }

        /// <summary>
        /// Read data from the sensor
        /// </summary>
        /// <returns>Returns visible, ultraviolet index and infrared data</returns>
        protected override Task<(Illuminance? VisibleLight, double? UltravioletIndex, Illuminance? Infrared)> ReadSensor()
        {
            (Illuminance? VisibleLight, double? UltravioletIndex, Illuminance? Infrared) conditions;

            // ultraviolet (UV) index
            BusComms.ReadRegister(0x2C, ReadBuffer.Span[0..2]);
            int rawUVIndex = (ReadBuffer.Span[1] << 8) | ReadBuffer.Span[0];
            conditions.UltravioletIndex = rawUVIndex / 100.0;

            // Infrared
            BusComms.ReadRegister(0x22, ReadBuffer.Span[0..2]);
            int rawInfrared = (ReadBuffer.Span[1] << 8) | ReadBuffer.Span[0];
            conditions.Infrared = new Illuminance(rawInfrared, Illuminance.UnitType.Lux);

            // Visible
            BusComms.ReadRegister(0x24, ReadBuffer.Span[0..2]);
            int rawVisible = (ReadBuffer.Span[1] << 8) | ReadBuffer.Span[0];
            conditions.VisibleLight = new Illuminance(rawVisible, Illuminance.UnitType.Lux);

            return Task.FromResult(conditions);
        }

        private void Initialize()
        {
            Reset();

            // enable UVindex measurement coefficients!
            BusComms.WriteRegister(Registers.REG_UCOEFF0, 0x29);
            BusComms.WriteRegister(Registers.REG_UCOEFF1, 0x89);
            BusComms.WriteRegister(Registers.REG_UCOEFF2, 0x02);
            BusComms.WriteRegister(Registers.REG_UCOEFF3, 0x00);

            // enable UV sensor
            WriteParam(Parameters.PARAM_CHLIST, Parameters.PARAM_CHLIST_ENUV |
                Parameters.PARAM_CHLIST_ENALSIR | Parameters.PARAM_CHLIST_ENALSVIS |
                Parameters.PARAM_CHLIST_ENPS1);

            // enable interrupt on every sample
            BusComms.WriteRegister(Registers.REG_INTCFG, Registers.REG_INTCFG_INTOE);
            BusComms.WriteRegister(Registers.REG_IRQEN, Registers.REG_IRQEN_ALSEVERYSAMPLE);

            // program LED current
            BusComms.WriteRegister(Registers.REG_PSLED21, 0x03); // 20mA for LED 1 only
            WriteParam(Parameters.PARAM_PS1ADCMUX, Parameters.PARAM_ADCMUX_LARGEIR);
            // prox sensor #1 uses LED #1
            WriteParam(Parameters.PARAM_PSLED12SEL, Parameters.PARAM_PSLED12SEL_PS1LED1);
            // fastest clocks, clock div 1
            WriteParam(Parameters.PARAM_PSADCGAIN, 0);
            // take 511 clocks to measure
            WriteParam(Parameters.PARAM_PSADCOUNTER, Parameters.PARAM_ADCCOUNTER_511CLK);
            // in prox mode, high range
            WriteParam(Parameters.PARAM_PSADCMISC, Parameters.PARAM_PSADCMISC_RANGE | Parameters.PARAM_PSADCMISC_PSMODE);

            WriteParam(Parameters.PARAM_ALSIRADCMUX, Parameters.PARAM_ADCMUX_SMALLIR);
            // fastest clocks, clock div 1
            WriteParam(Parameters.PARAM_ALSIRADCGAIN, 0);
            // take 511 clocks to measure
            WriteParam(Parameters.PARAM_ALSIRADCOUNTER, Parameters.PARAM_ADCCOUNTER_511CLK);
            // in high range mode
            WriteParam(Parameters.PARAM_ALSIRADCMISC, Parameters.PARAM_ALSIRADCMISC_RANGE);

            // fastest clocks, clock div 1
            WriteParam(Parameters.PARAM_ALSVISADCGAIN, 0);
            // take 511 clocks to measure
            WriteParam(Parameters.PARAM_ALSVISADCOUNTER, Parameters.PARAM_ADCCOUNTER_511CLK);
            // in high range mode (not normal signal)
            WriteParam(Parameters.PARAM_ALSVISADCMISC, Parameters.PARAM_ALSVISADCMISC_VISRANGE);

            // measurement rate for auto
            BusComms.WriteRegister(Registers.REG_MEASRATE0, 0xFF); // 255 * 31.25uS = 8ms

            // auto run
            BusComms.WriteRegister(Registers.REG_COMMAND, Commands.PSALS_AUTO);
        }

        private byte WriteParam(byte param, int value)
        {
            BusComms.WriteRegister(Registers.REG_PARAMWR, (byte)value);
            BusComms.WriteRegister(Registers.REG_COMMAND, (byte)(param | Commands.PARAM_SET));

            return BusComms.ReadRegister(Registers.REG_PARAMRD);
        }

        private byte ReadParam(byte param)
        {
            BusComms.WriteRegister(Registers.REG_COMMAND, (byte)(param | Commands.PARAM_QUERY));
            return BusComms.ReadRegister(Registers.REG_PARAMRD);
        }

        private void Reset()
        {
            BusComms.WriteRegister(Registers.REG_MEASRATE0, 0);
            BusComms.WriteRegister(Registers.REG_MEASRATE1, 0);
            BusComms.WriteRegister(Registers.REG_IRQEN, 0);
            BusComms.WriteRegister(Registers.REG_IRQMODE1, 0);
            BusComms.WriteRegister(Registers.REG_IRQMODE2, 0);
            BusComms.WriteRegister(Registers.REG_INTCFG, 0);
            BusComms.WriteRegister(Registers.REG_IRQSTAT, 0xFF);

            BusComms.WriteRegister(Registers.REG_COMMAND, Commands.RESET);

            Thread.Sleep(10);

            BusComms.WriteRegister(Registers.REG_HWKEY, 0x17);

            Thread.Sleep(10);
        }
    }
}