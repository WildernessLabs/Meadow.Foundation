namespace Meadow.Foundation.Sensors.Light
{
    public partial class Tcs3472x
    {
        /// <summary>
		///     Valid addresses for the sensor.
		/// </summary>
		public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x29
            /// </summary>
            Address_0x29 = 0x29,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x29
        }

        /// <summary>
        /// Type of TCS3472X
        /// </summary>
        public enum DeviceType
        {
            Tcs34721 = 0x44,
            Tcs34725 = 0x44,
            Tcs34723 = 0x4D,
            Tcs34727 = 0x4D
        }

        /// <summary>
        /// The gain used to integrate the colors
        /// </summary>
        public enum GainType
        {
            Gain1X = 0x00,
            Gain4X = 0x01,
            Gain16X = 0x02,
            Gain60X = 0x03,
        }

        protected enum Registers
        {
            ENABLE = 0x00,
            // RGBC interrupt enable.  When asserted, permits RGBC interrupts to be generated.
            ENABLE_AIEN = 0x10,
            // Wait enable.  This bit activates the wait feature.  Writing a 1 activates the wait timer.  Writing a 0 disables the wait timer.
            ENABLE_WEN = 0x08,
            // RGBC enable.  This bit actives the two-channel ADC.  Writing a 1 activates the RGBC.  Writing a 0 disables the RGBC.
            ENABLE_AEN = 0x02,
            // Power ON.  This bit activates the internal oscillator to permit the timers and ADC channels to operate. Writing a 1 activates the oscillator.  Writing a 0 disables the oscillator.
            ENABLE_PON = 0x01,
            // The RGBC timing register controls the internal integration time of the RGBC clear and IR channel ADCs in 2.4-ms increments. Max RGBC Count = (256 − ATIME) × 1024 up to a maximum of 65535.
            // Integration time
            ATIME = 0x01,
            // Wait time is set 2.4 ms increments unless the WLONG bit is asserted, in which case the wait times are 12× longer. WTIME is programmed as a 2’s complement number.
            WTIME = 0x03,
            // RGBC clear channel low threshold lower byte
            AILTL = 0x04,
            // RGBC clear channel low threshold upper byte
            AILTH = 0x05,
            // RGBC clear channel high threshold lower byte
            AIHTL = 0x06,
            // RGBC clear channel high threshold upper byte
            AIHTH = 0x07,
            // The persistence register controls the filtering interrupt capabilities of the device.
            PERS = 0x0C,
            // The configuration register sets the wait long time.
            CONFIG = 0x0D,
            // Wait Long. When asserted, the wait cycles are increased by a factor 12× from that programmed in the WTIME register.
            CONFIG_WLONG = 0x02,
            // The Control register provides eight bits of miscellaneous control to the analog block. These bits typically control functions such as gain settings and/or diode selection.
            CONTROL = 0x0F,
            // 0x44 = TCS34721/TCS34725, 0x4D = TCS34723/TCS34727
            ID = 0x12,
            // The Status Register provides the internal status of the device.
            STATUS = 0x13,
            // RGBC clear channel Interrupt.
            STATUS_AINT = 0x10,
            // RGBC Valid. Indicates that the RGBC channels have completed an integration cycle.
            STATUS_AVALID = 0x01,
            // Clear data
            CDATAL = 0x14,
            CDATAH = 0x15,
            // Red data
            RDATAL = 0x16,
            RDATAH = 0x17,
            // Green data
            GDATAL = 0x18,
            GDATAH = 0x19,
            // Blue data
            BDATAL = 0x1A,
            BDATAH = 0x1B,
            COMMAND_BIT = 0x80,
        }

        /// <summary>
        /// This enum allows to select how many cycles cill be done measuring before
        /// raising an interupt.
        /// </summary>
        public enum InterruptState
        {
            /// <summary>Every RGBC cycle generates an interrupt</summary>
            All = 0b0000,

            /// <summary>1 clear channel value outside of threshold range</summary>
            Percistence01Cycle = 0b0001,

            /// <summary>2 clear channel consecutive values out of range</summary>
            Percistence02Cycle = 0b0010,

            /// <summary>3 clear channel consecutive values out of range</summary>
            Percistence03Cycle = 0b0011,

            /// <summary>5 clear channel consecutive values out of range</summary>
            Percistence05Cycle = 0b0100,

            /// <summary>10 clear channel consecutive values out of range</summary>
            Percistence10Cycle = 0b0101,

            /// <summary>15 clear channel consecutive values out of range</summary>
            Percistence15Cycle = 0b0110,

            /// <summary>20 clear channel consecutive values out of range</summary>
            Percistence20Cycle = 0b0111,

            /// <summary>25 clear channel consecutive values out of range</summary>
            Percistence25Cycle = 0b1000,

            /// <summary>30 clear channel consecutive values out of range</summary>
            Percistence30Cycle = 0b1001,

            /// <summary>35 clear channel consecutive values out of range</summary>
            Percistence35Cycle = 0b1010,

            /// <summary>40 clear channel consecutive values out of range</summary>
            Percistence40Cycle = 0b1011,

            /// <summary>45 clear channel consecutive values out of range</summary>
            Percistence45Cycle = 0b1100,

            /// <summary>50 clear channel consecutive values out of range</summary>
            Percistence50Cycle = 0b1101,

            /// <summary>55 clear channel consecutive values out of range</summary>
            Percistence55Cycle = 0b1110,

            /// <summary>60 clear channel consecutive values out of range</summary>
            Percistence60Cycle = 0b1111,
        }
    }
}