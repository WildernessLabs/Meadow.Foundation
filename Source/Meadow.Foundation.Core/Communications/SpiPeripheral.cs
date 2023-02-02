using System;

namespace Meadow.Hardware
{
    /// <summary>
    /// Represents an SPI peripheral object
    /// This encapsulates and synchronizes the SPI bus and chip select ports
    /// </summary>
    public class SpiPeripheral : ISpiPeripheral
    {
        /// <summary>
        /// The SPI chip select port
        /// </summary>
        public IDigitalOutputPort? ChipSelect { get; }

        /// <summary>
        /// The chip select mode (active high or active low)
        /// </summary>
        ChipSelectMode chipSelectMode;

        /// <summary>
        /// the ISpiBus object
        /// </summary>
        public ISpiBus Bus { get; }

        /// <summary>
        /// Internal write buffer. Used in methods in which the buffers aren't
        /// passed in.
        /// </summary>
        protected Memory<byte> WriteBuffer { get; }
        /// <summary>
        /// Internal read buffer. Used in methods in which the buffers aren't
        /// passed in.
        /// </summary>
        protected Memory<byte> ReadBuffer { get; }

        /// <summary>
        /// Creates a new SpiPeripheral instance
        /// </summary>
        /// <param name="bus">The spi bus connected to the peripheral</param>
        /// <param name="chipSelect">The chip select port</param>
        /// <param name="readBufferSize">The size of the read buffer in bytes</param>
        /// <param name="writeBufferSize">The size of the write buffer in bytes</param>
        /// <param name="csMode">The chip select mode, active high or active low</param>
        public SpiPeripheral(
            ISpiBus bus,
            IDigitalOutputPort? chipSelect,
            int readBufferSize = 8, int writeBufferSize = 8,
            ChipSelectMode csMode = ChipSelectMode.ActiveLow)
        {
            this.Bus = bus;
            this.ChipSelect = chipSelect;
            this.chipSelectMode = csMode;
            WriteBuffer = new byte[writeBufferSize];
            ReadBuffer = new byte[readBufferSize];
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
            Bus.Read(this.ChipSelect, readBuffer, this.chipSelectMode);
        }

        /// <summary>
        /// Reads data from the peripheral starting at the specified address.
        /// </summary>
        /// <param name="address">The register address</param>
        /// <param name="readBuffer">The buffer to hold the data</param>
        public void ReadRegister(byte address, Span<byte> readBuffer)
        {
            WriteBuffer.Span[0] = address;
            Bus.Exchange(this.ChipSelect, WriteBuffer.Span[0..readBuffer.Length], readBuffer, this.chipSelectMode);
        }

        /// <summary>
        /// Reads a single byte from the specified address of the peripheral
        /// </summary>
        /// <param name="address">Address to read</param>
        /// <returns>The byte read</returns>
        public byte ReadRegister(byte address)
        {
            WriteBuffer.Span[0] = address;
            Bus.Exchange(this.ChipSelect, WriteBuffer.Span[0..1], ReadBuffer.Span[0..1], this.chipSelectMode);
            return ReadBuffer.Span[0];
        }

