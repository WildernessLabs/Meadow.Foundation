using System.Threading;
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
            communication = CommunicationType.I2C;

            StartI2cSensor(address);
        }

        void StartI2cSensor(byte address)
        {
            Peripheral.ReadRegister(address);
            Thread.Sleep(100);
        }

        Length ReadSensorI2c()
        {
            return new Length(Peripheral.ReadRegisterAsUShort(0x51), GetUnitsForSensor(sensorType));
        }
    }
}