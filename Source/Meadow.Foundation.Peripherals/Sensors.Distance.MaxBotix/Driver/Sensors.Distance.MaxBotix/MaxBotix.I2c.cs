using Meadow.Devices;
using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Distance
{
    public partial class MaxBotix
    {
        II2cPeripheral ic2Peripheral;

        public MaxBotix(IMeadowDevice device, II2cBus i2cBus, byte address)
        {
            ic2Peripheral = new I2cPeripheral(i2cBus, address);
        }
    }
}