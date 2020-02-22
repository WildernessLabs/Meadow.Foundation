using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meadow.Foundation.Transceivers
{
    public class SX127x : SpiPeripheral
    {
        private const byte REG_VERSION = 0x42;

        public SX127x(ISpiBus bus, IDigitalOutputPort chipSelect) 
            : base(bus, chipSelect)
        {
        }

        public byte GetVersion()
        {
            try
            {
                return this.ReadRegister(REG_VERSION);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ReadDeviceID " + ex.Message);
                return 0xff;
            }
        }
    }
}
