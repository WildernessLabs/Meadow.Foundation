namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Ak9753
    {
        /// <summary>
        /// Sensor operating mode — ECNTL1 bits 2:0
        /// </summary>
        public enum Mode : byte
        {
            /// <summary>Standby — minimal power consumption</summary>
            Standby      = 0b000,
            /// <summary>EEPROM access mode</summary>
            EepromAccess = 0b001,
            /// <summary>Single-shot measurement</summary>
            SingleShot   = 0b010,
            // 0b011 is prohibited
            /// <summary>Continuous mode 0 — recommended default</summary>
            Continuous0  = 0b100,
            /// <summary>Continuous mode 1</summary>
            Continuous1  = 0b101,
            /// <summary>Continuous mode 2</summary>
            Continuous2  = 0b110,
            /// <summary>Continuous mode 3</summary>
            Continuous3  = 0b111,
        }

        /// <summary>
        /// Digital filter cutoff frequency — ECNTL1 bits 5:3
        /// </summary>
        public enum FilterFrequency : byte
        {
            /// <summary>0.3 Hz — most filtering, slowest response</summary>
            Hz_0_3 = 0b000,
            /// <summary>0.6 Hz</summary>
            Hz_0_6 = 0b001,
            /// <summary>1.1 Hz</summary>
            Hz_1_1 = 0b010,
            /// <summary>2.2 Hz</summary>
            Hz_2_2 = 0b011,
            /// <summary>4.4 Hz</summary>
            Hz_4_4 = 0b100,
            /// <summary>8.8 Hz — least filtering, fastest response (default)</summary>
            Hz_8_8 = 0b101,
        }
    }
}
