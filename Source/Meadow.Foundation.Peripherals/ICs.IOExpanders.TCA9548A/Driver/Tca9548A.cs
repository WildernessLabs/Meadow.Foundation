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
        private readonly IReadOnlyDictionary<byte, II2cBus> buses;
        private byte selectedBus = 0xff;
        internal SemaphoreSlim BusSelectorSemaphore = new SemaphoreSlim(1, 1);

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
        /// <param name="i2cBus">The <see cref="I2cBus"/> the device is attached to</param>
        /// <param name="a0">The logic high/low state of pin A0</param>
        /// <param name="a1">The logic high/low state of pin A1</param>
        /// <param name="a2">The logic high/low state of pin A2</param>
        /// <exception cref="ArgumentOutOfRangeException">The device address was invalid</exception>
        /// <exception cref="ArgumentNullException">The bus was null</exception>
        public Tca9548a(II2cBus i2cBus, bool a0, bool a1, bool a2)
            : this(i2cBus, TcaAddressTable.GetAddressFromPins(a0, a1, a2))
        {
        }

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


        public void Write(Span<byte> writeBuffer)
        {
            Bus.Write(Address, writeBuffer);
        }

        public void WriteRegister(byte address, uint value, ByteOrder order = ByteOrder.LittleEndian)
        {
            throw new NotImplementedException();
        }

        public void WriteRegister(byte address, ulong value, ByteOrder order = ByteOrder.LittleEndian)
        {
            throw new NotImplementedException();
        }


        ///<inheritdoc cref="Write"/>
        public void Write(byte value)
        {
            Bus.WriteData(Address, value);
        }

        /// <inheritdoc cref="WriteBytes"/>
        /// <exception cref="NotSupportedException">This method is not supported for this device</exception>
        public void WriteBytes(byte[] values)
        {
            throw new NotSupportedException("This method is not supported for this device");
        }

        /// <inheritdoc cref="WriteRegister"/>
        /// <exception cref="NotSupportedException">This method is not supported for this device</exception>
        public void WriteRegister(byte address,
                                ushort value,
                                ByteOrder order = ByteOrder.LittleEndian)
        {
            throw new NotSupportedException("This method is not supported for this device");
        }

        /// <inheritdoc cref="WriteUShorts"/>
        /// <exception cref="NotSupportedException">This method is not supported for this device</exception>
        public void WriteUShorts(byte address,
                                 ushort[] values,
                                 ByteOrder order = ByteOrder.LittleEndian)
        {
            throw new NotSupportedException("This method is not supported for this device");
        }

        /// <inheritdoc cref="WriteRegister"/>
        /// <exception cref="NotSupportedException">This method is not supported for this device</exception>
        public void WriteRegister(byte address, byte value)
        {
            throw new NotSupportedException("This method is not supported for this device");
        }

        /// <inheritdoc cref="WriteRegisters"/>
        /// <exception cref="NotSupportedException">This method is not supported for this device</exception>
        public void WriteRegisters(byte address, byte[] data)
        {
            throw new NotSupportedException("This method is not supported for this device");
        }

        /// <inheritdoc cref="WriteRead(byte[],ushort)"/>
        /// <exception cref="NotSupportedException">This method is not supported for this device</exception>
        public byte[] WriteRead(byte[] write, ushort length)
        {
            throw new NotSupportedException("This method is not supported for this device");
        }

        /// <inheritdoc cref="IByteCommunications.Exchange(System.Span{byte},System.Span{byte})"/>
        /// <exception cref="NotSupportedException">This method is not supported for this device</exception>
        public void Exchange(Span<byte> writeBuffer, Span<byte> readBuffer)
        {
            throw new NotSupportedException("This method is not supported for this device");
        }

        /// <inheritdoc cref="ReadBytes"/>
        public byte[] ReadBytes(ushort numberOfBytes)
        {
            return Bus.ReadData(Address, numberOfBytes);
        }

        /// <inheritdoc cref="ReadRegister"/>
        /// <exception cref="NotSupportedException">This method is not supported for this device</exception>
        public byte ReadRegister(byte address)
        {
            throw new NotSupportedException("This method is not supported for this device");
        }

        /// <inheritdoc cref="ReadRegisters"/>
        /// <exception cref="NotSupportedException">This method is not supported for this device</exception>
        public byte[] ReadRegisters(byte address, ushort length)
        {
            throw new NotSupportedException("This method is not supported for this device");
        }

        /// <inheritdoc cref="ReadRegisterAsUShort"/>
        /// <exception cref="NotSupportedException">This method is not supported for this device</exception>
        public ushort ReadRegisterAsUShort(byte address, ByteOrder order)
        {
            throw new NotSupportedException("This method is not supported for this device");
        }

        /// <inheritdoc cref="ReadUShorts"/>
        /// <exception cref="NotSupportedException">This method is not supported for this device</exception>
        public ushort[] ReadUShorts(byte address,
                                    ushort number,
                                    ByteOrder order = ByteOrder.LittleEndian)
        {
            throw new NotSupportedException("This method is not supported for this device");
        }

        /// <exception cref="NotSupportedException">This method is not supported for this device</exception>
        public void Read(Span<byte> readBuffer)
        {
            throw new NotImplementedException();
        }

        /// <exception cref="NotSupportedException">This method is not supported for this device</exception>
        public void ReadRegister(byte address, Span<byte> buffer)
        {
            throw new NotImplementedException();
        }

        public void WriteRegister(byte address, Span<byte> writeBuffer, ByteOrder order = ByteOrder.LittleEndian)
        {
            throw new NotImplementedException();
        }

        public void Exchange(Span<byte> writeBuffer, Span<byte> readBuffer, DuplexType duplex)
        {
            throw new NotImplementedException();
        }
    }
}