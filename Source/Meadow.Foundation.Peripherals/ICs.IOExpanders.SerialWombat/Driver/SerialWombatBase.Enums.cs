namespace Meadow.Foundation.ICs.IOExpanders
{

    public partial class SerialWombatBase
    {
        /// <summary>
        /// Valid addresses for the Chip.
        /// </summary>
        public enum Address : byte
        {
            /// <summary>
            /// Bus address 0x6a
            /// </summary>
            Address_0x6a = 0x6a,
            /// <summary>
            /// Bus address 0x6c
            /// </summary>
            Address_0x6b = 0x6b,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x6b
        }

        public enum FlashRegister18
        {
            DeviceID = 0xFF0000,
            DeviceRevision = 0xFF0002,
            DeviceUuid = 0x801600
        }

        internal enum PinMode
        {
            DigitalIO = 0,
            Controlled = 1,
            AnalogInput = 2,
            Servo = 3,
            QuadEncoder = 5,
            Watchdog = 7,
            ProtectedOutput = 8,
            TM1637 = 11,
            WS2812 = 12,
            SW_UART = 13,
            InputProcessor = 14,
            MatrixKeypad = 15,
            PWM = 16,
            UART_RX_TX = 17,
            PulseTimer = 18,
            FrameTimer = 21,
            SW18AB_Touchscreen = 22,
            UART1_RX_TX = 23,
            ResistanceInput = 24,
            PulseOnChange = 25,
            HSServo = 26,
            UltrasonicDistance = 27,
            LCD = 28,
            UNKNOWN = 255
        }
    }
}