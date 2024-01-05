using Meadow.Hardware;
using System;

namespace Meadow.Foundation
{
    /// <summary>
    /// Helper class for I2C communications, handles registers, endian, etc.
    /// </summary>
    public class I2cCommunications : II2cCommunications
    {
        /// <summary>
        /// The I2C address
        /// </summary>
        public byte Address { get; protected set; }

        /// <summary>
        /// The I2C bus
        /// </summary>
        public II2cBus Bus { get; protected set; }

        /// <summary>
        /// Internal write buffer - used in methods in which the buffers aren't
        /// passed in.
        /// </summary>
        protected Memory<byte> WriteBuffer { get; }
        /// <summary>
        /// Internal read buffer - used in methods in which the buffers aren't
        /// passed in.
        /// </summary>
        protected Memory<byte> ReadBuffer { get; }

        /// <summary>
        /// Initializes a new instance of the I2cCommunications class
        /// </summary>
        /// <param name="bus">The II2cBus used for communication with the peripheral</param>
        /// <param name="peripheralAddress">The address of the peripheral on the I2C bus</param>
        /// <param name="writeBufferSize">The size of the buffer used for writing data to the peripheral. Defaults to 8 bytes</param>
        public I2cCommunications(II2cBus bus, byte peripheralAddress, int writeBufferSize = 8)
        {
            Bus = bus;
            Address = peripheralAddress;
            WriteBuffer = new byte[writeBufferSize];
            ReadBuffer = new byte[3];
        }

        /// <summary>
        /// Reads data from the peripheral.
        /// </summary>
        /// <param name="readBuffer">The buffer to read from the peripheral into.</param>
        /// <remarks>
        /// The number of bytes to be read is determined by the length of the
        /// `readBuffer`.
        /// </remarks>
        public void Read(Span<byte> readBuffer)
        {
            Bus.Read(Address, readBuffer);
        }

        /// <summary>
        /// Reads data from the peripheral starting at the specified address.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="readBuffer"></param>
        public virtual void ReadRegister(byte address, Span<byte> readBuffer)
        {
            WriteBuffer.Span[0] = address;
            Bus.Exchange(Address, WriteBuffer.Span[0..1], readBuffer);
        }

        /// <summary>
        /// Read a register from the peripheral.
        /// </summary>
        /// <param name="address">Address of the register to read.</param>
        public virtual byte ReadRegister(byte address)
        {
            WriteBuffer.Span[0] = address;
            Bus.Exchange(Address, WriteBuffer.Span[0..1], ReadBuffer.Span[0..1]);
            return ReadBuffer.Span[0];
        }

        /// <summary>
        /// Read an unsigned short from a register.
        /// </summary>
        /// <param name="address">Register address of the low byte (the high byte will follow).</param>
        /// <param name="order">Order of the bytes in the register (little endian is the default).</param>
        /// <returns>Value read from the register.</returns>
        public virtual ushort ReadRegisterAsUShort(byte address, ByteOrder order = ByteOrder.LittleEndian)
        {
            WriteBuffer.Span[0] = address;
            Bus.Exchange(Address, WriteBuffer[0..1].Span, ReadBuffer[0..2].Span);
            if (order == ByteOrder.LittleEndian)
            {
                return (ushort)(ReadBuffer.Span[0] | (ReadBuffer.Span[1] << 8));
            }
            else
            {
                return (ushort)(ReadBuffer.Span[0] << 8 | ReadBuffer.Span[1]);
            }
        }

        /// <summary>
        /// Write a single byte to the peripheral.
        /// </summary>
        /// <param name="value">Value to be written (8-bits).</param>
        public virtual void Write(byte value)
        {
            WriteBuffer.Span[0] = value;
            Bus.Write(Address, WriteBuffer.Span[0..1]);
        }

        /// <summary>
        /// Write an array of bytes to the peripheral.
        /// </summary>
        /// <param name="data">Values to be written.</param>
        public virtual void Write(Span<byte> data) => Bus.Write(Address, data);

