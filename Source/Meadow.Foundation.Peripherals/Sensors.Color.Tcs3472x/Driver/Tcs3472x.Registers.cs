namespace Meadow.Foundation.Sensors.Color;

public partial class Tcs3472x
{
    internal enum Registers
    {
        /// <summary>
        /// Enable register.
        /// </summary>
        ENABLE = 0x00,

        /// <summary>
        /// RGBC interrupt enable. When asserted, permits RGBC interrupts to be generated.
        /// </summary>
        ENABLE_AIEN = 0x10,

        /// <summary>
        /// Wait enable. This bit activates the wait feature. Writing a 1 activates the wait timer. Writing a 0 disables the wait timer.
        /// </summary>
        ENABLE_WEN = 0x08,

        /// <summary>
        /// RGBC enable. This bit activates the two-channel ADC. Writing a 1 activates the RGBC. Writing a 0 disables the RGBC.
        /// </summary>
        ENABLE_AEN = 0x02,

        /// <summary>
        /// Power ON. This bit activates the internal oscillator to permit the timers and ADC channels to operate. Writing a 1 activates the oscillator. Writing a 0 disables the oscillator.
        /// </summary>
        ENABLE_PON = 0x01,

        /// <summary>
        /// The RGBC timing register controls the internal integration time of the RGBC clear and IR channel ADCs in 2.4-ms increments. Max RGBC Count = (256 − ATIME) × 1024 up to a maximum of 65535.
        /// </summary>
        ATIME = 0x01,

        /// <summary>
        /// Wait time is set in 2.4 ms increments unless the WLONG bit is asserted, in which case the wait times are 12× longer. WTIME is programmed as a 2’s complement number.
        /// </summary>
        WTIME = 0x03,

        /// <summary>
        /// RGBC clear channel low threshold lower byte.
        /// </summary>
        AILTL = 0x04,

        /// <summary>
        /// RGBC clear channel low threshold upper byte.
        /// </summary>
        AILTH = 0x05,

        /// <summary>
        /// RGBC clear channel high threshold lower byte.
        /// </summary>
        AIHTL = 0x06,

        /// <summary>
        /// RGBC clear channel high threshold upper byte.
        /// </summary>
        AIHTH = 0x07,

        /// <summary>
        /// The persistence register controls the filtering interrupt capabilities of the device.
        /// </summary>
        PERS = 0x0C,

        /// <summary>
        /// The configuration register sets the wait long time.
        /// </summary>
        CONFIG = 0x0D,

        /// <summary>
        /// Wait Long. When asserted, the wait cycles are increased by a factor of 12× from that programmed in the WTIME register.
        /// </summary>
        CONFIG_WLONG = 0x02,

        /// <summary>
        /// The Control register provides eight bits of miscellaneous control to the analog block. These bits typically control functions such as gain settings and/or diode selection.
        /// </summary>
        CONTROL = 0x0F,

        /// <summary>
        /// 0x44 = TCS34721/TCS34725, 0x4D = TCS34723/TCS34727
        /// </summary>
        ID = 0x12,

        /// <summary>
        /// The Status Register provides the internal status of the device.
        /// </summary>
        STATUS = 0x13,

        /// <summary>
        /// RGBC clear channel Interrupt.
        /// </summary>
        STATUS_AINT = 0x10,

        /// <summary>
        /// RGBC Valid. Indicates that the RGBC channels have completed an integration cycle.
        /// </summary>
        STATUS_AVALID = 0x01,

        /// <summary>
        /// Clear data low byte.
        /// </summary>
        CDATAL = 0x14,

        /// <summary>
        /// Clear data high byte.
        /// </summary>
        CDATAH = 0x15,

        /// <summary>
        /// Red data low byte.
        /// </summary>
        RDATAL = 0x16,

        /// <summary>
        /// Red data high byte.
        /// </summary>
        RDATAH = 0x17,

        /// <summary>
        /// Green data low byte.
        /// </summary>
        GDATAL = 0x18,

        /// <summary>
        /// Green data high byte.
        /// </summary>
        GDATAH = 0x19,

        /// <summary>
        /// Blue data low byte.
        /// </summary>
        BDATAL = 0x1A,

        /// <summary>
        /// Blue data high byte.
        /// </summary>
        BDATAH = 0x1B,

        /// <summary>
        /// Command bit.
        /// </summary>
        COMMAND_BIT = 0x80
    }
}
