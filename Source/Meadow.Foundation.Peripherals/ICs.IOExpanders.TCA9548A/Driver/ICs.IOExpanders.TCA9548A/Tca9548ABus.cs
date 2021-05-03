using System;
using System.Collections.Generic;

using Meadow.Hardware;

namespace ICs.IOExpanders.TCA9548A
{
    public class Tca9548aBus : II2cBus
    {
        private readonly Tca9548a _tca9548a;
        private readonly byte _busIndex;

        internal Tca9548aBus(Tca9548a tca9548A, int frequency, byte busIndex)
        {
            _tca9548a = tca9548A;
            Frequency = frequency;
            _busIndex = busIndex;
        }

        public int Frequency { get; set; }

        

        public void WriteData(byte peripheralAddress, params byte[] data)
        {
            _tca9548a.BusSelectorSemaphore.Wait(TimeSpan.FromSeconds(10));
            try
            {
                _tca9548a.SelectBus(_busIndex);
                _tca9548a.Bus.WriteData(peripheralAddress, data);
            }
            finally
            {
                _tca9548a.BusSelectorSemaphore.Release();
            }
        }

        public void WriteData(byte peripheralAddress, IEnumerable<byte> data)
        {
            _tca9548a.BusSelectorSemaphore.Wait(TimeSpan.FromSeconds(10));
            try
            {
                _tca9548a.SelectBus(_busIndex);
                _tca9548a.Bus.WriteData(peripheralAddress, data);
            }
            finally
            {
                _tca9548a.BusSelectorSemaphore.Release();
            }
        }

        public void WriteData(byte peripheralAddress, Span<byte> data)
        {
            _tca9548a.BusSelectorSemaphore.Wait(TimeSpan.FromSeconds(10));
            try
            {
                _tca9548a.SelectBus(_busIndex);
                _tca9548a.Bus.WriteData(peripheralAddress, data);
            }
            finally
            {
                _tca9548a.BusSelectorSemaphore.Release();
            }
        }

        [Obsolete("This overload if WriteReadData is obsolete for performance reasons and will be removed in a future release.  Migrate to another overload.", false)]
        public byte[] WriteReadData(byte peripheralAddress, int byteCountToRead, params byte[] dataToWrite)
        {
            _tca9548a.BusSelectorSemaphore.Wait(TimeSpan.FromSeconds(10));
            try
            {
                _tca9548a.SelectBus(_busIndex);
                return _tca9548a.Bus.WriteReadData(peripheralAddress, byteCountToRead, dataToWrite);
            }
            finally
            {
                _tca9548a.BusSelectorSemaphore.Release();
            }
        }

        public void WriteReadData(byte peripheralAddress, Span<byte> writeBuffer, Span<byte> readBuffer)
        {
            _tca9548a.BusSelectorSemaphore.Wait(TimeSpan.FromSeconds(10));
            try
            {
                _tca9548a.SelectBus(_busIndex);
                _tca9548a.Bus.WriteReadData(peripheralAddress, writeBuffer, readBuffer);
            }
            finally
            {
                _tca9548a.BusSelectorSemaphore.Release();
            }
        }

        public byte[] ReadData(byte peripheralAddress, int numberOfBytes)
        {
            _tca9548a.BusSelectorSemaphore.Wait(TimeSpan.FromSeconds(10));
            try
            {
                _tca9548a.SelectBus(_busIndex);
                return _tca9548a.Bus.ReadData(peripheralAddress, numberOfBytes);
            }
            finally
            {
                _tca9548a.BusSelectorSemaphore.Release();
            }
        }

        public void WriteData(byte peripheralAddress, byte[] data, int length)
        {
            throw new NotImplementedException();
        }

        public void WriteReadData(byte peripheralAddress, Span<byte> writeBuffer, int writeCount, Span<byte> readBuffer, int readCount)
        {
            throw new NotImplementedException();
        }
    }
}
