using Meadow.Hardware;
using Meadow.Units;
using System.Threading;

namespace Meadow.Foundation.Sensors.Distance
{
    public partial class MaxBotix : II2cPeripheral
    {
        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte I2cDefaultAddress => (byte)Address.Default;

        /// <summary>
        /// Creates a new MaxBotix object communicating over I2C
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="sensor">The distance sensor type</param>
        /// <param name="address">The I2C address</param>
        public MaxBotix(II2cBus i2cBus, SensorType sensor, byte address = (byte)Address.Default)
            : base(i2cBus, address)
        {
            sensorType = sensor;
            communication = CommunicationType.I2C;

            StartI2cSensor(address);
        }

        void StartI2cSensor(byte address)
        {
            BusComms.ReadRegister(address);
            Thread.Sleep(100);
        }

        Length ReadSensorI2c()
        {
            return new Length(BusComms.ReadRegisterAsUShort(0x51), GetUnitsForSensor(sensorType));
        }
    }
}