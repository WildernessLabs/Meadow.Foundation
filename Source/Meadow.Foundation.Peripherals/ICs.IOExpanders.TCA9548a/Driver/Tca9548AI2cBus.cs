using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Tca9548a I2C bus
    /// </summary>
    public class Tca9548AI2cBus : II2cBus
    {
        private readonly Tca9548a tca9548a;
        private readonly byte busIndex;

        /// <summary>
        /// I2C bus frequency
        /// </summary>
        public I2cBusSpeed BusSpeed { get; set; }

        internal Tca9548AI2cBus(Tca9548a tca9548A, byte busIndex)
        {
            tca9548a = tca9548A;
            BusSpeed = I2cBusSpeed.Standard;
            this.busIndex = busIndex;
        }

        /// <summary>
        /// Write data to the bus
        /// </summary>
        /// <param name="peripheralAddress">Device address</param>
        /// <param name="data">Data to write</param>
        public void Write(byte peripheralAddress, Span<byte> data)
        {
            tca9548a.BusSelectorSemaphore.Wait(TimeSpan.FromSeconds(10));
            try
            {
                tca9548a.SelectBus(busIndex);
                tca9548a.I2cBus.Write(peripheralAddress, data);
            }
            finally
            {
                tca9548a.BusSelectorSemaphore.Release();
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
            tca9548a.BusSelectorSemaphore.Wait(TimeSpan.FromSeconds(10));
            try
            {
                tca9548a.SelectBus(busIndex);
                tca9548a.I2cBus.Exchange(peripheralAddress, writeBuffer, readBuffer);
            }
            finally
            {
                tca9548a.BusSelectorSemaphore.Release();
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
            tca9548a.BusSelectorSemaphore.Wait(TimeSpan.FromSeconds(10));
            try
            {
                tca9548a.SelectBus(busIndex);
                var data = new byte[numberOfBytes];
                tca9548a.I2cBus.Read(peripheralAddress, data);
                return data;
            }
            finally
            {
                tca9548a.BusSelectorSemaphore.Release();
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        { }

        /// <summary>
        /// Read data from the bus
        /// </summary>
        /// <param name="peripheralAddress">Device address</param>
        /// <param name="readBuffer">Buffer to receive data</param>
        public void Read(byte peripheralAddress, Span<byte> readBuffer)
        {
            tca9548a.BusSelectorSemaphore.Wait(TimeSpan.FromSeconds(10));
            try
            {
                tca9548a.SelectBus(busIndex);
                tca9548a.I2cBus.Read(peripheralAddress, readBuffer);
            }
            finally
            {
                tca9548a.BusSelectorSemaphore.Release();
            }
        }
    }
}