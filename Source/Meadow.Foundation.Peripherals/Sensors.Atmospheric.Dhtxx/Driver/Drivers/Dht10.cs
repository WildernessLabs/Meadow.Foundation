using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Represents a DHT10 temp / humidity sensor
    /// -40 - 80 celius +/- 0.5
    /// 0 - 99.9% humidity +/- 3%
    /// </summary>
    public class Dht10 : DhtBase
    {
        private const byte CMD_INIT = 0b_1110_0001;
        private const byte CMD_START = 0b_1010_1100;
        private const byte CMD_SOFTRESET = 0b_1011_1010;

        /// <summary>
        /// Create a new Dht10 object.
        /// </summary>
        /// <param name="address">Address of the Dht12 (default = 0x27).</param>
        /// <param name="i2cBus">I2C bus (default = 100 KHz).</param>
        public Dht10(II2cBus i2cBus, byte address = (byte)Addresses.Default)
            : base(i2cBus, address)
        {
            BusComms?.Write(CMD_SOFTRESET);
            Thread.Sleep(20);
            BusComms?.Write(CMD_INIT);
        }

        internal override void ReadDataI2c()
        {
            WasLastReadSuccessful = true;

            BusComms?.Write(CMD_START);
            Thread.Sleep(75);

            //data stored in the read buffer
            BusComms?.Read(ReadBuffer.Span);
        }

        internal override float GetHumidity()
        {
            int value = (((ReadBuffer.Span[1] << 8) | ReadBuffer.Span[2]) << 4) | ReadBuffer.Span[3] >> 4;

            return (float)(value / Math.Pow(2, 20) * 100);
        }

        internal override float GetTemperature()
        {
            int value = ((((ReadBuffer.Span[3] & 0b_0000_1111) << 8) | ReadBuffer.Span[4]) << 8) | ReadBuffer.Span[5];

            float temperature = (float)(value / Math.Pow(2, 20) * 200 - 50);

            return temperature;
        }
    }
}