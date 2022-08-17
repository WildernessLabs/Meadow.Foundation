using System;
using System.IO;
using Meadow.Hardware;
using Meadow.Utilities;

namespace Meadow.Foundation.ICs.IOExpanders.Device
{
    public class SpiMcpDeviceComms : IMcpDeviceComms
    {
        // 10 MHz
        private const long MaximumSpeed = 10_000L;

        private readonly byte _readAddress;
        private readonly byte _writeAddress;
        private readonly ISpiPeripheral _peripheral;

        public SpiMcpDeviceComms(ISpiBus bus, IDigitalOutputPort chipSelect, byte peripheralAddress)
        {
            //if (bus.Configuration.SpeedKHz > MaximumSpeed)
            //{
            //    throw new ArgumentException(
            //        $"Maximum SPI clock speed is {MaximumSpeed}KHz, bus is configured to {bus.Configuration.SpeedKHz}KHz",
            //        nameof(bus));
            //}

            _readAddress = BitHelpers.SetBit((byte) (peripheralAddress << 1), 0x00, false);
            _writeAddress = BitHelpers.SetBit((byte) (peripheralAddress << 1), 0x00, true);

            McpLogger.DebugOut.WriteLine($"SPI Write address:  {Convert.ToString(_readAddress, 2).PadLeft(8, '0')}");
            McpLogger.DebugOut.WriteLine($"SPI Read address: {Convert.ToString(_writeAddress, 2).PadLeft(8, '0')}");
            _peripheral = new SpiPeripheral(bus, chipSelect);
        }

        public byte ReadRegister(byte address)
        {
            return ReadRegisters(address, 1)[0];
        }

        public byte[] ReadRegisters(byte address, ushort length)
        {
            if (length == 0)
            {
                return new byte[0];
            }

            var readBytes = new[] { _readAddress, address };
            var result = _peripheral.WriteRead(new[] { _readAddress, address }, length);
            LogRead(readBytes, result);
            return result;
        }

        public void WriteRegister(byte address, byte value)
        {
            var writeBytes = new[] { _writeAddress, address, value };
            _peripheral.WriteBytes(writeBytes);
            LogWrite(writeBytes);
        }

        public void WriteRegisters(byte address, byte[] data)
        {
            if (data.Length == 0)
            {
                return;
            }

            var writeBytes = new byte[2 + data.Length];
            writeBytes[0] = _writeAddress;
            writeBytes[1] = address;
            data.CopyTo(writeBytes, 2);
            _peripheral.WriteBytes(writeBytes);
            LogWrite(writeBytes);
        }

        void LogRead(byte[] addresses, byte[] results)
        {
            if (McpLogger.DebugOut == TextWriter.Null)
            {
                return;
            }

            McpLogger.DebugOut.Write("SPI Read:  ");
            foreach (var address in addresses)
            {
                McpLogger.DebugOut.Write(Convert.ToString(address, 2).PadLeft(8, '0') + ' ');
            }

            McpLogger.DebugOut.Write("| ");

            foreach (var result in results)
            {
                McpLogger.DebugOut.Write(Convert.ToString(result, 2).PadLeft(8, '0') + ' ');
            }

            McpLogger.DebugOut.Write("\n");
        }
        void LogWrite(byte[] addresses)
        {
            if (McpLogger.DebugOut == TextWriter.Null)
            {
                return;
            }

            McpLogger.DebugOut.Write("SPI Write: ");
            foreach (var address in addresses)
            {
                McpLogger.DebugOut.Write(Convert.ToString(address, 2).PadLeft(8, '0') + ' ');
            }

            McpLogger.DebugOut.Write("\n");
        }
    }
}
