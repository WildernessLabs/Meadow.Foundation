using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Barometric
{
    public class GY63
    {
        public GY63(II2cBus i2c, byte address = 0x76)
        {
            //address is either 0x76 or 0x77
        }

        public GY63(ISpiBus spi)
        {
        }
    }
}
