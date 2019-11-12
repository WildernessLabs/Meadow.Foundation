using System;
using System.Threading;
using Meadow.Hardware;

namespace Meadow.Foundation.ICs.EEPROM
{
    /// <summary>
    ///     Encapsulation for EEPROMs based upon the AT24Cxx family of chips.
    /// </summary>
    public class AT24Cxx
    {
        #region Member variables / fields

        /// <summary>
        ///     Communication bus used to communicate with the EEPEOM.
        /// </summary>
        private readonly II2cPeripheral _eeprom;

        /// <summary>
        ///     Number of bytes in a page.
        /// </summary>
        private ushort _pageSize;

        /// <summary>
        ///     Number of bytes in the EEPROM module.
        /// </summary>
        private readonly ushort _memorySize;

        #endregion Member variables / fields

        #region Constructors

        /// <summary>
        ///     Default constructor is private to prevent the developer from calling it.
        /// </summary>
        private AT24Cxx()
        {
        }

        /// <summary>
        ///     Create a new AT24Cxx object using the default parameters for the component.
        /// </summary>
        /// <param name="address">Address of the MAG3110 (default = 0x50).</param>
        /// <param name="pageSize">Number of bytes in a page (default = 32 - AT24C32).</param>
        /// <param name="memorySize">Total number of bytes in the EEPROM (default = 8192 - AT24C32).</param>
        public AT24Cxx(II2cBus i2cBus, byte address = 0x50, ushort pageSize = 32, ushort memorySize = 8192)
        {
            var device = new I2cPeripheral(i2cBus, address);
            _eeprom = device;
            _pageSize = pageSize;
            _memorySize = memorySize;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        ///     Check the startAddress and the amount of data being accessed to make sure that the
        ///     addresss and the startAddress plus the amount remain within the bounds of the memory chip.
        /// </summary>
        /// <param name="address">Start startAddress for the memory activity.</param>
        /// <param name="amount">Amunt of data to be accessed.</param>
        private void CheckAddress(ushort address, ushort amount)
        {
            if (address >= _memorySize)
            {
                throw new ArgumentOutOfRangeException(
                    "address", "startAddress should be less than the amount of memory in the module");
            }
            if ((address + amount) >= _memorySize)
            {
                throw new ArgumentOutOfRangeException(
                    "address", "startAddress + amount should be less than the amount of memory in the module");
            }
        }

        /// <summary>
        ///     Force the sensor to make a reading and update the relevanyt properties.
        /// </summary>
        /// <param name="startAddress">Start address for the read operation.</param>
        /// <param name="amount">Amount of data to read from the EEPROM.</param>
        public byte[] Read(ushort startAddress, ushort amount)
        {
            CheckAddress(startAddress, amount);
            var address = new byte[2];
            address[0] = (byte) ((startAddress >> 8) & 0xff);
            address[1] = (byte) (startAddress & 0xff);
            _eeprom.WriteBytes(address);
            return _eeprom.ReadBytes(amount);
        }

        /// <summary>
        ///     Write a number of bytes to the EEPROM.
        /// </summary>
        /// <param name="startAddress">Address of he first byte to be written.</param>
        /// <param name="data">Data to be written to the EEPROM.</param>
        public void Write(ushort startAddress, byte[] data)
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
                _eeprom.WriteBytes(addressAndData);
                Thread.Sleep(10);
            }
        }

        #endregion
    }
}