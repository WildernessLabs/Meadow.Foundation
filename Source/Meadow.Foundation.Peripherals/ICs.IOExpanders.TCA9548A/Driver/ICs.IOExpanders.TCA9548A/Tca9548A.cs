using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Meadow;
using Meadow.Hardware;
using Meadow.Utilities;

namespace ICs.IOExpanders.TCA9548A
{
    public class Tca9548A : II2cPeripheral
    {
        private readonly Dictionary<byte, II2cBus> _buses;
        private byte _selectedBus = 0xff;
        internal SemaphoreSlim BusSelectorSemaphore = new SemaphoreSlim(1, 1);

        public Tca9548A(II2cBus bus, byte address)
        {
            Console.WriteLine($"Creating TCA9548A @ {Address}");
            Address = address;
            Bus = bus;
            _buses = Enumerable.Range(0, 8).Select(Convert.ToByte)
                               .ToDictionary(b => b, b => new Tca9548ABus(this, bus.Frequency, b) as II2cBus);
        }

        public Tca9548A(II2cBus bus, bool a0, bool a1, bool a2) : this(bus, TcaAddressTable.GetAddressFromPins(a0, a1, a2))
        {

        }

        public byte Address { get; }
        public II2cBus Bus { get; }

        public II2cBus Bus0 => _buses[0];
        public II2cBus Bus1 => _buses[1];
        public II2cBus Bus2 => _buses[2];
        public II2cBus Bus3 => _buses[3];
        public II2cBus Bus4 => _buses[4];
        public II2cBus Bus5 => _buses[5];
        public II2cBus Bus6 => _buses[6];
        public II2cBus Bus7 => _buses[7];

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
                throw new Exception($"Failed to switch to the desired bus. Expected {@byte:X8} got {selectedBus:X8}");

            _selectedBus = busIndex;
        }

        public void WriteByte(byte value)
        {
            Bus.WriteData(Address, value);
        }

        public void WriteBytes(byte[] values)
        {
            throw new NotImplementedException();
        }

        public void WriteUShort(byte address, ushort value, ByteOrder order = ByteOrder.LittleEndian)
        {
            throw new NotImplementedException();
        }

        public void WriteUShorts(byte address, ushort[] values, ByteOrder order = ByteOrder.LittleEndian)
        {
            throw new NotImplementedException();
        }

        public void WriteRegister(byte address, byte value)
        {
            throw new NotImplementedException();
        }

        public void WriteRegisters(byte address, byte[] data)
        {
            throw new NotImplementedException();
        }

        public byte[] WriteRead(byte[] write, ushort length)
        {
            throw new NotImplementedException();
        }

        public void WriteRead(Span<byte> writeBuffer, Span<byte> readBuffer)
        {
            throw new NotImplementedException();
        }

        public byte[] ReadBytes(ushort numberOfBytes)
        {
            return Bus.ReadData(Address, numberOfBytes);
        }

        public byte ReadRegister(byte address)
        {
            throw new NotImplementedException();
        }

        public byte[] ReadRegisters(byte address, ushort length)
        {
            throw new NotImplementedException();
        }

        public ushort ReadUShort(byte address, ByteOrder order)
        {
            throw new NotImplementedException();
        }

        public ushort[] ReadUShorts(byte address, ushort number, ByteOrder order = ByteOrder.LittleEndian)
        {
            throw new NotImplementedException();
        }
    }
}
