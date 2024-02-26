using Meadow.Hardware;
using System;
using System.IO;
using System.Threading;

namespace Meadow.Foundation.ICs.IOExpanders;

public abstract partial class FtdiExpander
{
    public abstract class I2CBus : II2cBus
    {
        protected internal readonly FtdiExpander _expander;

        public I2cBusSpeed BusSpeed { get; set; }

        private SpinWait _spinWait = new();

        internal abstract void Configure();
        internal abstract void Start();
        internal abstract void Stop();
        internal abstract void Idle();
        internal abstract TransferStatus SendDataByte(byte data);
        internal abstract byte ReadDataByte(bool ackAfterRead);
        protected internal const byte MaskGpio = 0xF8;

        internal I2CBus(FtdiExpander expander, I2cBusSpeed busSpeed)
        {
            _expander = expander;
            BusSpeed = busSpeed;
        }

        internal enum TransferStatus
        {
            Ack = 0,
            Nack
        }

        protected static class PinDirection
        {
            public const byte SDAinSCLin = 0x00;
            public const byte SDAinSCLout = 0x01;
            public const byte SDAoutSCLin = 0x02;
            public const byte SDAoutSCLout = 0x03;
        }

        protected static class PinData
        {
            public const byte SDAloSCLhi = 0x01;
            public const byte SDAhiSCLhi = 0x03;
            public const byte SDAloSCLlo = 0x00;
            public const byte SDAhiSCLlo = 0x02;
        }

        protected void Wait(int spinCount)
        {
            for (var i = 0; i < spinCount; i++)
            {
                _spinWait.SpinOnce();
            }
        }

        private TransferStatus SendAddressByte(byte address, bool isRead)
        {
            // Set address for read or write
            address <<= 1;
            if (isRead == true)
            {
                address |= 0x01;
            }

            return SendDataByte(address);
        }

        public void Write(byte peripheralAddress, Span<byte> writeBuffer)
        {
            Write(peripheralAddress, writeBuffer, true);
        }

        public void Write(byte peripheralAddress, Span<byte> writeBuffer, bool terminatingStop)
        {
            Start();
            if (SendAddressByte(peripheralAddress, false) == TransferStatus.Nack)
            {
                Stop();
                throw new IOException($"Error writing device while setting up address");
            }

            for (int i = 0; i < writeBuffer.Length; i++)
            {
                if (SendDataByte(writeBuffer[i]) == TransferStatus.Nack)
                {
                    Stop();
                    throw new IOException($"Error writing device on byte {i}");
                }
            }

            if (terminatingStop)
            {
                Stop();
            }
        }

        public void Dispose()
        {
        }

        public void Exchange(byte peripheralAddress, Span<byte> writeBuffer, Span<byte> readBuffer)
        {
            Write(peripheralAddress, writeBuffer, false);
            Read(peripheralAddress, readBuffer);
        }

        public void Read(byte peripheralAddress, Span<byte> readBuffer)
        {
            Start();
            if (SendAddressByte(peripheralAddress, true) == TransferStatus.Nack)
            {
                Stop();
                throw new IOException($"Error reading device while setting up address");
            }

            for (int i = 0; i < readBuffer.Length - 1; i++)
            {
                readBuffer[i] = ReadDataByte(true);
            }

            if (readBuffer.Length > 0)
            {
                readBuffer[readBuffer.Length - 1] = ReadDataByte(false);
            }

            Stop();
        }
    }
}