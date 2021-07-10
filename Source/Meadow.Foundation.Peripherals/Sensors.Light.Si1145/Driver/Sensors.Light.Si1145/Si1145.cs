using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Light
{
    // TODO: the proximity stuff isn't exposed. exposing it needs some more
    // thought. you need to hook up an infrared LED to the LED pin. this will
    // cause that pin to fire, then the sensor measures the time of flight.
    // it's basically a completely different sensor.

    public partial class Si1145
        : ByteCommsSensorBase<(Illuminance? VisibleLight, double? UltravioletIndex, Illuminance? Infrared)>
    {
        /// <summary>
        ///     Create a new SI1145 sensor object.
        /// </summary>
        /// <param name="address">Address of the chip on the I2C bus (default to 0x60).</param>
        /// <param name="i2cBus">I2cBus (default to 400 KHz).</param>
        public Si1145(II2cBus i2cBus)
            : base(i2cBus, 0x60)
        {
            if (Peripheral.ReadRegister(Registers.REG_PARTID) != 0x45) {
                throw new Exception("Invalid part ID");
            }
            Initialize();
        }

        protected async override Task<(Illuminance? VisibleLight, double? UltravioletIndex, Illuminance? Infrared)> ReadSensor()
        {
            return await Task.Run(() => {
                (Illuminance? VisibleLight, double? UltravioletIndex, Illuminance? Infrared) conditions;

                // ultraviolet (UV) index
                Peripheral.ReadRegister(0x2C, ReadBuffer.Span[0..2]);
                int rawUVIndex = (ReadBuffer.Span[1] << 8) | ReadBuffer.Span[0];
                conditions.UltravioletIndex = rawUVIndex / 100.0;

                // Infrared
                Peripheral.ReadRegister(0x22, ReadBuffer.Span[0..2]);
                int rawInfrared = (ReadBuffer.Span[1] << 8) | ReadBuffer.Span[0];
                conditions.Infrared = new Illuminance(rawInfrared, Illuminance.UnitType.Lux);

                // Visible
                Peripheral.ReadRegister(0x24, ReadBuffer.Span[0..2]);
                int rawVisible = (ReadBuffer.Span[1] << 8) | ReadBuffer.Span[0];
                conditions.VisibleLight = new Illuminance(rawVisible, Illuminance.UnitType.Lux);

                return conditions;
            });
        }

        //public double GetProximity()
        //{
        //    byte[] data = Peripheral.ReadRegisters(0x26, 2);
        //    int result = (data[1] << 8) | data[0];
        //    return result;
        //}

        private void Initialize()
        {
            Reset();

            // enable UVindex measurement coefficients!
            Peripheral.WriteRegister(Registers.REG_UCOEFF0, 0x29);
            Peripheral.WriteRegister(Registers.REG_UCOEFF1, 0x89);
            Peripheral.WriteRegister(Registers.REG_UCOEFF2, 0x02);
            Peripheral.WriteRegister(Registers.REG_UCOEFF3, 0x00);

            // enable UV sensor
            WriteParam(Parameters.PARAM_CHLIST, Parameters.PARAM_CHLIST_ENUV |
                Parameters.PARAM_CHLIST_ENALSIR | Parameters.PARAM_CHLIST_ENALSVIS |
                Parameters.PARAM_CHLIST_ENPS1);

            // enable interrupt on every sample
            Peripheral.WriteRegister(Registers.REG_INTCFG, Registers.REG_INTCFG_INTOE);
            Peripheral.WriteRegister(Registers.REG_IRQEN, Registers.REG_IRQEN_ALSEVERYSAMPLE);

            // program LED current
            Peripheral.WriteRegister(Registers.REG_PSLED21, 0x03); // 20mA for LED 1 only
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
            Peripheral.WriteRegister(Registers.REG_MEASRATE0, 0xFF); // 255 * 31.25uS = 8ms

            // auto run
            Peripheral.WriteRegister(Registers.REG_COMMAND, Commands.PSALS_AUTO);
        }

        private byte WriteParam(byte param, int value)
        {
            Peripheral.WriteRegister(Registers.REG_PARAMWR, (byte)value);
            Peripheral.WriteRegister(Registers.REG_COMMAND, (byte)(param | Commands.PARAM_SET));

            return Peripheral.ReadRegister(Registers.REG_PARAMRD);
        }

        private byte ReadParam(byte param)
        {
            Peripheral.WriteRegister(Registers.REG_COMMAND, (byte)(param | Commands.PARAM_QUERY));
            return Peripheral.ReadRegister(Registers.REG_PARAMRD);
        }

        private void Reset ()
        {
            Peripheral.WriteRegister(Registers.REG_MEASRATE0, 0);
            Peripheral.WriteRegister(Registers.REG_MEASRATE1, 0);
            Peripheral.WriteRegister(Registers.REG_IRQEN, 0);
            Peripheral.WriteRegister(Registers.REG_IRQMODE1, 0);
            Peripheral.WriteRegister(Registers.REG_IRQMODE2, 0);
            Peripheral.WriteRegister(Registers.REG_INTCFG, 0);
            Peripheral.WriteRegister(Registers.REG_IRQSTAT, 0xFF);

            Peripheral.WriteRegister(Registers.REG_COMMAND, Commands.RESET);

            Thread.Sleep(10);

            Peripheral.WriteRegister(Registers.REG_HWKEY, 0x17);

            Thread.Sleep(10);
        }
    }
}