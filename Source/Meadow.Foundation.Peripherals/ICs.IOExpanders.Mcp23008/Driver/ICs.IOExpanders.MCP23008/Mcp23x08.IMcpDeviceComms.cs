using System;
using System.Linq;
using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Mcp23x08
    {
        internal interface IMcpDeviceComms
        {
            byte ReadRegister(byte address);
            byte[] ReadRegisters(byte address, ushort length);
            void WriteRegister(byte address, byte value);
            void WriteRegisters(byte address, byte[] data);
        }

        internal class I2cMcpDeviceComms : I2cPeripheral, IMcpDeviceComms
        {
            public I2cMcpDeviceComms(II2cBus bus, byte peripheralAddress)
                : base(bus, peripheralAddress)
            {
            }
        }

        internal class SpiMcpDeviceComms : SpiPeripheralWithAddress, IMcpDeviceComms
        {
            public SpiMcpDeviceComms(ISpiBus bus, IDigitalOutputPort chipSelect,
                byte peripheralAddress)
                : base(bus, chipSelect, peripheralAddress)
            {
            }
        }

    }
}
