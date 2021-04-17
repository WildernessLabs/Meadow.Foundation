using System;
using System.Collections.Generic;

using Meadow.Hardware;

namespace ICs.IOExpanders.TCA9548A
{
    public class Tca9548ABus : II2cBus
    {
        private readonly Tca9548A _tca9548A;
        private readonly byte _busIndex;

        internal Tca9548ABus(Tca9548A tca9548A, int frequency, byte busIndex)
        {
            _tca9548A = tca9548A;
            Frequency = frequency;
            _busIndex = busIndex;
        }

        public int Frequency { get; set; }

        

        public void WriteData(byte peripheralAddress, params byte[] data)
        {
            _tca9548A.BusSelectorSemaphore.Wait(TimeSpan.FromSeconds(10));
            try
            {
                _tca9548A.SelectBus(_busIndex);
                _tca9548A.Bus.WriteData(peripheralAddress, data);
            }
            finally
            {
                _tca9548A.BusSelectorSemaphore.Release();
            }
        }

        public void WriteData(byte peripheralAddress, IEnumerable<byte> data)
        {
            _tca9548A.BusSelectorSemaphore.Wait(TimeSpan.FromSeconds(10));
            try
            {
                _tca9548A.SelectBus(_busIndex);
                _tca9548A.Bus.WriteData(peripheralAddress, data);
            }
            finally
            {
                _tca9548A.BusSelectorSemaphore.Release();
            }
        }

        public void WriteData(byte peripheralAddress, Span<byte> data)
        {
            _tca9548A.BusSelectorSemaphore.Wait(TimeSpan.FromSeconds(10));
            try
            {
                _tca9548A.SelectBus(_busIndex);
                _tca9548A.Bus.WriteData(peripheralAddress, data);
            }
            finally
            {
                _tca9548A.BusSelectorSemaphore.Release();
            }
        }

        [Obsolete("This overload if WriteReadData is obsolete for performance reasons and will be removed in a future release.  Migrate to another overload.", false)]
        public byte[] WriteReadData(byte peripheralAddress, int byteCountToRead, params byte[] dataToWrite)
        {
            _tca9548A.BusSelectorSemaphore.Wait(TimeSpan.FromSeconds(10));
            try
            {
                _tca9548A.SelectBus(_busIndex);
                return _tca9548A.Bus.WriteReadData(peripheralAddress, byteCountToRead, dataToWrite);
            }
            finally
            {
                _tca9548A.BusSelectorSemaphore.Release();
            }
        }

        public void WriteReadData(byte peripheralAddress, Span<byte> writeBuffer, Span<byte> readBuffer)
        {
            _tca9548A.BusSelectorSemaphore.Wait(TimeSpan.FromSeconds(10));
            try
            {
                _tca9548A.SelectBus(_busIndex);
                _tca9548A.Bus.WriteReadData(peripheralAddress, writeBuffer, readBuffer);
            }
            finally
            {
                _tca9548A.BusSelectorSemaphore.Release();
            }
        }

        public byte[] ReadData(byte peripheralAddress, int numberOfBytes)
        {
            _tca9548A.BusSelectorSemaphore.Wait(TimeSpan.FromSeconds(10));
            try
            {
                _tca9548A.SelectBus(_busIndex);
                return _tca9548A.Bus.ReadData(peripheralAddress, numberOfBytes);
            }
            finally
            {
                _tca9548A.BusSelectorSemaphore.Release();
            }
        }
    }
}