        /// <summary>
        /// Reads a single ushort value from the specified address of the peripheral
        /// </summary>
        /// <param name="address">Address of the read</param>
        /// <param name="order">Endianness of the value read</param>
        /// <returns>The value read</returns>
        public ushort ReadRegisterAsUShort(byte address, ByteOrder order = ByteOrder.LittleEndian)
        {
            ReadRegister(address, ReadBuffer[0..2].Span);
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
        /// Writes a single byte to the peripheral
        /// </summary>
        /// <param name="value">Value to write</param>
        public void Write(byte value)
        {
            WriteBuffer.Span[0] = value;
            Bus.Write(ChipSelect, WriteBuffer.Span[0..1], this.chipSelectMode);
        }

        /// <summary>
        /// Write a span of bytes to the peripheral.
        /// </summary>
        /// <param name="data">Data to be written.</param>
        public void Write(Span<byte> data)
        {
            Bus.Write(this.ChipSelect, data, this.chipSelectMode);
        }

        /// <summary>
        /// Writes a single byte to the specified address of the peripheral
        /// </summary>
        /// <param name="address">The target write register address</param>
        /// <param name="value">Value to write</param>
        public void WriteRegister(byte address, byte value)
        {
            // stuff the address and value into the write buffer
            WriteBuffer.Span[0] = address;
            WriteBuffer.Span[1] = value;
            Bus.Write(ChipSelect, WriteBuffer.Span[0..2], this.chipSelectMode);
        }

        /// <summary>
        /// Writes a single ushort value to a target register address on the peripheral (i.e. [address][ushort])
        /// </summary>
        /// <param name="address">The target write register address</param>
        /// <param name="value">Value to write</param>
        /// <param name="order">Endianness of the value to be written</param>
        public void WriteRegister(byte address, ushort value, ByteOrder order = ByteOrder.LittleEndian)
        {
            // split the 16 bit ushort into two bytes
            var bytes = BitConverter.GetBytes(value);
            // call the helper method
            WriteRegister(address, bytes, order);
        }

        /// <summary>
        /// Write an unsigned integer to the peripheral.
        /// </summary>
        /// <param name="address">Address to write the first byte to.</param>
        /// <param name="value">Value to be written.</param>
        /// <param name="order">Indicate if the data should be written as big or little endian.</param>
        public void WriteRegister(byte address, uint value, ByteOrder order = ByteOrder.LittleEndian)
        {
            // split the 32 bit ushort into four bytes
            var bytes = BitConverter.GetBytes(value);
            // call the helper method
            WriteRegister(address, bytes, order);
        }

        /// <summary>
        /// Write an unsigned long to the peripheral.
        /// </summary>
        /// <param name="address">Address to write the first byte to.</param>
        /// <param name="value">Value to be written.</param>
        /// <param name="order">Indicate if the data should be written as big or little endian.</param>
        public void WriteRegister(byte address, ulong value, ByteOrder order = ByteOrder.LittleEndian)
        {
            // split the 64 bit ushort into eight bytes
            var bytes = BitConverter.GetBytes(value);
            // call the helper method
            WriteRegister(address, bytes, order);
        }

        /// <summary>
        /// Write data to a register in the peripheral.
        /// </summary>
        /// <param name="address">Address of the register to write to.</param>
        /// <param name="writeBuffer">A buffer of byte values to be written.</param>
        /// <param name="order">Indicate if the data should be written as big or little endian.</param>
        public void WriteRegister(byte address, Span<byte> writeBuffer, ByteOrder order = ByteOrder.LittleEndian)
        {
            if (WriteBuffer.Length < writeBuffer.Length + 1)
            {
                throw new ArgumentException("Data to write is too large for the write buffer. " +
                    "Must be less than WriteBuffer.Length + 1 (to allow for address). " +
                    "Instantiate this class with a larger WriteBuffer, or send a smaller" +
                    "amount of data to fix.");
            }

            // stuff the register address into the write buffer
            WriteBuffer.Span[0] = address;

            // stuff the bytes into the write buffer (starting at `1` index,
            // because `0` is the register address.
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
                        // stuff them backwards
                        WriteBuffer.Span[i + 1] = writeBuffer[writeBuffer.Length - (i + 1)];
                    }
                    break;
            }
            // write it
            this.Bus.Write(this.ChipSelect, WriteBuffer.Span[0..(writeBuffer.Length + 1)], this.chipSelectMode);
        }

        /// <summary>
        /// Exchange data over the SPI bus
        /// </summary>
        /// <param name="writeBuffer">The buffer holding the data to write</param>
        /// <param name="readBuffer">The buffer to receieve data</param>
        /// <param name="duplex">The duplex mode - half or full</param>
        public void Exchange(Span<byte> writeBuffer, Span<byte> readBuffer, DuplexType duplex = DuplexType.Half)
        {
            if (duplex == DuplexType.Half)
            {
                // Todo: we should move this functionality deeper into the stack
                // and have nuttx write the write buffer, then continue clocking out
                // 0x00's until it's hit writeBuffer.Length + readBuffer.Lenght
                // and ignore the input until it hits writeBuffer.Length, and then
                // start writing directly into the readBuffer starting at 0.
                // that will prevent all the allocations and copying we're doing
                // here.

                // clock in and clock out data means that the buffers have to be as
                // long as both tx and rx together
                int length = writeBuffer.Length + readBuffer.Length;
                Span<byte> txBuffer = stackalloc byte[length];
                Span<byte> rxBuffer = stackalloc byte[length];

                // copy the write into tx
                writeBuffer.CopyTo(txBuffer);

                // write/read the data
                Bus.Exchange(ChipSelect, txBuffer, rxBuffer, this.chipSelectMode);

                // move the rx data into the read buffer, starting it at zero
                rxBuffer[writeBuffer.Length..length].CopyTo(readBuffer);
            }
            else
            {
                Bus.Exchange(ChipSelect, writeBuffer, readBuffer, this.chipSelectMode);
            }
        }
    }
}