        /// <summary>
        /// Write data a register in the peripheral.
        /// </summary>
        /// <param name="address">Address of the register to write to.</param>
        /// <param name="value">Data to write into the register.</param>
        public virtual void WriteRegister(byte address, byte value)
        {
            WriteBuffer.Span[0] = address;
            WriteBuffer.Span[1] = value;
            Bus.Write(Address, WriteBuffer.Span[0..2]);
        }

        /// <summary>
        /// Write an unsigned short to the peripheral.
        /// </summary>
        /// <param name="address">Address to write the first byte to.</param>
        /// <param name="value">Value to be written (16-bits).</param>
        /// <param name="order">Indicate if the data should be written as big or little endian.</param>
        public virtual void WriteRegister(byte address, ushort value, ByteOrder order = ByteOrder.LittleEndian)
        {
            var bytes = BitConverter.GetBytes(value);
            WriteRegister(address, bytes, order);
        }

        /// <summary>
        /// Write an unsigned integer to the peripheral.
        /// </summary>
        /// <param name="address">Address to write the first byte to.</param>
        /// <param name="value">Value to be written.</param>
        /// <param name="order">Indicate if the data should be written as big or little endian.</param>
        public virtual void WriteRegister(byte address, uint value, ByteOrder order = ByteOrder.LittleEndian)
        {
            var bytes = BitConverter.GetBytes(value);
            WriteRegister(address, bytes, order);
        }

        /// <summary>
        /// Write an unsigned long to the peripheral.
        /// </summary>
        /// <param name="address">Address to write the first byte to.</param>
        /// <param name="value">Value to be written.</param>
        /// <param name="order">Indicate if the data should be written as big or little endian.</param>
        public virtual void WriteRegister(byte address, ulong value, ByteOrder order = ByteOrder.LittleEndian)
        {
            var bytes = BitConverter.GetBytes(value);
            WriteRegister(address, bytes, order);
        }

        /// <summary>
        /// Write data to a register in the peripheral.
        /// </summary>
        /// <param name="address">Address of the register to write to.</param>
        /// <param name="writeBuffer">A buffer of byte values to be written.</param>
        /// <param name="order">Indicate if the data should be written as big or little endian.</param>
        public virtual void WriteRegister(byte address, Span<byte> writeBuffer, ByteOrder order = ByteOrder.LittleEndian)
        {
            if (WriteBuffer.Length < writeBuffer.Length + 1)
            {
                throw new ArgumentException("Data to write is too large for the write buffer. " +
                    "Must be less than WriteBuffer.Length + 1 (to allow for address). " +
                    "Instantiate this class with a larger WriteBuffer, or send a smaller " +
                    "amount of data to fix.");
            }

            // add the register address to the start of the write buffer
            WriteBuffer.Span[0] = address;

            switch (order)
            {
                case ByteOrder.LittleEndian:
                    for (int i = 0; i < writeBuffer.Length; i++)
                    {
                        WriteBuffer.Span[i + 1] = writeBuffer[i];
                    }
                    break;
                case ByteOrder.BigEndian:
                    for (int i = 0; i < writeBuffer.Length; i++)
                    {
                        WriteBuffer.Span[i + 1] = writeBuffer[writeBuffer.Length - (i + 1)];
                    }
                    break;
            }
            Bus.Write(Address, WriteBuffer.Span[0..(writeBuffer.Length + 1)]);
        }

        /// <summary>
        /// Exchanges data with an I2C device through a specified write and read buffer
        /// </summary>
        /// <param name="writeBuffer">A span of bytes that represents the data to be written to the device</param>
        /// <param name="readBuffer">A span of bytes where the data read from the device will be stored</param>
        /// <param name="duplex">An optional parameter that specifies the duplex type of the communication.
        /// It defaults to half-duplex.</param>
        /// <exception cref="ArgumentException">Thrown when duplex is set to full-duplex, as I2C only supports half-duplex communication</exception>
        public virtual void Exchange(Span<byte> writeBuffer, Span<byte> readBuffer, DuplexType duplex = DuplexType.Half)
        {
            if (duplex == DuplexType.Full) { throw new ArgumentException("I2C doesn't support full-duplex communications. Only half-duplex is available because it only has a single data line."); }

            Bus.Exchange(Address, writeBuffer, readBuffer);
        }
    }
}