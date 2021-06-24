using System;
using Meadow.Hardware;

namespace Meadow.Foundation
{
    public abstract class ByteCommsSensorBase<UNIT> :
        SamplingSensorBase<UNIT>, IDisposable where UNIT : struct
    {
        //==== events

        //==== internals
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
        protected ByteCommsSensorBase(
            II2cBus i2cBus, byte address,
            int readBufferSize = 8, int writeBufferSize = 8)
        {
            Peripheral = new I2cPeripheral(i2cBus, address, readBufferSize, writeBufferSize);
            Init(readBufferSize, writeBufferSize);
        }

        protected ByteCommsSensorBase(
            ISpiBus spiBus, IDigitalOutputPort? chipSelect,
            int readBufferSize = 8, int writeBufferSize = 8,
            ChipSelectMode csMode = ChipSelectMode.ActiveLow)
        {
            Peripheral = new SpiPeripheral(spiBus, chipSelect, readBufferSize, writeBufferSize, csMode);
            Init(readBufferSize, writeBufferSize);
        }

        protected virtual void Init(int readBufferSize = 8, int writeBufferSize = 8)
        {
            this.ReadBuffer = new byte[readBufferSize];
            this.WriteBuffer = new byte[writeBufferSize];
        }

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