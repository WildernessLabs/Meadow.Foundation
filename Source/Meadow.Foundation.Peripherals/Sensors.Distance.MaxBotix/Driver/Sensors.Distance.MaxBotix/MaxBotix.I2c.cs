using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Distance
{
    public partial class MaxBotix
    {
        II2cPeripheral ic2Peripheral;

        public MaxBotix(IMeadowDevice device, II2cBus i2cBus, byte address = (byte)Addresses.Default)
        {
            ic2Peripheral = new I2cPeripheral(i2cBus, address);
        }

        Length ReadSensorI2c()
        {
            var value = ic2Peripheral.ReadUShorts(0x51, 1);

            return new Length(value[0], GetUnitsForSensor(sensorType));
        }
    }
}