namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Lsm303agr
    {
        const byte ACC_CTRL_REG1_A = 0x20;
        const byte ACC_CTRL_REG4_A = 0x23;
        const byte ACC_OUT_X_L_A = 0x28;
        const byte MAG_CTRL_REG1_M = 0x20;
        const byte MAG_CTRL_REG2_M = 0x21;
        const byte MAG_OUTX_L_REG_M = 0x68;
    }
}