namespace Meadow.Foundation.Leds
{
    public partial class Pca9633
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x62
            /// </summary>
            Address_0x62 = 0x62,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x62
        }

        /// <summary>
        /// Pca9633 registers
        /// </summary>
        public enum Registers : byte
        {
            /// <summary>
            /// Mode 1
            /// </summary>
            MODE1 = 0x00,
            /// <summary>
            /// Mode 2
            /// </summary>
            MODE2 = 0x01,
            /// <summary>
            /// Brightness control LED0
            /// </summary>
            PWM0 = 0x02,
            /// <summary>
            /// Brightness control LED1
            /// </summary>
            PWM1 = 0x03,
            /// <summary>
            /// Brightness control LED2
            /// </summary>
            PWM2 = 0x04,
            /// <summary>
            /// Brightness control LED3
            /// </summary>
            PWM3 = 0x05,
            /// <summary>
            /// Group duty cycle control
            /// </summary>
            GRPPWM = 0x06,
            /// <summary>
            /// Group frequency
            /// </summary>
            GRPFREQ = 0x07,
            /// <summary>
            /// Led output state
            /// </summary>
            LEDOUT = 0x08,
        }

        /// <summary>
        /// Led position on controller
        /// </summary>
        public enum LedPosition : byte
        {
            /// <summary>
            /// Led 0
            /// </summary>
            Led0 = 0x02,
            /// <summary>
            /// Led 1
            /// </summary>
            Led1 = 0x03,
            /// <summary>
            /// Led 2
            /// </summary>
            Led2 = 0x04,
            /// <summary>
            /// Led 3
            /// </summary>
            Led3 = 0x05,
            /// <summary>
            /// Led position unknown
            /// </summary>
            Undefined,
        }

        /// <summary>
        /// Auto increment mode
        /// </summary>
        public enum AutoIncrement : byte
        {
            /// <summary>
            /// None
            /// </summary>
            None,
            /// <summary>
            /// All registers
            /// </summary>
            AllRegisters,
            /// <summary>
            /// Individual brightness registers
            /// </summary>
            IndividualBrightnessRegisters,
            /// <summary>
            /// Global control registers
            /// </summary>
            GlobalControlRegisters,
            /// <summary>
            /// Individual and global registers
            /// </summary>
            IndividualAndGlobalRegisters,
        }

        /// <summary>
        /// Global driver output type 
        /// </summary>
        public enum DriveType : byte
        {
            /// <summary>
            /// Open drain
            /// </summary>
            OpenDrain,
            /// <summary>
            /// Totem pole
            /// </summary>
            TotemPole,
        }
    }
}
