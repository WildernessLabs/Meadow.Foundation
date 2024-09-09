namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Ads1263
    {
        /// <summary> Multiplexer sources for either of the differential inputs of the ADC </summary>
        public enum AdcSource : byte
        {
            /// <summary> AIN0 </summary>
            AIN0 = 0x0,
            /// <summary> AIN1 </summary>
            AIN1 = 0x1,
            /// <summary> AIN2 </summary>
            AIN2 = 0x2,
            /// <summary> AIN3 </summary>
            AIN3 = 0x3,
            /// <summary> AIN4 </summary>
            AIN4 = 0x4,
            /// <summary> AIN5 </summary>
            AIN5 = 0x5,
            /// <summary> AIN6 </summary>
            AIN6 = 0x6,
            /// <summary> AIN7 </summary>
            AIN7 = 0x7,
            /// <summary> AIN8 </summary>
            AIN8 = 0x8,
            /// <summary> AIN9 </summary>
            AIN9 = 0x9,
            /// <summary> AIN COM </summary>
            AINCOM = 0xA,
            /// <summary> Temp Sensor. Ensure that gain is set to 1! </summary>
            TempSensor = 0xB,
            /// <summary> Analog Power Supply (Divided by 4). Ensure that gain is set to 1, and the reference voltage is at least 1.5V! </summary>
            AnalogSupply = 0xC,
            /// <summary> Digital Power Supply (Divided by 4). Ensure that gain is set to 1, and the reference voltage is at least 1.5V! </summary>
            DigitalSupply = 0xD,
            /// <summary> Test DAC </summary>
            TESTDAC = 0xE,
            /// <summary> Floating </summary>
            Float = 0xF,
        }

        /// <summary> Multiplexer sources for the positive differential voltage reference for ADC1 </summary>
        public enum Adc1ReferenceP : byte
        {
            /// <summary> Internal 2.5V reference (default) </summary>
            Internal = 0x0,
            /// <summary> AIN0 </summary>
            AIN0 = 0x1,
            /// <summary> AIN2 </summary>
            AIN2 = 0x2,
            /// <summary> AIN4 </summary>
            AIN4 = 0x3,
            /// <summary> Analog Power Supply AVDD (usually 5V) </summary>
            AVDD = 0x4
        }

        /// <summary> Multiplexer sources for the negative differential voltage reference for ADC1 </summary>
        public enum Adc1ReferenceN : byte
        {
            /// <summary> Analog Power Supply AVSS (Usually GND) </summary>
            Internal = 0x0,
            /// <summary> AIN1 </summary>
            AIN1 = 0x1,
            /// <summary> AIN3 </summary>
            AIN3 = 0X2,
            /// <summary> AIN5 </summary>
            AIN5 = 0X3,
            /// <summary> Analog Power Supply AVSS (Usually GND) </summary>
            AVSS = 0X4
        }

        /// <summary> Sample rate output from ADC1 after all filtering is applied </summary>
        public enum Adc1DataRate : byte
        {
            /// <summary> 2.5 Samples per second </summary>
            SPS_2p5 = 0x0,
            /// <summary> 5 Samples per second </summary>
            SPS_5 = 0x1,
            /// <summary> 10 Samples per second </summary>
            SPS_10 = 0x2,
            /// <summary> 16.6 Samples per second (60 ms) </summary>
            SPS_16p6 = 0x3,
            /// <summary> 20 Samples per second </summary>
            SPS_20 = 0x4,
            /// <summary> 50 Samples per second </summary>
            SPS_50 = 0x5,
            /// <summary> 60 Samples per second </summary>
            SPS_60 = 0x6,
            /// <summary> 100 Samples per second </summary>
            SPS_100 = 0x7,
            /// <summary> 400 Samples per second </summary>
            SPS_400 = 0x8,
            /// <summary> 1200 Samples per second </summary>
            SPS_1200 = 0x9,
            /// <summary> 2400 Samples per second </summary>
            SPS_2400 = 0xA,
            /// <summary> 4800 Samples per second </summary>
            SPS_4800 = 0xB,
            /// <summary> 7200 Samples per second </summary>
            SPS_7200 = 0xC,
            /// <summary> 14400 Samples per second </summary>
            SPS_14400 = 0xD,
            /// <summary> 19200 Samples per second </summary>
            SPS_19200 = 0xE,
            /// <summary> 38400 Samples per second </summary>
            SPS_38400 = 0xF,
        }

        /// <summary> PGA gain for ADC1 </summary>
        public enum Adc1Gain : byte
        {
            /// <summary> 1 V/V (resulting in ±2.5 V range assuming Internal reference) </summary>
            Gain_1 = 0x0,
            /// <summary> 2 V/V (resulting in ±1.25 V range assuming Internal reference) </summary>
            Gain_2 = 0x1,
            /// <summary> 4 V/V (resulting in ±625 mV range assuming Internal reference) </summary>
            Gain_4 = 0x2,
            /// <summary> 8 V/V (resulting in ±312 mV range assuming Internal reference) </summary>
            Gain_8 = 0x3,
            /// <summary> 16 V/V (resulting in ±156 mV range assuming Internal reference) </summary>
            Gain_16 = 0x4,
            /// <summary> 32 V/V (resulting in ±78 mV range assuming Internal reference) </summary>
            Gain_32 = 0x5,
        }

        /// <summary> Digital Filtering modes for ADC1 (See datasheet) </summary>
        public enum Adc1Filter : byte
        {
            /// <summary> First order sin(x)/x </summary>
            Sinc1 = 0x00,
            /// <summary> Second order sin(x)/x </summary>
            Sinc2 = 0x01,
            /// <summary> Third order sin(x)/x </summary>
            Sinc3 = 0x02,
            /// <summary> Fourth order sin(x)/x </summary>
            Sinc4 = 0x03,
            /// <summary> Finite Impulse Response. Specifically tuned to filter out 50Hz and 60Hz (see datasheet) </summary>
            FIR = 0x04,

        }

        /// <summary> Sample rate output from ADC2 after all filtering is applied </summary>
        public enum Adc2DataRate : byte
        {
            /// <summary> 10 Samples per second </summary>
            SPS_10 = 0x0,
            /// <summary> 100 Samples per second </summary>
            SPS_100 = 0x1,
            /// <summary> 400 Samples per second </summary>
            SPS_400 = 0x2,
            /// <summary> 600 Samples per second </summary>
            SPS_800 = 0x3,
        }

        /// <summary> PGA gain for ADC2 </summary>
        public enum Adc2Gain : byte
        {
            /// <summary> 1 V/V (resulting in ±2.5 V range assuming Internal reference) </summary>
            Gain_1 = 0x0,
            /// <summary> 2 V/V (resulting in ±1.25 V range assuming Internal reference) </summary>
            Gain_2 = 0x1,
            /// <summary> 4 V/V (resulting in ±625 mV range assuming Internal reference) </summary>
            Gain_4 = 0x2,
            /// <summary> 8 V/V (resulting in ±312 mV range assuming Internal reference) </summary>
            Gain_8 = 0x3,
            /// <summary> 16 V/V (resulting in ±156 mV range assuming Internal reference) </summary>
            Gain_16 = 0x4,
            /// <summary> 32 V/V (resulting in ±78 mV range assuming Internal reference) </summary>
            Gain_32 = 0x5,
            /// <summary> 64 V/V (resulting in ±39 mV range assuming Internal reference) </summary>
            Gain_64 = 0x6,
                /// <summary> 128 V/V (resulting in ±19.5 mV range assuming Internal reference) </summary>
            Gain_128 = 0x7,
        }

        /// <summary> Multiplexer sources for the differential voltage reference for ADC2 </summary>
        public enum Adc2Reference : byte
        {
            /// <summary> Internal 2.5V reference </summary>
            Internal = 0x0,
            /// <summary> AIN0 (positive) and AIN1 (negative) </summary>
            AIN0_AIN1 = 0x1,
            /// <summary> AIN2 (positive) and AIN3 (negative) </summary>
            AIN2_AIN3 = 0x2,
            /// <summary> AIN4 (positive) and AIN5 (negative) </summary>
            AIN4_AIN5 = 0x3,
            /// <summary> Analog power supply (AVDD and AVSS) </summary>
            AVDD_AVSS = 0x4
        }

        // TODO: Enums for IDACMUX, IDACMAG

        internal enum OpCode : byte
        {
            NOP = 0x00,
            RESET = 0x06,
            START1 = 0x08,
            STOP1 = 0x0A,
            START2 = 0x0C,
            STOP2 = 0x0E,
            RDATA1 = 0x12,
            RDATA2 = 0x14,
            /// <summary> Read register. Add register number to byte, and follow with (number of registers being read minus 1) </summary>
            RREG = 0x20,
            /// <summary> Write register. Add register number to byte, and follow with (number of registers being written minus 1) </summary>
            WREG = 0x40,
        }

        /// <summary>
        /// Register Map. Used with <see cref="OpCode.RREG"/> and <see cref="OpCode.WREG"/>.
        /// </summary>
        public enum Register : byte
        {
            /// <summary> ID Register </summary>
            ID = 0x00,
            /// <summary> Power Register </summary>
            POWER = 0x01,
            /// <summary> Interface Register </summary>
            INTERFACE = 0x02,
            /// <summary> ADC1 Configuration Register 0 </summary>
            MODE0 = 0x03,
            /// <summary> ADC1 Configuration Register 1 </summary>
            MODE1 = 0x04,
            /// <summary> ADC1 Configuration Register 2 </summary>
            MODE2 = 0x05,
            /// <summary> Input Multiplexer Register </summary>
            INPMUX = 0x06,
            /// <summary> Offset Calibration Register 0 </summary>
            OFCAL0 = 0x07,
            /// <summary> Offset Calibration Register 1 </summary>
            OFCAL1 = 0x08,
            /// <summary> Offset Calibration Register 2 </summary>
            OFCAL2 = 0x09,
            /// <summary> Full Scale Calibration Register 0 </summary>
            FSCAL0 = 0x0A,
            /// <summary> Full Scale Calibration Register 1 </summary>
            FSCAL1 = 0x0B,
            /// <summary> Full Scale Calibration Register 2 </summary>
            FSCAL2 = 0x0C,
            /// <summary> IDAC Multiplexer Register </summary>
            IDACMUX = 0x0D,
            /// <summary> IDAC Magnitude Register </summary>
            IDACMAG = 0x0E,
            /// <summary> Analog Reference Multiplexer Register </summary>
            REFMUX = 0x0F,
            /// <summary> TDAC Positive Control Register </summary>
            TDACP = 0x10,
            /// <summary> TDAC Negative Control Register </summary>
            TDACN = 0x11,
            /// <summary> GPIO Control Enable Register </summary>
            GPIOCON = 0x12,
            /// <summary> GPIO Direction Register </summary>
            GPIODIR = 0x13,
            /// <summary> GPIO Data Register </summary>
            GPIODAT = 0x14,
            /// <summary> ADC2 Configuration Register </summary>
            ADC2CFG = 0x15,
            /// <summary> ADC2 Multiplexer Register </summary>
            ADC2MUX = 0x16,
            /// <summary> ADC2 Offset Calibration Register 0 </summary>
            ADC2OFC0 = 0x17,
            /// <summary> ADC2 Offset Calibration Register 1 </summary>
            ADC2OFC1 = 0x18,
            /// <summary> ADC2 Scale Calibration Register 0 </summary>
            ADC2OSC0 = 0x19,
            /// <summary> ADC2 Scale Calibration Register 1 </summary>
            ADC2OSC1 = 0x1A,
        }
    }
}
