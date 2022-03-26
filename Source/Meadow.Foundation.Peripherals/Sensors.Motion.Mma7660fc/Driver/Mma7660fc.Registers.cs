namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Mma7660fc
    {
        public enum Registers : byte
        {
            XOUT = 0x00,
            YOUT = 0x01,
            ZOUT = 0x02,
            TILT = 0x03,
            Mode = 0x07,
            SleepRate = 0x08
        }
    }
}
