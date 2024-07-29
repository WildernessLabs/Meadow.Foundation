using Meadow.Hardware;
using System;
using System.IO;
using System.Threading;

namespace Meadow.Foundation.ICs.IOExpanders;

/// <summary>
/// Represents an FTDI I2C bus expander.
/// </summary>
public abstract partial class FtdiExpander
{
    /// <summary>
    /// Represents an I2C bus for the FTDI expander.
    /// </summary>
    public abstract class I2CBus : II2cBus
    {
        /// <summary>
        /// The FTDI expander instance associated with this I2C bus.
        /// </summary>
        protected internal readonly FtdiExpander _expander;

        /// <summary>
        /// Gets or sets the speed of the I2C bus.
        /// </summary>
        public I2cBusSpeed BusSpeed { get; set; }

        private SpinWait _spinWait = new();

        /// <summary>
        /// Configures the I2C bus.
        /// </summary>
        internal abstract void Configure();

        /// <summary>
        /// Starts the I2C communication.
        /// </summary>
        internal abstract void Start();

        /// <summary>
        /// Stops the I2C communication.
        /// </summary>
        internal abstract void Stop();

        /// <summary>
        /// Idles the I2C bus.
        /// </summary>
        internal abstract void Idle();

        /// <summary>
        /// Sends a data byte over the I2C bus.
        /// </summary>
        /// <param name="data">The data byte to send.</param>
        /// <returns>The transfer status after sending the data byte.</returns>
        internal abstract TransferStatus SendDataByte(byte data);

        /// <summary>
        /// Reads a data byte from the I2C bus.
        /// </summary>
        /// <param name="ackAfterRead">Whether to acknowledge after reading the byte.</param>
        /// <returns>The read data byte.</returns>
        internal abstract byte ReadDataByte(bool ackAfterRead);

        /// <summary>
        /// Mask for GPIO operations.
        /// </summary>
        protected internal const byte MaskGpio = 0xF8;

        /// <summary>
        /// Initializes a new instance of the <see cref="I2CBus"/> class.
        /// </summary>
        /// <param name="expander">The FTDI expander instance.</param>
        /// <param name="busSpeed">The speed of the I2C bus.</param>
        internal I2CBus(FtdiExpander expander, I2cBusSpeed busSpeed)
        {
            _expander = expander;
            BusSpeed = busSpeed;
        }

        /// <summary>
        /// Represents the status of an I2C transfer.
        /// </summary>
        internal enum TransferStatus
        {
            Ack = 0,
            Nack
        }

        /// <summary>
        /// Contains constants for pin directions.
        /// </summary>
        protected static class PinDirection
        {
            /// <summary>
            /// SDA in and SCL in
            /// </summary>
            public const byte SDAinSCLin = 0x00;
            /// <summary>
            /// SDA in and SCL out
            /// </summary>
            public const byte SDAinSCLout = 0x01;
            /// <summary>
            /// SDA out and SCL in
            /// </summary>
            public const byte SDAoutSCLin = 0x02;
            /// <summary>
            /// SDA out and SCL out
            /// </summary>
            public const byte SDAoutSCLout = 0x03;
        }

        /// <summary>
        /// Contains constants for pin data states.
        /// </summary>
        protected static class PinData
        {
            /// <summary>
            /// SDA low and SCL high
            /// </summary>
            public const byte SDAloSCLhi = 0x01;

            /// <summary>
            /// SDA high and SCL high
            /// </summary>
            public const byte SDAhiSCLhi = 0x03;
            /// <summary>
            /// SDA low and SCL low
            /// </summary>
            public const byte SDAloSCLlo = 0x00;
            /// <summary>
            /// SDA high and SCL low
            /// </summary>
            public const byte SDAhiSCLlo = 0x02;
        }

        /// <summary>
        /// Waits for a specified number of spin counts.
        /// </summary>
        /// <param name="spinCount">The number of spin counts to wait.</param>
        protected void Wait(int spinCount)
        {
            for (var i = 0; i < spinCount; i++)
            {
                _spinWait.SpinOnce();
            }
        }

        /// <summary>
        /// Sends the address byte over the I2C bus.
        /// </summary>
        /// <param name="address">The address byte to send.</param>
        /// <param name="isRead">Indicates if the operation is a read operation.</param>
        /// <returns>The transfer status after sending the address byte.</returns>
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

        /// <summary>
        /// Writes data to a peripheral device.
        /// </summary>
        /// <param name="peripheralAddress">The address of the peripheral device.</param>
        /// <param name="writeBuffer">The data buffer to write.</param>
        public void Write(byte peripheralAddress, Span<byte> writeBuffer)
        {
            Write(peripheralAddress, writeBuffer, true);
        }

        /// <summary>
        /// Writes data to a peripheral device with an option to send a stop condition.
        /// </summary>
        /// <param name="peripheralAddress">The address of the peripheral device.</param>
        /// <param name="writeBuffer">The data buffer to write.</param>
        /// <param name="terminatingStop">Whether to send a stop condition after writing.</param>
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

        /// <summary>
        /// Releases all resources used by the <see cref="I2CBus"/>.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Exchanges data with a peripheral device.
        /// </summary>
        /// <param name="peripheralAddress">The address of the peripheral device.</param>
        /// <param name="writeBuffer">The data buffer to write.</param>
        /// <param name="readBuffer">The data buffer to read.</param>
        public void Exchange(byte peripheralAddress, Span<byte> writeBuffer, Span<byte> readBuffer)
        {
            Write(peripheralAddress, writeBuffer, false);
            Read(peripheralAddress, readBuffer);
        }

        /// <summary>
        /// Reads data from a peripheral device.
        /// </summary>
        /// <param name="peripheralAddress">The address of the peripheral device.</param>
        /// <param name="readBuffer">The data buffer to read.</param>
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