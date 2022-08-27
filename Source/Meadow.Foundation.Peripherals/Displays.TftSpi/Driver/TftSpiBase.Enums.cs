namespace Meadow.Foundation.Displays
{
    public abstract partial class TftSpiBase
    {
        protected enum LcdCommand
        {
            CASET = 0x2A,
            RASET = 0x2B,
            RAMWR = 0x2C,
            RADRD = 0x2E
        };

        // TODO: @Adrian - should this use the graphics rotation?
        public enum Rotation
        {
            Normal, //zero
            Rotate_90, //in degrees
            Rotate_180,
            Rotate_270,
        }

        protected enum Register : byte
        {
            NO_OP = 0x0,
            MADCTL = 0x36,
            MADCTL_MY = 0x80,
            MADCTL_MX = 0x40,
            MADCTL_MV = 0x20,
            MADCTL_ML = 0x10,
            MADCTL_RGB = 0x00,
            MADCTL_BGR = 0X08,
            MADCTL_MH = 0x04,
            MADCTL_SS = 0x02,
            MADCTL_GS = 0x01,
            COLOR_MODE = 0x3A,
        }
    }
}