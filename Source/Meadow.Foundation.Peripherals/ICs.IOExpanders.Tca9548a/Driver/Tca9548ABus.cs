using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Tca9548a I2C bus
    /// </summary>
    public class Tca9548aBus : II2cBus
    {
        private readonly Tca9548a _tca9548a;
        private readonly byte _busIndex;

        internal Tca9548aBus(Tca9548a tca9548A, int frequency, byte busIndex)
        {
            _tca9548a = tca9548A;
            BusSpeed = I2cBusSpeed.Standard;
            _busIndex = busIndex;
        }

        /// <summary>
        /// I2C bus frequency
        /// </summary>
        public I2cBusSpeed BusSpeed { get; set; }

        /// <summary>
        /// Write data to the bus
        /// </summary>
        /// <param name="peripheralAddress">Device address</param>
        /// <param name="data">Data to write</param>
        public void WriteData(byte peripheralAddress, params byte[] data)
        {
            _tca9548a.BusSelectorSemaphore.Wait(TimeSpan.FromSeconds(10));
            try
            {
                _tca9548a.SelectBus(_busIndex);
                _tca9548a.Bus.Write(peripheralAddress, data);
            }
            finally
            {
                _tca9548a.BusSelectorSemaphore.Release();
            }
        }

        /*
        public void WriteData(byte peripheralAddress, IEnumerable<byte> data)
        {
            _tca9548a.BusSelectorSemaphore.Wait(TimeSpan.FromSeconds(10));
            try
            {
                _tca9548a.SelectBus(_busIndex);
                _tca9548a.Bus.Write(peripheralAddress, data);
            }
            finally
            {
                _tca9548a.BusSelectorSemaphore.Release();
            }
        }*/

        /// <summary>
        /// Write data to the bus
        /// </summary>
        /// <param name="peripheralAddress">Device address</param>
        /// <param name="data">Data to write</param>
        public void Write(byte peripheralAddress, Span<byte> data)
        {
            _tca9548a.BusSelectorSemaphore.Wait(TimeSpan.FromSeconds(10));
            try
            {
                _tca9548a.SelectBus(_busIndex);
                _tca9548a.Bus.Write(peripheralAddress, data);
            }
            finally
            {
                _tca9548a.BusSelectorSemaphore.Release();
            }
        }

        /// <summary>
        /// Exchange data
        /// </summary>
        /// <param name="peripheralAddress">Device address</param>
        /// <param name="writeBuffer">Buffer with data to write</param>
        /// <param name="readBuffer">Buffer to receive data</param>
        public void Exchange(byte peripheralAddress, Span<byte> writeBuffer, Span<byte> readBuffer)
        {
            _tca9548a.BusSelectorSemaphore.Wait(TimeSpan.FromSeconds(10));
            try
            {
                _tca9548a.SelectBus(_busIndex);
                _tca9548a.Bus.Exchange(peripheralAddress, writeBuffer, readBuffer);
            }
            finally
            {
                _tca9548a.BusSelectorSemaphore.Release();
            }
        }

        /// <summary>
        /// Read data from the I2C bus
        /// </summary>
        /// <param name="peripheralAddress">Device address</param>
        /// <param name="numberOfBytes">Number of bytes to read</param>
        /// <returns></returns>
        public byte[] ReadData(byte peripheralAddress, int numberOfBytes)
        {
            _tca9548a.BusSelectorSemaphore.Wait(TimeSpan.FromSeconds(10));
            try
            {
                _tca9548a.SelectBus(_busIndex);
                var data = new byte[numberOfBytes];
                _tca9548a.Bus.Read(peripheralAddress, data);
                return data;
            }
            finally
            {
                _tca9548a.BusSelectorSemaphore.Release();
            }
        }

        /// <summary>
        /// Write data to the bus from a span
        /// </summary>
        /// <param name="peripheralAddress">Device address</param>
        /// <param name="data">Data to write</param>
        /// <param name="length">Length of data to write</param>
        public void WriteData(byte peripheralAddress, Span<byte> data, int length)
        {
            _tca9548a.BusSelectorSemaphore.Wait(TimeSpan.FromSeconds(10));
            try
            {
                _tca9548a.SelectBus(_busIndex);
                _tca9548a.Bus.Write(peripheralAddress, data[..length]);
            }
            finally
            {
                _tca9548a.BusSelectorSemaphore.Release();
            }
        }

        /// <summary>
        /// Exchange data
        /// </summary>
        /// <param name="peripheralAddress">Device address</param>
        /// <param name="writeBuffer">Buffer with data to write</param>
        /// <param name="writeCount">Number of bytes to write</param>
        /// <param name="readBuffer">Buffer to receive data</param>
        /// <param name="readCount">Number of bytes to read</param>
        public void ExchangeData(byte peripheralAddress, Span<byte> writeBuffer, int writeCount, Span<byte> readBuffer, int readCount)
        {
            _tca9548a.BusSelectorSemaphore.Wait(TimeSpan.FromSeconds(10));
            try
            {
                _tca9548a.SelectBus(_busIndex);
                _tca9548a.Bus.Exchange(peripheralAddress, writeBuffer[0..writeCount], readBuffer[0..readCount]);
            }
            finally
            {
                _tca9548a.BusSelectorSemaphore.Release();
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Read data from the bus
        /// </summary>
        /// <param name="peripheralAddress">Device address</param>
        /// <param name="readBuffer">Buffer to receive data</param>
        public void Read(byte peripheralAddress, Span<byte> readBuffer)
        {
            _tca9548a.BusSelectorSemaphore.Wait(TimeSpan.FromSeconds(10));
            try
            {
                _tca9548a.SelectBus(_busIndex);
                _tca9548a.Bus.Read(peripheralAddress, readBuffer);
            }
            finally
            {
                _tca9548a.BusSelectorSemaphore.Release();
            }
        }
    }
}