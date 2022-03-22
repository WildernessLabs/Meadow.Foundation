using System;
using System.Threading;
using Meadow.Hardware;

namespace Meadow.Foundation.ICs.EEPROM
{
    /// <summary>
    /// Encapsulation for EEPROMs based upon the AT24Cxx family of chips.
    /// </summary>
    public partial class At24Cxx
    {
        /// <summary>
        ///     Communication bus used to communicate with the EEPROM.
        /// </summary>
        private II2cPeripheral Peripheral { get; }

        /// <summary>
        ///     Number of bytes in a page.
        /// </summary>
        public ushort PageSize { get; }

        /// <summary>
        ///     Number of bytes in the EEPROM module.
        /// </summary>
        public ushort MemorySize { get; }

        private Memory<byte> ReadBuffer { get; set; }
        private Memory<byte> WriteBuffer { get; set; }

        /// <summary>
        ///     Create a new AT24Cxx object using the default parameters for the component.
        /// </summary>
        /// <param name="i2cBus">I2CBus connected to display</param>
        /// <param name="address">Address of the At24Cxx (default = 0x50).</param>
        /// <param name="pageSize">Number of bytes in a page (default = 32 - AT24C32).</param>
        /// <param name="memorySize">Total number of bytes in the EEPROM (default = 8192 - AT24C32).</param>
        public At24Cxx(II2cBus i2cBus,
            byte address = (byte)Address.Default,
            ushort pageSize = 32,
            ushort memorySize = 8192)
        {
            var device = new I2cPeripheral(i2cBus, address);
            Peripheral = device;
            PageSize = pageSize;
            MemorySize = memorySize;

            ReadBuffer = new byte[3];
            WriteBuffer = new byte[3];
        }

        /// <summary>
        ///     Check the startAddress and the amount of data being accessed to make sure that the
        ///     addresss and the startAddress plus the amount remain within the bounds of the memory chip.
        /// </summary>
        /// <param name="address">Start startAddress for the memory activity.</param>
        /// <param name="amount">Amunt of data to be accessed.</param>
        private void CheckAddress(ushort address, ushort amount)
        {
            if (address > MemorySize)
            {
                throw new ArgumentOutOfRangeException(
                    "address", "startAddress should be less than the amount of memory in the module");
            }
            if ((address + amount) > MemorySize)
            {
                throw new ArgumentOutOfRangeException(
                    "address", "startAddress + amount should be less than the amount of memory in the module");
            }
        }

        /// <summary>
        ///     Force the sensor to make a reading and update the relevant properties.
        /// </summary>
        /// <param name="startAddress">Start address for the read operation.</param>
        /// <param name="amount">Amount of data to read from the EEPROM.</param>
        public byte[] Read(ushort startAddress, ushort amount)
        {
            CheckAddress(startAddress, amount);
            Span<byte> data = WriteBuffer.Span[0..2];
            data[0] = (byte) ((startAddress >> 8) & 0xff);
            data[1] = (byte) (startAddress & 0xff);

            var results = new byte[amount];

            Peripheral.Write(data);
            Peripheral.Read(results);

            return results;
        }

        /// <summary>
        ///     Write a number of bytes to the EEPROM.
        /// </summary>
        /// <param name="startAddress">Address of he first byte to be written.</param>
        /// <param name="data">Data to be written to the EEPROM.</param>
        public void Write(ushort startAddress, params byte[] data)
        {
            CheckAddress(startAddress, (ushort) data.Length);
            //
            //  TODO: Convert to use page writes where possible.
            //
            for (ushort index = 0; index < data.Length; index++)
            {
                var address = (ushort) (startAddress + index);
                var addressAndData = new byte[3];
                addressAndData[0] = (byte) ((address >> 8) & 0xff);
                addressAndData[1] = (byte) (address & 0xff);
                addressAndData[2] = data[index];
                Peripheral.Write(addressAndData);
                Thread.Sleep(10);
            }
        }
    }
}