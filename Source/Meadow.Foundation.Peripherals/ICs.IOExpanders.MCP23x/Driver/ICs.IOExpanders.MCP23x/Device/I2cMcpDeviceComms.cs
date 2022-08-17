using System;
using System.IO;
using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders.Device
{
    public class I2cMcpDeviceComms : IMcpDeviceComms
    {
        private readonly I2cPeripheral _peripheral;

        public I2cMcpDeviceComms(II2cBus bus, byte peripheralAddress)
        {
            _peripheral = new I2cPeripheral(bus, peripheralAddress);
        }

        public byte ReadRegister(byte address)
        {
            var result = _peripheral.ReadRegister(address);

            LogRead(address, result);

            return result;
        }

        public byte[] ReadRegisters(byte address, ushort length)
        {
            if (length == 0)
            {
                return new byte[0];
            }

            var result = _peripheral.ReadRegisters(address, length);
            LogRead(address, result);
            return result;
        }

        public void WriteRegister(byte address, byte value)
        {
            _peripheral.WriteRegister(address, value);
            LogWrite(address, value);
        }

        public void WriteRegisters(byte address, byte[] data)
        {
            if (data.Length == 0)
            {
                return;
            }

            _peripheral.WriteRegisters(address, data);
            LogWrite(address, data);
        }

        private void LogRead(byte address, params byte[] results)
        {
            if (McpLogger.DebugOut == TextWriter.Null)
            {
                return;
            }

            McpLogger.DebugOut.Write($"I2C Read:  {Convert.ToString(address, 2).PadLeft(8, '0')} | ");

            foreach (var result in results)
            {
                McpLogger.DebugOut.Write(Convert.ToString(result, 2).PadLeft(8, '0') + ' ');
            }

            McpLogger.DebugOut.Write("\n");
        }

        private void LogWrite(byte address, params byte[] values)
        {
            if (McpLogger.DebugOut == TextWriter.Null)
            {
                return;
            }

            McpLogger.DebugOut.Write($"I2C Write: {Convert.ToString(address, 2).PadLeft(8, '0')} | ");
            foreach (var value in values)
            {
                McpLogger.DebugOut.Write(Convert.ToString(value, 2).PadLeft(8, '0') + ' ');
            }

            McpLogger.DebugOut.Write("\n");
        }
    }
}
