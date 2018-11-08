using Meadow;
using System;
using System.Runtime.CompilerServices;
/*
namespace Meadow.Foundation.Communications
{
    /// <summary>
    ///     Implement a software version of the SPI communication protocol.
    /// </summary>
    public class SoftwareSPIBus : ICommunicationBus
    {
        #region Member variables / fields

        /// <summary>
        ///     MOSI output port.
        /// </summary>
        private DigitalOutputPort _mosi;

        /// <summary>
        ///     MISO Input port.
        /// </summary>
        private DigitalInputPort _miso;

        /// <summary>
        ///     Clock output port.
        /// </summary>
        private DigitalOutputPort _clock;

        /// <summary>
        ///     Chip select port.
        /// </summary>
        private DigitalOutputPort _chipSelect;

        /// <summary>
        ///     Boolean representation of the clock polarity.
        /// </summary>
        private readonly bool _polarity;

        /// <summary>
        ///     Boolean representation of the clock phase.
        /// </summary>
        private readonly bool _phase;

        #endregion Member variables / fields

        #region Constructors

        /// <summary>
        ///     Default constructor (private to prevent it from being used).
        /// </summary>
        private SoftwareSPIBus()
        {
        }

        /// <summary>
        ///     Create a new SoftwareSPIBus object using the specified pins.
        /// </summary>
        /// <param name="mosi">MOSI pin.</param>
        /// <param name="miso">MISO pin</param>
        /// <param name="clock">Clock pin.</param>
        /// <param name="chipSelect">Chip select pin.</param>
        /// <param name="cpha">Clock phase (0 or 1, default is 0).</param>
        /// <param name="cpol">Clock polarity (0 or 1, default is 0).</param>
        public SoftwareSPIBus(Cpu.Pin mosi, Cpu.Pin miso, Cpu.Pin clock, Cpu.Pin chipSelect, byte cpha = 0, byte cpol = 0)
        {
            if (mosi == Cpu.Pin.GPIO_NONE)
            {
                throw new ArgumentException("MOSI line cannot be set to None.");
            }
            if (clock == Cpu.Pin.GPIO_NONE)
            {
                throw new ArgumentException("Clock line cannot be set to None");
            }

            _phase = (cpha == 1);
            _polarity = (cpol == 1);
            _mosi = new DigitalOutputPort(mosi, false);
            _miso = miso == Cpu.Pin.GPIO_NONE ? null : new InputPort(miso, false, Port.ResistorMode.Disabled);
            _clock = new DigitalOutputPort(clock, _polarity);
            _chipSelect = chipSelect == Cpu.Pin.GPIO_NONE ? null : new DigitalOutputPort(chipSelect, true);
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        ///     Write a single byte to the SPI bus.
        /// </summary>
        /// <param name="value">Byte value to write to the SPI bus.</param>
        private void Write(byte value)
        {
            byte mask = 0x80;
            var clock = _phase;

            for (var index = 0; index < 8; index++)
            {
                _mosi.Write((value & mask) > 0);
                _clock.Write(!clock);
                _clock.Write(clock);
                mask >>= 1;
            }
        }

        /// <summary>
        ///     Write a single byte to the device.
        /// </summary>
        /// <param name="value">Value to be written (8-bits).</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void WriteByte(byte value)
        {
            _chipSelect?.Write(false);
            Write(value);
            _chipSelect?.Write(true);
        }

        /// <summary>
        ///     Write a number of bytes to the device.
        /// </summary>
        /// <remarks>
        ///     The number of bytes to be written will be determined by the length of the byte array.
        /// </remarks>
        /// <param name="values">Values to be written.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void WriteBytes(byte[] values)
        {
            _chipSelect?.Write(false);
            for (int index = 0; index < values.Length; index++)
            {
                Write(values[index]);
            }
            _chipSelect?.Write(true);
        }

        /// <summary>
        ///     Write data a register in the device.
        /// </summary>
        /// <param name="register">Address of the register to write to.</param>
        /// <param name="value">Data to write into the register.</param>
        public void WriteRegister(byte register, byte value)
        {
            byte[] values = new byte[] { register, value};
            WriteBytes(values);
        }

        /// <summary>
        ///     Write data to one or more registers.
        /// </summary>
        /// <param name="address">Address of the first register to write to.</param>
        /// <param name="data">Data to write into the registers.</param>
        public void WriteRegisters(byte address, byte[] data)
        {
            byte[] values = new byte[data.Length + 1];
            values[0] = address;
            Array.Copy(data, 0, values, 1, data.Length);
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
                    data[2 * index] = (byte) (values[index] & 0xff);
                    data[(2 * index) + 1] = (byte) ((values[index] >> 8) & 0xff);
                }
                else
                {
                    data[2 * index] = (byte) ((values[index] >> 8) & 0xff);
                    data[(2 * index) + 1] = (byte) (values[index] & 0xff);
                }
            }
            WriteRegisters(address, data);
        }

        /// <summary>
        ///     Write and read a single byte.
        /// </summary>
        /// <remarks>
        ///     This internal method assumes that CS has been asserted correctly
        ///     before it is called.
        /// </remarks>
        /// <param name="value">Value to write.</param>
        /// <returns>Byte read from the SPI interface.</returns>
        private byte WriteRead(byte value)
        {
            byte result = 0;
            byte mask = 0x80;
            var clock = _phase;

            for (var index = 0; index < 8; index++)
            {
                _mosi.Write((value & mask) > 0);
                _clock.Write(!clock);
                bool data;
                if (_phase)
                {
                    _clock.Write(clock);
                    data = _miso.Read();
                }
                else
                {
                    data = _miso.Read();
                    _clock.Write(clock);
                }
                result <<= 1;
                if (data)
                {
                    result |= 0x01;
                }
                mask >>= 1;
            }
            return(result);
        }

        /// <summary>
        ///     Write data to the device and also read some data from the device.
        /// </summary>
        /// <remarks>
        ///     The number of bytes to be written and read will be determined by the length of the byte arrays.
        /// </remarks>
        /// <param name="write">Array of bytes to be written to the device.</param>
        /// <param name="length">Amount of data to read from the device.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public byte[] WriteRead(byte[] write, ushort length)
        {
            if (length < write.Length)
            {
                throw new ArgumentException(nameof(length));
            }
            if (_miso == null)
            {
                throw new InvalidOperationException("Cannot read from SPI bus when the MISO pin is set to None");
            }
            _chipSelect?.Write(false);
            byte[] result = new byte[length];
            for (var index = 0; index < length; index++)
            {
                byte value = 0;
                if (index < write.Length)
                {
                    value = write[index];
                }
                result[index] = WriteRead(value);
            }
            _chipSelect?.Write(true);
            _mosi.Write(false);
            return(result);
        }

        /// <summary>
        ///     Read the specified number of bytes from the SPI device.
        /// </summary>
        /// <param name="numberOfBytes">Number of bytes.</param>
        /// <returns>The bytes read from the device.</returns>
        public byte[] ReadBytes(ushort numberOfBytes)
        {
            byte[] values = new byte[numberOfBytes];
            for (int index = 0; index < numberOfBytes; index++)
            {
                values[index] = 0;
            }
            return(WriteRead(values, numberOfBytes));
        }

        /// <summary>
        ///     Read a register from the device.
        /// </summary>
        /// <param name="address">Address of the register to read.</param>
        public byte ReadRegister(byte address)
        {
            return(WriteRead(address));
        }

        /// <summary>
        ///     Read one or more registers from the device.
        /// </summary>
        /// <param name="address">Address of the first register to read.</param>
        /// <param name="length">Number of bytes to read from the device.</param>
        public byte[] ReadRegisters(byte address, ushort length)
        {
            byte[] registerAddress = { address };
            return WriteRead(registerAddress, length);
        }

        /// <summary>
        ///     Read an unsigned short from a pair of registers.
        /// </summary>
        /// <param name="address">Register address of the low byte (the high byte will follow).</param>
        /// <param name="order">Order of the bytes in the register (little endian is the default).</param>
        /// <returns>Value read from the register.</returns>
        public ushort ReadUShort(byte address, ByteOrder order = ByteOrder.LittleEndian)
        {
            var data = ReadRegisters(address, 3);
            ushort result = 0;
            if (order == ByteOrder.LittleEndian)
            {
                result = (ushort) ((data[2] << 8) + data[1]);
            }
            else
            {
                result = (ushort) ((data[1] << 8) + data[2]);
            }
            return result;
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
            var data = ReadRegisters(address, (ushort) ((2 * number) & 0xffff));
            var result = new ushort[number];
            for (var index = 0; index < number; index++)
            {
                if (order == ByteOrder.LittleEndian)
                {
                    result[index] = (ushort) ((data[(2 * index) + 2] << 8) + data[(2 * index) + 1]);
                }
                else
                {
                    result[index] = (ushort) ((data[(2 * index) + 1] << 8) + data[(2 * index) + 2]);
                }
            }
            return result;
        }

        #endregion Methods
    }
}

*/