using Meadow.Units;
using System;

namespace Meadow.Hardware
{
    /// <summary>
    /// Helper class for SPI communications, handles registers, endian, etc.
    /// This encapsulates and synchronizes the SPI bus and chip select ports
    /// </summary>
    public class SpiCommunications : ISpiCommunications
    {
        /// <summary>
        /// The SPI chip select port
        /// </summary>
        public IDigitalOutputPort? ChipSelect { get; }

        /// <summary>
        /// The chip select mode (active high or active low)
        /// </summary>
        private readonly ChipSelectMode chipSelectMode;

        /// <summary>
        /// the ISpiBus object
        /// </summary>
        public ISpiBus Bus { get; }

        /// <summary>
        /// SPI bus speed
        /// </summary>
        public Frequency BusSpeed { get; set; }

        /// <summary>
        /// SPI bus mode
        /// </summary>
        public SpiClockConfiguration.Mode BusMode { get; set; }

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
        /// Creates a new SpiCommunications instance
        /// </summary>
        /// <param name="bus">The spi bus connected to the peripheral</param>
        /// <param name="chipSelect">The chip select port</param>
        /// <param name="busSpeed">The SPI bus speed</param>
        /// <param name="busMode">The SPI bus mode (0-3)</param>
        /// <param name="readBufferSize">The size of the read buffer in bytes</param>
        /// <param name="writeBufferSize">The size of the write buffer in bytes</param>
        /// <param name="csMode">The chip select mode, active high or active low</param>
        public SpiCommunications(
            ISpiBus bus,
            IDigitalOutputPort? chipSelect,
            Frequency busSpeed,
            SpiClockConfiguration.Mode busMode = SpiClockConfiguration.Mode.Mode0,
            int readBufferSize = 8, int writeBufferSize = 8,
            ChipSelectMode csMode = ChipSelectMode.ActiveLow)
        {
            Bus = bus;
            BusMode = busMode;
            BusSpeed = busSpeed;
            ChipSelect = chipSelect;
            chipSelectMode = csMode;
            WriteBuffer = new byte[writeBufferSize];
            ReadBuffer = new byte[readBufferSize];

            // de-assert the chip select
            if (chipSelect != null)
            {
                chipSelect.State = (chipSelectMode == ChipSelectMode.ActiveLow) ? true : false;
            }
        }

        /// <summary>
        /// Reads data from the peripheral.
        /// </summary>
        /// <param name="readBuffer">The buffer to read from the peripheral into.</param>
        /// <remarks>
        /// The number of bytes to be read is determined by the length of the
        /// `readBuffer`.
        /// </remarks>
        public virtual void Read(Span<byte> readBuffer)
        {
            AutoSetBusSpeedAndMode();

            Bus.Read(ChipSelect, readBuffer, chipSelectMode);
        }

        /// <summary>
        /// Reads data from the peripheral starting at the specified address.
        /// </summary>
        /// <param name="address">The register address</param>
        /// <param name="readBuffer">The buffer to hold the data</param>
        public virtual void ReadRegister(byte address, Span<byte> readBuffer)
        {
            AutoSetBusSpeedAndMode();

            WriteBuffer.Span[0] = address;
            Bus.Exchange(ChipSelect, WriteBuffer.Span[0..readBuffer.Length], readBuffer, chipSelectMode);
        }

        /// <summary>
        /// Reads a single byte from the specified address of the peripheral
        /// </summary>
        /// <param name="address">Address to read</param>
        /// <returns>The byte read</returns>
        public virtual byte ReadRegister(byte address)
        {
            AutoSetBusSpeedAndMode();

            WriteBuffer.Span[0] = address;
            Bus.Exchange(ChipSelect, WriteBuffer.Span[0..1], ReadBuffer.Span[0..1], chipSelectMode);
            return ReadBuffer.Span[0];
        }

