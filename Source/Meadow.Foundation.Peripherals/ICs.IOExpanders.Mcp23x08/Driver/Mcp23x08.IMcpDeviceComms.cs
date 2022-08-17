using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Mcp23x08
    {
        internal interface IMcpDeviceComms
        {
            byte ReadRegister(byte address);
            void WriteRegister(byte address, byte value);
        }

        internal class I2cMcpDeviceComms : I2cPeripheral, IMcpDeviceComms
        {
            public I2cMcpDeviceComms(II2cBus bus, byte peripheralAddress)
                :base(bus, peripheralAddress)
            {
            }
        }
    }
}