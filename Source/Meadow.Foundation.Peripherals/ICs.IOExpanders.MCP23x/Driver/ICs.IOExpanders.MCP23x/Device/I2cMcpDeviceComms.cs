using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders.Device
{
    public class I2cMcpDeviceComms : I2cPeripheral, IMcpDeviceComms
    {
        public I2cMcpDeviceComms(II2cBus bus, byte peripheralAddress) : base(bus, peripheralAddress)
        {
        }
    }
}
