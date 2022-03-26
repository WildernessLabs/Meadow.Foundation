using System;
namespace Meadow.Foundation.Sensors.LoadCell
{
    public partial class Nau7802
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x2A
            /// </summary>
            Address_0x2A = 0x2A,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x2A
        }

        private enum CTRL2_BITS : byte
        {
            CALMOD = 1 << 0,
            CALS = 1 << 2,
            CAL_ERROR = 1 << 3,
            CRS = 1 << 4,
            CHS = 1 << 7,
        }

        private enum Register : byte
        {
            PU_CTRL = 0x00,
            CTRL1 = 0x01,
            CTRL2 = 0x02,
            OCAL1_B2 = 0x03,
            OCAL1_B1 = 0x04,
            OCAL1_B0 = 0x05,
            GCAL1_B3 = 0x06,
            GCAL1_B2 = 0x07,
            GCAL1_B1 = 0x08,
            GCAL1_B0 = 0x09,
            OCAL2_B2 = 0x0a,
            OCAL2_B1 = 0x0b,
            OCAL2_B0 = 0x0c,
            GCAL2_B3 = 0x0d,
            GCAL2_B2 = 0x0e,
            GCAL2_B1 = 0x0f,
            GCAL2_B0 = 0x10,
            I2C_CTRL = 0x11,
            ADCO_B2 = 0x12,
            ADCO_B1 = 0x13,
            ADCO_B0 = 0x14,
            OTP_ADC = 0x15,
            OTP_B1 = 0x16,
            OTP_B0 = 0x17,
            PGA = 0x1B,
            PGA_PWR = 0x1C,
            DEV_REV = 0x1f
        }

        [Flags]
        private enum PU_CTRL_BITS
        {
            /// <summary>
            /// Register Reset
            /// </summary>
            RR = 1 << 0,
            /// <summary>
            /// Power up digital
            /// </summary>
            PUD = 1 << 1,
            /// <summary>
            /// Power-up analog
            /// </summary>
            PUA = 1 << 2,
            /// <summary>
            /// Power-up ready
            /// </summary>
            PUR = 1 << 3,
            /// <summary>
            /// Cycle start
            /// </summary>
            CS = 1 << 4,
            /// <summary>
            /// Cycle ready
            /// </summary>
            CR = 1 << 5,
            /// <summary>
            /// System clock source select
            /// </summary>
            OSCS = 1 << 6,
            /// <summary>
            /// AVDD source select
            /// </summary>
            AVDDS = 1 << 7
        }

        private enum LdoVoltage
        {
            LDO_2V4 = 0b111,
            LDO_2V7 = 0b110,
            LDO_3V0 = 0b101,
            LDO_3V3 = 0b100,
            LDO_3V6 = 0b011,
            LDO_3V9 = 0b010,
            LDO_4V2 = 0b001,
            LDO_4V5 = 0b000,
        }

        private enum AdcGain
        {
            Gain0 = 0b000,
            Gain2 = 0b001,
            Gain4 = 0b010,
            Gain8 = 0b011,
            Gain16 = 0b100,
            Gain32 = 0b101,
            Gain64 = 0b110,
            Gain128 = 0b111,
        }

        private enum ConversionRate
        {
            SamplePerSecond10 = 0b000,
            SamplePerSecond20 = 0b001,
            SamplePerSecond40 = 0b010,
            SamplePerSecond80 = 0b011,
            SamplePerSecond320 = 0b111,
        }
    }
}
