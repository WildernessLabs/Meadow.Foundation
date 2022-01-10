using System;
using Meadow.Hardware;

namespace Meadow.Foundation
{
    /// <summary>
    /// ByteCommsSensorBase abstract class 
    /// </summary>
    /// <typeparam name="UNIT"></typeparam>
    public abstract class ByteCommsSensorBase<UNIT> :
        SamplingSensorBase<UNIT>, IDisposable where UNIT : struct
    {
        //==== events

        //==== internals
        /// <summary>
        /// Peripheral object, i.e. an I2CPeripheral or SpiPeripheral
        /// </summary>
        protected IByteCommunications Peripheral { get; set; }

        //==== properties
        /// <summary>
        /// 
        /// </summary>
        protected Memory<byte> ReadBuffer { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        protected Memory<byte> WriteBuffer { get; private set; }

        //==== ctors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i2cBus"></param>
        /// <param name="address"></param>
        /// <param name="readBufferSize"></param>
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
        /// <param name="readBufferSize">Read buffer size</param>
        /// <param name="writeBufferSize">Write buffer size</param>
        /// <param name="chipSelectMode">Chip select mode</param>
        protected ByteCommsSensorBase(
            ISpiBus spiBus, IDigitalOutputPort? chipSelect,
            int readBufferSize = 8, int writeBufferSize = 8,
            ChipSelectMode chipSelectMode = ChipSelectMode.ActiveLow)
        {
            Peripheral = new SpiPeripheral(spiBus, chipSelect, readBufferSize, writeBufferSize, chipSelectMode);
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
            this.ReadBuffer = new byte[readBufferSize];
            this.WriteBuffer = new byte[writeBufferSize];
        }

        /// <summary>
        /// Dispose object
        /// </summary>
        /// <param name="disposing">is disposing</param>

        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
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