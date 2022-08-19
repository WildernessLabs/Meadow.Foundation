using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Mcp23xxx
    {
        internal class SpiMcpDeviceComms : SpiPeripheral, IMcpDeviceComms
        {
            public SpiMcpDeviceComms(ISpiBus spiBus, IDigitalOutputPort chipSelectPort)
                : base(spiBus, chipSelectPort)
            {
            }
        }
    }
}