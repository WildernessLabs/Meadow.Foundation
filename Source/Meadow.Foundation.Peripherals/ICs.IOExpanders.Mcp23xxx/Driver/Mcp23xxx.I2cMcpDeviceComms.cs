using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Mcp23xxx
    {
        internal class I2cMcpDeviceComms : I2cPeripheral, IMcpDeviceComms
        {
            public I2cMcpDeviceComms(II2cBus i2cBus, byte peripheralAddress)
                : base(i2cBus, peripheralAddress)
            {
            }
        }
    }
}