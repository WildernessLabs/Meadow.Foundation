using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public sealed class Cp2112I2cBus : II2cBus, IDisposable
    {
        private bool _isDisposed;

        internal Cp2112I2cBus(I2cBusSpeed busSpeed)
        {
            BusSpeed = busSpeed;
        }

        public I2cBusSpeed BusSpeed { get; set; }

        private void Dispose(bool _)
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
            }
        }

        ~Cp2112I2cBus()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Exchange(byte peripheralAddress, Span<byte> writeBuffer, Span<byte> readBuffer)
        {
            Write(peripheralAddress, writeBuffer);
            Read(peripheralAddress, readBuffer);
        }

        public void Read(byte peripheralAddress, Span<byte> readBuffer)
        {
            throw new NotImplementedException();
        }

        public void Write(byte peripheralAddress, Span<byte> writeBuffer)
        {
            throw new NotImplementedException();
        }
    }
}