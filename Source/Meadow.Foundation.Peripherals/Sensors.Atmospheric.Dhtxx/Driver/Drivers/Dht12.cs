﻿using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Represents a DHT10 temp / humidity sensor
    /// -20 - 60 celsius +/- 0.5
    /// 0 - 95% humidity +/- 4%
    /// Currently only supports I2C
    /// </summary>
    public class Dht12 : DhtBase, II2cPeripheral
    {
        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        /// <summary>
        /// Create a new Dht12 object
        /// </summary>
        /// <param name="address">Address of the Dht12 (default = 0x27)</param>
        /// <param name="i2cBus">I2C bus (default = 100 KHz)</param>
        public Dht12(II2cBus i2cBus, byte address = (byte)Addresses.Default)
            : base(i2cBus, address)
        { }

        internal override float GetHumidity()
        {
            return ReadBuffer.Span[0] + ReadBuffer.Span[1] * 0.1f;
        }

        internal override float GetTemperature()
        {
            var temperature = (ReadBuffer.Span[2] & 0x7F) + ReadBuffer.Span[3] * 0.1f;
            // if MSB is 1, value is negative
            temperature = ((ReadBuffer.Span[2] & 0x80) == 0 ? temperature : -temperature);

            return temperature;
        }
    }
}