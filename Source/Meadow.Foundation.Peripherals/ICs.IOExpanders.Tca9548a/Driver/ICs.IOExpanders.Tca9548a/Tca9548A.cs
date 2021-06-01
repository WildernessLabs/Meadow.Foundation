﻿using Meadow.Hardware;
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
    public class Tca9548a : II2cPeripheral
    {
        private readonly IReadOnlyDictionary<byte, II2cBus> _buses;
        private byte _selectedBus = 0xff;
        internal SemaphoreSlim BusSelectorSemaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Create a <see cref="Tca9548a"/> i2c multiplexer
        /// </summary>
        /// <param name="bus">The <see cref="II2cBus"/> the device is attached to</param>
        /// <param name="address">The address of the device on the specified <paramref name="bus"/></param>
        /// <exception cref="ArgumentOutOfRangeException">The device address was invalid</exception>
        /// <exception cref="ArgumentNullException">The bus was null</exception>
        public Tca9548a(II2cBus bus, byte address = 0x70)
        {
            Address = TcaAddressTable.IsValidAddress(address)
                          ? address
                          : throw new ArgumentOutOfRangeException(
                                nameof(address),
                                $"0x{address:X}",
                                $"Invalid address. Valid address are 0x{TcaAddressTable.MinAddress:X} through 0x{TcaAddressTable.MaxAddress:X}");

            Bus = bus ?? throw new ArgumentNullException(nameof(bus), "The bus cannot be null.");

            _buses = Enumerable.Range(0, 8)
                               .Select(Convert.ToByte)
                               .ToDictionary(
                                   b => b,
                                   b => new Tca9548aBus(this, (int)bus.Frequency.Hertz, b) as II2cBus);
        }

        /// <summary>
        /// Create a <see cref="Tca9548a"/> i2c multiplexer.
        /// </summary>
        /// <param name="bus">The <see cref="I2cBus"/> the device is attached to</param>
        /// <param name="a0">The logic high/low state of pin A0</param>
        /// <param name="a1">The logic high/low state of pin A1</param>
        /// <param name="a2">The logic high/low state of pin A2</param>
        /// <exception cref="ArgumentOutOfRangeException">The device address was invalid</exception>
        /// <exception cref="ArgumentNullException">The bus was null</exception>
        public Tca9548a(II2cBus bus, bool a0, bool a1, bool a2)
            : this(bus, TcaAddressTable.GetAddressFromPins(a0, a1, a2))
        {
        }

        /// <summary>
        /// The address of this device on the <see cref="Bus"/>.
        /// </summary>
        public byte Address { get; }

        /// <summary>
        /// The <see cref="II2cBus"/> this device is connected to.
        /// </summary>
        public II2cBus Bus { get; }

        /// <summary>
        /// The <see cref="II2cBus"/> connected to SD0/SC0
        /// </summary>
        public II2cBus Bus0 => _buses[0];

        /// <summary>
        /// The <see cref="II2cBus"/> connected to SD1/SC1
        /// </summary>
        public II2cBus Bus1 => _buses[1];

        /// <summary>
        /// The <see cref="II2cBus"/> connected to SD2/SC2
        /// </summary>
        public II2cBus Bus2 => _buses[2];

        /// <summary>
        /// The <see cref="II2cBus"/> connected to SD3/SC3
        /// </summary>
        public II2cBus Bus3 => _buses[3];

        /// <summary>
        /// The <see cref="II2cBus"/> connected to SD4/SC4
        /// </summary>
        public II2cBus Bus4 => _buses[4];

        /// <summary>
        /// The <see cref="II2cBus"/> connected to SD5/SC5
        /// </summary>
        public II2cBus Bus5 => _buses[5];

        /// <summary>
        /// The <see cref="II2cBus"/> connected to SD6/SC6
        /// </summary>
        public II2cBus Bus6 => _buses[6];

        /// <summary>
        /// The <see cref="II2cBus"/> connected to SD7/SC7
        /// </summary>
        public II2cBus Bus7 => _buses[7];

        /// <summary>
        /// Activate the specified bus
        /// </summary>
        /// <param name="busIndex"></param>
        internal void SelectBus(byte busIndex)
        {
            if (_selectedBus == busIndex)
            {
                return;
            }

            var @byte = BitHelpers.SetBit(0x00, busIndex, true);
            WriteByte(@byte);
            var selectedBus = ReadBytes(1)[0];
            if (selectedBus.CompareTo(selectedBus) != 0)
                throw new Exception(
                    $"Failed to switch to the desired bus. Expected {@byte:X8} got {selectedBus:X8}");

            _selectedBus = busIndex;
        }

        ///<inheritdoc cref="WriteByte"/>
        public void WriteByte(byte value)
        {
            Bus.WriteData(Address, value);
        }

        /// <inheritdoc cref="WriteBytes"/>
        /// <exception cref="NotSupportedException">This method is not supported for this device</exception>
        public void WriteBytes(byte[] values)
        {
            throw new NotSupportedException("This method is not supported for this device");
        }

        /// <inheritdoc cref="WriteUShort"/>
        /// <exception cref="NotSupportedException">This method is not supported for this device</exception>
        public void WriteUShort(byte address,
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

        /// <inheritdoc cref="IByteCommunications.WriteRead(System.Span{byte},System.Span{byte})"/>
        /// <exception cref="NotSupportedException">This method is not supported for this device</exception>
        public void WriteRead(Span<byte> writeBuffer, Span<byte> readBuffer)
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

        /// <inheritdoc cref="ReadUShort"/>
        /// <exception cref="NotSupportedException">This method is not supported for this device</exception>
        public ushort ReadUShort(byte address, ByteOrder order)
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
    }
}