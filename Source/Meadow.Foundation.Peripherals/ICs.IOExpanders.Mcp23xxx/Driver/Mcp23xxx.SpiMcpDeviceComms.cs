using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Mcp23xxx
    {
        internal class SpiMcpDeviceComms : SpiPeripheral, IMcpDeviceComms
        {
            public SpiMcpDeviceComms(ISpiBus spiBus,
                IDigitalOutputPort chipSelectPort,
                Frequency busSpeed,
                SpiClockConfiguration.Mode busMode)
                : base(spiBus, chipSelectPort, busSpeed, busMode)
            {
            }
        }
    }
}