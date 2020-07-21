using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders.Device
{
    public class SpiMcpDeviceComms : SpiPeripheral, IMcpDeviceComms
    {
        public SpiMcpDeviceComms(ISpiBus bus, IDigitalOutputPort chipSelect) : base(
            bus,
            chipSelect)
        {
        }
    }
}
