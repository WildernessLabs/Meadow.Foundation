using Meadow.Hardware;
using System;

namespace Netduino.Foundation.Communications
{
    public class SPIBus : ICommunicationBus
    {
        #region Member variables / fields

        /// <summary>
        ///     SPI bus object.
        /// </summary>
        private static SPI _spi;

        /// <summary>
        ///     Configuration to use for this instance of the SPIBus.
        /// </summary>
        private SPI.Configuration _configuration;

        #endregion Member variables / fields

        #region Constructor(s)

        /// <summary>
        ///     Default constructor for the SPIBus.
        /// </summary>
        /// <remarks>
        ///     This is private to prevent the programmer using it.
        /// </remarks>
        private SPIBus()
        {
        }

        /// <summary>
        ///     Create a new SPIBus object.
        /// </summary>
        /// <param name="configuration">SPI bus configuration.</param>
        public SPIBus(SPI.Configuration configuration)
        {
            _configuration = configuration;
            _spi = new SPI(configuration);
        }

        /// <summary>
        ///     Create a new SPIBus object using the requested clock phase and polarity.
        /// </summary>
        /// <param name="module">SPI module to configure.</param>
        /// <param name="chipSelect">Chip select pin.</param>
        /// <param name="cpha">CPHA - Clock Phase (0 or 1).</param>
        /// <param name="cpol">CPOL - Clock Polarity (0 or 1).</param>
        /// <param name="speed">Speed of the SPI bus.</param>
        public SPIBus(SPI.SPI_module module, Cpu.Pin chipSelect, ushort speed = 1000, byte cpha = 0, byte cpol = 0)
        {
            Configure(module, chipSelect, cpha, cpol, speed);
            _spi = new SPI(_configuration);
        }

        /// <summary>
        ///     Create a new SPIBus operating in the specified mode.
        /// </summary>
        /// <remarks>
        ///     Mode    CPOL    CPHA
        ///     0       0       0
        ///     1       0       1
        ///     2       1       0
        ///     3       1       1
        /// </remarks>
        /// <param name="module">SPI module to configure.</param>
        /// <param name="chipSelect">Chip select pin.</param>
        /// <param name="mode">SPI Bus Mode - should be in the range 0 - 3.</param>
        /// <param name="speed">Speed of the SPI bus.</param>
        public SPIBus(SPI.SPI_module module, Cpu.Pin chipSelect, byte mode, ushort speed)
        {
            if (mode > 3)
            {
                throw new ArgumentException("SPI Mode should be in the range 0 - 3.");
            }
            byte cpha = 0;
            byte cpol = 0;
            switch (mode)
            {
                case 1:
                    cpha = 1;
                    break;
                case 2:
                    cpol = 1;
                    break;
                case 3:
                    cpol = 1;
                    cpha = 1;
                    break;
            }
            Configure(module, chipSelect, cpha, cpol, speed);
            _spi = new SPI(_configuration);
        }

        #endregion Constructor(s)

        #region Methods

        /// <summary>
        ///     Work out how the SPI bus should be configured from the clock polarity and phase.
        /// </summary>
        /// <param name="module">SPI module to configure.</param>
        /// <param name="chipSelect">Chip select pin.</param>
        /// <param name="cpha">CPHA - Clock phase (0 or 1).</param>
        /// <param name="cpol">CPOL - Clock polarity (0 or 1).</param>
        /// <param name="speed">Speed of the SPI bus.</param>
        /// <returns>SPI Configuration object.</returns>
        private void Configure(SPI.SPI_module module, Cpu.Pin chipSelect, byte cpha, byte cpol,
            ushort speed)
        {
            if (cpha > 1)
            {
                throw new ArgumentException("Clock phase should be 0 to 1.");
            }
            if (cpol > 1)
            {
                throw new ArgumentException("Clock polarity should be 0 to 1.");
            }
            _configuration = new SPI.Configuration(SPI_mod: module,
                                               ChipSelect_Port: chipSelect,
                                               ChipSelect_ActiveState: false,
                                               ChipSelect_SetupTime: 0,
                                               ChipSelect_HoldTime: 0,
                                               Clock_IdleState: (cpol == 1),
                                               Clock_Edge: (cpha == 1),
                                               Clock_RateKHz: speed);
        }

        /// <summary>
        ///     Write a single byte to the device.
        /// </summary>
        /// <param name="value">Value to be written (8-bits).</param>
        public void WriteByte(byte value)
        {
            WriteBytes(new[] { value });
        }

        /// <summary>
        ///     Write a number of bytes to the device.
        /// </summary>
        /// <remarks>
        ///     The number of bytes to be written will be determined by the length of the byte array.
        /// </remarks>
        /// <param name="values">Values to be written.</param>
        public void WriteBytes(byte[] values)
        {
            _spi.Config = _configuration;
            _spi.Write(values);
        }

