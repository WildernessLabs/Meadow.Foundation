namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Lis2Mdl
    {
        private const byte WHO_AM_I = 0x4F;
        private const byte CFG_REG_A = 0x60;
        private const byte CFG_REG_B = 0x61;
        private const byte CFG_REG_C = 0x62;
        private const byte OUTX_L_REG = 0x68;
    }
}