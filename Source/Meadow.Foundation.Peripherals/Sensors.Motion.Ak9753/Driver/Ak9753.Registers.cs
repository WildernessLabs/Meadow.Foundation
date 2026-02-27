namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Ak9753
    {
        private enum Registers : byte
        {
            WIA1    = 0x00, // Company code
            WIA2    = 0x01, // Device ID — expected value 0x13
            INFO1   = 0x02, // Sensor variant: 0x00 = AK9750, 0x01 = AK9753
            INFO2   = 0x03, // Information
            INTST   = 0x04, // Interrupt status
            ST1     = 0x05, // Status 1: bit0 = DRDY (data ready), bit1 = DOR (data overrun)
            IR1L    = 0x06, // IR channel 1 LSB — burst-read 10 bytes from here through TMP MSB
            IR1H    = 0x07, // IR channel 1 MSB
            IR2L    = 0x08, // IR channel 2 LSB
            IR2H    = 0x09, // IR channel 2 MSB
            IR3L    = 0x0A, // IR channel 3 LSB
            IR3H    = 0x0B, // IR channel 3 MSB
            IR4L    = 0x0C, // IR channel 4 LSB
            IR4H    = 0x0D, // IR channel 4 MSB
            TMPL    = 0x0E, // Temperature LSB (10-bit value in bits 15:6)
            TMPH    = 0x0F, // Temperature MSB
            ST2     = 0x10, // Dummy register — must read after IR data to trigger next measurement
            ETH13HL = 0x11, // IR1-IR3 upper threshold low byte
            ETH13HH = 0x12, // IR1-IR3 upper threshold high byte
            ETH13LL = 0x13, // IR1-IR3 lower threshold low byte
            ETH13LH = 0x14, // IR1-IR3 lower threshold high byte
            ETH24HL = 0x15, // IR2-IR4 upper threshold low byte
            ETH24HH = 0x16, // IR2-IR4 upper threshold high byte
            ETH24LL = 0x17, // IR2-IR4 lower threshold low byte
            ETH24LH = 0x18, // IR2-IR4 lower threshold high byte
            EHYS13  = 0x19, // Interrupt hysteresis, channels 1 & 3
            EHYS24  = 0x1A, // Interrupt hysteresis, channels 2 & 4
            EINTEN  = 0x1B, // Interrupt enable
            ECNTL1  = 0x1C, // Control 1: mode (bits 2:0), filter frequency (bits 5:3)
            CNTL2   = 0x1D, // Control 2: software reset — set bit 0 (SRST=1); auto-clears after reset
        }
    }
}
