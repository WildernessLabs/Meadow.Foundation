namespace Meadow.Foundation.Sensors.Accelerometers
{
    public partial class Lis2Mdl
    {
        private const byte WHO_AM_I = 0x4F;
        private const byte CTRL_REG1 = 0x60;
        private const byte CTRL_REG2 = 0x61;
        private const byte CTRL_REG3 = 0x62;
        private const byte OUTX_L_REG = 0x68;
    }
}