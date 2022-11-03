using Meadow.Hardware;
using Meadow.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// A TCA9548A i2c multiplexer
    /// </summary>
    public partial class Tca9548a : II2cPeripheral
    {
        internal SemaphoreSlim BusSelectorSemaphore = new SemaphoreSlim(1, 1);

        private readonly IReadOnlyDictionary<byte, II2cBus> buses;
        private byte selectedBus = 0xff;

        /// <summary>
        /// The address of this device on the <see cref="Bus"/>.
        /// </summary>
        public byte Address { get; }

        /// <summary>
        /// Create a <see cref="Tca9548a"/> i2c multiplexer
        /// </summary>
        /// <param name="bus">The <see cref="II2cBus"/> the device is attached to</param>
        /// <param name="address">The address of the device on the specified <paramref name="bus"/></param>
        /// <exception cref="ArgumentOutOfRangeException">The device address was invalid</exception>
        /// <exception cref="ArgumentNullException">The bus was null</exception>
        public Tca9548a(II2cBus bus, byte address = (byte)Addresses.Default)
        {
            Address = TcaAddressTable.IsValidAddress(address)
                          ? address
                          : throw new ArgumentOutOfRangeException(
                                nameof(address),
                                $"0x{address:X}",
                                $"Invalid address. Valid address are 0x{TcaAddressTable.MinAddress:X} through 0x{TcaAddressTable.MaxAddress:X}");

            Bus = bus ?? throw new ArgumentNullException(nameof(bus), "The bus cannot be null.");

            buses = Enumerable.Range(0, 8)
                               .Select(Convert.ToByte)
                               .ToDictionary(
                                   b => b,
                                   b => new Tca9548aBus(this, (int)bus.Frequency.Hertz, b) as II2cBus);
        }

        /// <summary>
        /// Create a <see cref="Tca9548a"/> i2c multiplexer.
        /// </summary>
        /// <param name="i2cBus">The I2cBus the device is attached to</param>
        /// <param name="a0">The logic high/low state of pin A0</param>
        /// <param name="a1">The logic high/low state of pin A1</param>
        /// <param name="a2">The logic high/low state of pin A2</param>
        public Tca9548a(II2cBus i2cBus, bool a0, bool a1, bool a2)
            : this(i2cBus, TcaAddressTable.GetAddressFromPins(a0, a1, a2))
        { }

        /// <summary>
        /// The <see cref="II2cBus"/> this device is connected to.
        /// </summary>
        public II2cBus Bus { get; }

        /// <summary>
        /// The <see cref="II2cBus"/> connected to SD0/SC0
        /// </summary>
        public II2cBus Bus0 => buses[0];

        /// <summary>
        /// The <see cref="II2cBus"/> connected to SD1/SC1
        /// </summary>
        public II2cBus Bus1 => buses[1];

        /// <summary>
        /// The <see cref="II2cBus"/> connected to SD2/SC2
        /// </summary>
        public II2cBus Bus2 => buses[2];

        /// <summary>
        /// The <see cref="II2cBus"/> connected to SD3/SC3
        /// </summary>
        public II2cBus Bus3 => buses[3];

        /// <summary>
        /// The <see cref="II2cBus"/> connected to SD4/SC4
        /// </summary>
        public II2cBus Bus4 => buses[4];

        /// <summary>
        /// The <see cref="II2cBus"/> connected to SD5/SC5
        /// </summary>
        public II2cBus Bus5 => buses[5];

        /// <summary>
        /// The <see cref="II2cBus"/> connected to SD6/SC6
        /// </summary>
        public II2cBus Bus6 => buses[6];

        /// <summary>
        /// The <see cref="II2cBus"/> connected to SD7/SC7
        /// </summary>
        public II2cBus Bus7 => buses[7];

        /// <summary>
        /// Activate the specified bus
        /// </summary>
        /// <param name="busIndex"></param>
        internal void SelectBus(byte busIndex)
        {
            if (this.selectedBus == busIndex)
            {
                return;
            }

            var @byte = BitHelpers.SetBit(0x00, busIndex, true);
            Write(@byte);
            var selectedBus = ReadBytes(1)[0];
            if (selectedBus.CompareTo(selectedBus) != 0)
                throw new Exception(
                    $"Failed to switch to the desired bus. Expected {@byte:X8} got {selectedBus:X8}");

            this.selectedBus = busIndex;
        }

        /// <summary>
        /// Write an array of bytes to the peripheral
        /// </summary>
        /// <param name="data">Values to be written</param>
        public void Write(Span<byte> data)
        {
            Bus.Write(Address, data);
        }

        /// <summary>
        /// Write an unsigned integer to the peripheral
        /// </summary>
        /// <param name="address">Address to write the first byte to</param>
        /// <param name="value">Value to be written</param>
        /// <param name="order">Indicate if the data should be written as big or little endian</param>
        public void WriteRegister(byte address, uint value, ByteOrder order = ByteOrder.LittleEndian)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Write an unsigned long to the peripheral.
        /// </summary>
        /// <param name="address">Address to write the first byte to</param>
        /// <param name="value">Value to be written</param>
        /// <param name="order">Indicate if the data should be written as big or little endian</param>
        public void WriteRegister(byte address, ulong value, ByteOrder order = ByteOrder.LittleEndian)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Write a single byte to the peripheral.
        /// </summary>
        /// <param name="value">Value to be written (8-bits)</param>
        public void Write(byte value)
        {
            Bus.Write(Address, new byte[] { value });
        }

        /// <summary>
        /// Write an unsigned short to the peripheral.
        /// </summary>
        /// <param name="address">Address to write the first byte to</param>
        /// <param name="value">Value to be written (16-bits)</param>
        /// <param name="order">Indicate if the data should be written as big or little endian</param>
        public void WriteRegister(byte address,
                                ushort value,
                                ByteOrder order = ByteOrder.LittleEndian)
        {
            throw new NotSupportedException("This method is not supported for this device");
        }

        /// <summary>
        /// Write data tp a register in the peripheral
        /// </summary>
        /// <param name="address">Address of the register to write to</param>
        /// <param name="value">Data to write into the register</param>
        public void WriteRegister(byte address, byte value)
        {
            throw new NotSupportedException("This method is not supported for this device");
        }

        /// <summary>
        /// Read bytes from the I2cBus
        /// </summary>
        /// <param name="numberOfBytes"></param>
        public byte[] ReadBytes(ushort numberOfBytes)
        {
            var data = new byte[numberOfBytes];
            Bus.Read(Address, data);
            return data;
        }

        /// <summary>
        /// Read a register from the peripheral
        /// </summary>
        /// <param name="address">Address of the register to read.</param>
        public byte ReadRegister(byte address)
        {
            throw new NotSupportedException("This method is not supported for this device");
        }

        /// <summary>
        /// Read an usingned short from a register
        /// </summary>
        /// <param name="address">Register address of the low byte (the high byte will follow).</param>
        /// <param name="order">Order of the bytes in the register (little endian is the default).</param>
        public ushort ReadRegisterAsUShort(byte address, ByteOrder order)
        {
            throw new NotSupportedException("This method is not supported for this device");
        }

        /// <summary>
        /// Reads data from the peripheral
        /// </summary>
        /// <param name="readBuffer"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void Read(Span<byte> readBuffer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads data from the peripheral starting at the specified address
        /// </summary>
        /// <param name="address"></param>
        /// <param name="readBuffer"></param>
        public void ReadRegister(byte address, Span<byte> readBuffer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Write data to a register in the peripheral
        /// </summary>
        /// <param name="address">Address of the register to write to</param>
        /// <param name="writeBuffer">A buffer of byte values to be written</param>
        /// <param name="order">Indicate if the data should be written as big or little endian</param>
        public void WriteRegister(byte address, Span<byte> writeBuffer, ByteOrder order = ByteOrder.LittleEndian)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Write data to followed by read data from the peripheral.
        /// </summary>
        /// <param name="writeBuffer">Data to write</param>
        /// <param name="readBuffer">Buffer where read data will be written. Number of bytes read is determined by buffer size.</param>
        /// <param name="duplex">Whether the communication will happen in a half-duplex or full-duplex fashion.</param>
        public void Exchange(Span<byte> writeBuffer, Span<byte> readBuffer, DuplexType duplex)
        {
            throw new NotImplementedException();
        }
    }
}