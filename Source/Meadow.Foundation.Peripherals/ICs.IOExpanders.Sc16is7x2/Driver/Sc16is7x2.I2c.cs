﻿using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Sc16is7x2 : II2cPeripheral
    {
        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        /// <summary>
        /// The maximum I2C bus speed for the peripheral
        /// </summary>
        public I2cBusSpeed MaxI2cBusSpeed => I2cBusSpeed.Fast;

        private readonly II2cCommunications? _i2cComms;

        /// <summary>
        /// Create a new Sc16is7x2 object
        /// </summary>
        /// <param name="i2cBus">The I2C bus connected to the peripheral</param>
        /// <param name="address">The I2C address</param>
        /// <param name="oscillatorFrequency">The frequency of the oscillator connected to the SC16IS</param>
        /// <param name="irq">An optional interrupt port used to detect change conditions on the peripheral</param>
        /// <param name="latchGpioInterrupt">An interrupt triggered by a GPIO change, will remain until handled. State will also be kept.</param>
        public Sc16is7x2(II2cBus i2cBus, Frequency oscillatorFrequency, Addresses address = Addresses.Default, IDigitalInterruptPort? irq = null, bool latchGpioInterrupt = false)
            : this(oscillatorFrequency, irq, latchGpioInterrupt)
        {
            _i2cComms = new I2cCommunications(i2cBus, (byte)address);
        }
    }
}