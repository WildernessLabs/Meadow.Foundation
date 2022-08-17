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

        //ToDo move into it's own file
        internal class I2cMcpDeviceComms : I2cPeripheral, IMcpDeviceComms
        {
            public I2cMcpDeviceComms(II2cBus i2cBus, byte peripheralAddress)
                :base(i2cBus, peripheralAddress)
            {
            }
        }

        internal class SpiMcpDeviceComms : SpiPeripheral, IMcpDeviceComms
        {
            public SpiMcpDeviceComms(ISpiBus spiBus, IDigitalOutputPort chipSelectPort)
                :base(spiBus, chipSelectPort)
            {
            }
        }
    }
}