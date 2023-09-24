namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Lis3Mdl
    {
        private const byte WHO_AM_I = 0x0F;
        private const byte CTRL_REG1 = 0x20;
        private const byte CTRL_REG2 = 0x21;
        private const byte CTRL_REG3 = 0x22;
        private const byte CTRL_REG4 = 0x23;
        private const byte CTRL_REG5 = 0x24;
        private const byte OUTX_L_REG = 0x28;
        private const byte TEMP_L_REG = 0x2E;
    }
}