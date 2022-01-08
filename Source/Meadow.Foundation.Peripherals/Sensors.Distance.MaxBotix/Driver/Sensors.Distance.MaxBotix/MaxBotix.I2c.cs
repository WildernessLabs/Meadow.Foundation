using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Distance
{
    public partial class MaxBotix
    {
        public MaxBotix(II2cBus i2cBus, SensorType sensor, byte address = (byte)Addresses.Default)
            :base(i2cBus, address)
        {
            sensorType = sensor;
        }

        Length ReadSensorI2c()
        {
            return new Length(Peripheral.ReadRegisterAsUShort(0x51), GetUnitsForSensor(sensorType));
        }
    }
}