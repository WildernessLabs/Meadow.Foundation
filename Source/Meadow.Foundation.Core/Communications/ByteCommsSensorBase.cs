using Meadow.Hardware;
using Meadow.Units;
using System;

namespace Meadow.Foundation
{
    /// <summary>
    /// ByteCommsSensorBase abstract class 
    /// </summary>
    /// <typeparam name="UNIT">The unit type</typeparam>
    public abstract class ByteCommsSensorBase<UNIT> :
        PollingSensorBase<UNIT>, IDisposable where UNIT : struct
    {
        /// <summary>
        /// Peripheral object, i.e. an I2CPeripheral or SpiPeripheral
        /// </summary>
        protected IByteCommunications? Peripheral { get; set; }

        /// <summary>
        /// Creates a new ByteCommsSensorBase object
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="address">The I2C address</param>
        /// <param name="readBufferSize">Read buffer size in bytes</param>
        /// <param name="writeBufferSize"></param>
        protected ByteCommsSensorBase(
            II2cBus i2cBus, byte address,
            int readBufferSize = 8, int writeBufferSize = 8)
        {
            Peripheral = new I2cPeripheral(i2cBus, address, readBufferSize, writeBufferSize);
            Init(readBufferSize, writeBufferSize);
        }

        /// <summary>
        /// ByteCommsSensorBase abstract ctor for SPI
        /// </summary>
        /// <param name="spiBus">SPI bus object</param>
        /// <param name="chipSelect">Chip select port</param>
        /// <param name="busSpeed">The SPI bus speed</param>
        /// <param name="busMode">The SPI bus mode (0-3)</param>
        /// <param name="readBufferSize">Read buffer size</param>
        /// <param name="writeBufferSize">Write buffer size</param>
        /// <param name="chipSelectMode">Chip select mode</param>
        protected ByteCommsSensorBase(
            ISpiBus spiBus,
            IDigitalOutputPort? chipSelect,
            Frequency busSpeed,
            SpiClockConfiguration.Mode busMode = SpiClockConfiguration.Mode.Mode0,
            int readBufferSize = 8,
            int writeBufferSize = 8,
            ChipSelectMode chipSelectMode = ChipSelectMode.ActiveLow)
        {
            Peripheral = new SpiPeripheral(spiBus, chipSelect, busSpeed, busMode, readBufferSize, writeBufferSize, chipSelectMode);
            Init(readBufferSize, writeBufferSize);
        }

        /// <summary>
        /// Simple constructor for peripherals that don't use a bus 
        /// and don't need an IByteCommunications
        /// </summary>
        /// <param name="readBufferSize"></param>
        /// <param name="writeBufferSize"></param>
        protected virtual void Init(int readBufferSize = 8, int writeBufferSize = 8)
        {
            ReadBuffer = new byte[readBufferSize];
            WriteBuffer = new byte[writeBufferSize];
        }

        /// <summary>
        /// Dispose object
        /// </summary>
        /// <param name="disposing">is disposing</param>

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                base.StopUpdating();
            }
        }

        /// <summary>
        /// Dispose managed resources
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(true);
        }
    }
}