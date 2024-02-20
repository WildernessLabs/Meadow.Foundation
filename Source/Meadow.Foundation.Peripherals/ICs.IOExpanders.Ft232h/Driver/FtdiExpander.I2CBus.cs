using Meadow.Hardware;
using System;
using System.IO;
using System.Threading;

namespace Meadow.Foundation.ICs.IOExpanders;

public abstract partial class FtdiExpander
{
    public abstract class I2CBus : II2cBus
    {
        public I2cBusSpeed BusSpeed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private SpinWait _spinWait = new();

        public abstract void Configure();
        public abstract void Start();
        public abstract void Stop();
        public abstract void Idle();
        public abstract TransferStatus SendDataByte(byte data);
        public abstract byte ReadDataByte(bool ackAfterRead);
        protected const byte NumberCycles = 6;
        protected const byte MaskGpio = 0xF8;

        public enum TransferStatus
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

            Stop();
        }

        public void Dispose()
        {
        }

        public void Exchange(byte peripheralAddress, Span<byte> writeBuffer, Span<byte> readBuffer)
        {
            Write(peripheralAddress, writeBuffer);
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