        /// <summary>
        /// Reads a single ushort value from the specified address of the peripheral
        /// </summary>
        /// <param name="address">Address of the read</param>
        /// <param name="order">Endianness of the value read</param>
        /// <returns>The value read</returns>
        public virtual ushort ReadRegisterAsUShort(byte address, ByteOrder order = ByteOrder.LittleEndian)
        {
            ReadRegister(address, ReadBuffer[0..2].Span);
            if (order == ByteOrder.LittleEndian)
            {
                return (ushort)(ReadBuffer.Span[0] | (ReadBuffer.Span[1] << 8));
            }
            else
            {
                return (ushort)((ReadBuffer.Span[0] << 8) | ReadBuffer.Span[1]);
            }
        }

        /// <summary>
        /// Writes a single byte to the peripheral
        /// </summary>
        /// <param name="value">Value to write</param>
        public void Write(byte value)
        {
            AutoSetBusSpeedAndMode();

            WriteBuffer.Span[0] = value;
            Bus.Write(ChipSelect, WriteBuffer.Span[0..1], chipSelectMode);
        }

        /// <summary>
        /// Write a span of bytes to the peripheral.
        /// </summary>
        /// <param name="data">Data to be written.</param>
        public virtual void Write(Span<byte> data)
        {
            AutoSetBusSpeedAndMode();

            Bus.Write(ChipSelect, data, chipSelectMode);
        }

        /// <summary>
        /// Writes a single byte to the specified address of the peripheral
        /// </summary>
        /// <param name="address">The target write register address</param>
        /// <param name="value">Value to write</param>
        public virtual void WriteRegister(byte address, byte value)
        {
            AutoSetBusSpeedAndMode();

            WriteBuffer.Span[0] = address;
            WriteBuffer.Span[1] = value;
            Bus.Write(ChipSelect, WriteBuffer.Span[0..2], chipSelectMode);
        }

        /// <summary>
        /// Writes a single ushort value to a target register address on the peripheral (i.e. [address][ushort])
        /// </summary>
        /// <param name="address">The target write register address</param>
        /// <param name="value">Value to write</param>
        /// <param name="order">Endianness of the value to be written</param>
        public virtual void WriteRegister(byte address, ushort value, ByteOrder order = ByteOrder.LittleEndian)
        {
            // split the 16 bit ushort into two bytes
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
                    "Instantiate this class with a larger WriteBuffer, or send a smaller" +
                    "amount of data to fix.");
            }

            AutoSetBusSpeedAndMode();

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
            Bus.Write(ChipSelect, WriteBuffer.Span[0..(writeBuffer.Length + 1)], chipSelectMode);
        }

        private void AutoSetBusSpeedAndMode()
        {
            if (Bus.Configuration.SpiMode != BusMode)
            {
                Bus.Configuration.SetBusMode(BusMode);
            }

            if (Bus.Configuration.Speed != BusSpeed)
            {
                Bus.Configuration.Speed = BusSpeed;
            }
        }

        /// <summary>
        /// Exchange data over the SPI bus
        /// </summary>
        /// <param name="writeBuffer">The buffer holding the data to write</param>
        /// <param name="readBuffer">The buffer to receieve data</param>
        /// <param name="duplex">The duplex mode - half or full</param>
        public virtual void Exchange(Span<byte> writeBuffer, Span<byte> readBuffer, DuplexType duplex = DuplexType.Half)
        {
            AutoSetBusSpeedAndMode();

            if (duplex == DuplexType.Half)
            {
                int length = writeBuffer.Length + readBuffer.Length;
                Span<byte> txBuffer = stackalloc byte[length];
                Span<byte> rxBuffer = stackalloc byte[length];

                writeBuffer.CopyTo(txBuffer);

                Bus.Exchange(ChipSelect, txBuffer, rxBuffer, chipSelectMode);

                rxBuffer[writeBuffer.Length..length].CopyTo(readBuffer);
            }
            else
            {
                Bus.Exchange(ChipSelect, writeBuffer, readBuffer, chipSelectMode);
            }
        }
    }
}