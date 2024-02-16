﻿using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Temperature
{
    /// <summary>
    /// TMP102 Temperature sensor object
    /// </summary>    
    public partial class Lm75 : ByteCommsSensorBase<Units.Temperature>,
        ITemperatureSensor, II2cPeripheral
    {
        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        /// <summary>
        /// The Temperature value from the last reading
        /// </summary>
        public Units.Temperature? Temperature { get; protected set; }

        /// <summary>
        /// Create a new TMP102 object using the default configuration for the sensor
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="address">I2C address of the sensor</param>
        public Lm75(II2cBus i2cBus, byte address = (byte)Addresses.Default)
            : base(i2cBus, address)
        {
        }

        /// <summary>
        /// Update the Temperature property
        /// </summary>
        protected override Task<Units.Temperature> ReadSensor()
        {
            BusComms?.Write((byte)Registers.LM_TEMP);

            BusComms?.ReadRegister((byte)Registers.LM_TEMP, ReadBuffer.Span[0..2]);

            // Details in Datasheet P10
            double temp;
            ushort raw = (ushort)((ReadBuffer.Span[0] << 3) | (ReadBuffer.Span[1] >> 5));
            if ((ReadBuffer.Span[0] & 0x80) == 0)
            {
                // temperature >= 0
                temp = raw * 0.125;
            }
            else
            {
                raw |= 0xF800;
                raw = (ushort)(~raw + 1);

                temp = raw * (-1) * 0.125;
            }

            return Task.FromResult(new Units.Temperature((float)Math.Round(temp, 1), Units.Temperature.UnitType.Celsius));
        }
    }
}