        /// <summary>
        ///     Write an unsigned short to the device.
        /// </summary>
        /// <param name="address">Address to write the first byte to.</param>
        /// <param name="value">Value to be written (16-bits).</param>
        /// <param name="order">Indicate if the data should be written as big or little endian.</param>
        public void WriteUShort(byte address, ushort value, ByteOrder order = ByteOrder.LittleEndian)
        {
            var data = new byte[2];
            if (order == ByteOrder.LittleEndian)
            {
                data[0] = (byte) (value & 0xff);
                data[1] = (byte) ((value >> 8) & 0xff);
            }
            else
            {
                data[0] = (byte) ((value >> 8) & 0xff);
                data[1] = (byte) (value & 0xff);
            }
            WriteRegisters(address, data);
        }

        /// <summary>
        ///     Write a number of unsigned shorts to the device.
        /// </summary>
        /// <remarks>
        ///     The number of bytes to be written will be determined by the length of the byte array.
        /// </remarks>
        /// <param name="address">Address to write the first byte to.</param>
        /// <param name="values">Values to be written.</param>
        /// <param name="order">Indicate if the data should be written as big or little endian.</param>
        public void WriteUShorts(byte address, ushort[] values, ByteOrder order = ByteOrder.LittleEndian)
        {
            var data = new byte[2 * values.Length];
            for (var index = 0; index < values.Length; index++)
            {
                if (order == ByteOrder.LittleEndian)
                {
                    data[index * 2] = (byte) (values[index] & 0xff);
                    data[(index * 2) + 1] = (byte) ((values[index] >> 8) & 0xff);
                }
                else
                {
                    data[index * 2] = (byte) ((values[index] >> 8) & 0xff);
                    data[(index * 2) + 1] = (byte) (values[index] & 0xff);
                }
            }
            WriteRegisters(address, data);
        }

        /// <summary>
        ///     Write data a register in the device.
        /// </summary>
        /// <param name="address">Address of the register to write to.</param>
        /// <param name="value">Data to write into the register.</param>
        public void WriteRegister(byte address, byte value)
        {
            WriteRegisters(address, new[] { value });
        }

        /// <summary>
        ///     Write data to one or more registers.
        /// </summary>
        /// <param name="address">Address of the first register to write to.</param>
        /// <param name="data">Data to write into the registers.</param>
        public void WriteRegisters(byte address, byte[] values)
        {
            var data = new byte[values.Length + 1];
            data[0] = address;
            Array.Copy(values, 0, data, 1, values.Length);
            WriteBytes(data);
        }

        /// <summary>
        ///     Write data to the device and also read some data from the device.
        /// </summary>
        /// <remarks>
        ///     The number of bytes to be written and read will be determined by the length of the byte arrays.
        /// </remarks>
        /// <param name="write">Array of bytes to be written to the device.</param>
        /// <param name="length">Amount of data to read from the device.</param>
        public byte[] WriteRead(byte[] write, ushort length)
        {
            var result = new byte[length];
            _spi.Config = _configuration;
            _spi.WriteRead(write, result);
            return result;
        }

        /// <summary>
        ///     Read the specified number of bytes from the I2C device.
        /// </summary>
        /// <returns>The bytes.</returns>
        /// <param name="numberOfBytes">Number of bytes.</param>
        public byte[] ReadBytes(ushort numberOfBytes)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Read a registers from the device.
        /// </summary>
        /// <param name="address">Address of the register to read.</param>
        public byte ReadRegister(byte address)
        {
            return WriteRead(new[] { address }, 1)[0];
        }

        /// <summary>
        ///     Read one or more registers from the device.
        /// </summary>
        /// <param name="address">Address of the first register to read.</param>
        /// <param name="length">Number of bytes to read from the device.</param>
        public byte[] ReadRegisters(byte address, ushort length)
        {
            return WriteRead(new[] { address }, length);
        }

        /// <summary>
        ///     Read an unsigned short from a pair of registers.
        /// </summary>
        /// <param name="address">Register address of the low byte (the high byte will follow).</param>
        /// <param name="order">Order of the bytes in the register (little endian is the default).</param>
        /// <returns>Value read from the register.</returns>
        public ushort ReadUShort(byte address, ByteOrder order = ByteOrder.LittleEndian)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Read the specified number of unsigned shorts starting at the register
        ///     address specified.
        /// </summary>
        /// <param name="address">First register address to read from.</param>
        /// <param name="number">Number of unsigned shorts to read.</param>
        /// <param name="order">Order of the bytes (Little or Big endian)</param>
        /// <returns>Array of unsigned shorts.</returns>
        public ushort[] ReadUShorts(byte address, ushort number, ByteOrder order = ByteOrder.LittleEndian)
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}