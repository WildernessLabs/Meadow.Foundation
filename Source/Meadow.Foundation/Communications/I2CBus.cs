using Meadow.Hardware;
using System;
using System.Runtime.CompilerServices;

namespace Meadow.Foundation.Communications
{
    /// <summary>
    ///     I2CBus object used to communicate with an I2C device using the ICommunicationBus
    ///     interface methods.
    /// </summary>
    public class I2CBus : ICommunicationBus
    {
        #region Member variables / fields.

        //    Config property of the I2CDevice.
        /// <summary>
        ///     I2C bus used to communicate with a device (sensor etc.).
        /// </summary>
        /// <remarks>
        ///     This I2CDevice is static and shared across all instances of the I2CBus.
        ///     Communication with difference devices is made possible by changing the
        /// </remarks>
        private static I2CDevice _device;

        /// <summary>
        ///     Configuration property for this I2CDevice.
        /// </summary>
        private readonly I2CDevice.Configuration _configuration;

        /// <summary>
        ///     Timeout for I2C transactions.
        /// </summary>
        private readonly ushort _transactionTimeout = 100;

        #endregion Member variables / fields.

        #region Constructors

        /// <summary>
        ///     Default constructor for the I2CBus class.  This is private to prevent the
        ///     developer from calling it.
        /// </summary>
        private I2CBus()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Meadow.Foundation.Core.I2CBus" /> class.
        /// </summary>
        /// <param name="address">Address of the device.</param>
        /// <param name="speed">Bus speed in kHz.</param>
        /// <param name="transactionTimeout">Transaction timeout in milliseconds.</param>
        public I2CBus(byte address, ushort speed, ushort transactionTimeout = 100)
        {
            _configuration = new I2CDevice.Configuration(address, speed);
            if (_device == null)
            {
                _device = new I2CDevice(_configuration);
            }
            _transactionTimeout = transactionTimeout;
        }

        #endregion Constructors

        #region ICommunicationBus methods.

        /// <summary>
        ///     Write a single byte to the device.
        /// </summary>
        /// <param name="value">Value to be written (8-bits).</param>
        public void WriteByte(byte value)
        {
            byte[] data = { value };
            WriteBytes(data);
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
            _device.Config = _configuration;
            I2CDevice.I2CTransaction[] transaction =
            {
                I2CDevice.CreateWriteTransaction(values)
            };
            var retryCount = 0;
            while (_device.Execute(transaction, _transactionTimeout) != values.Length)
            {
                if (retryCount > 3)
                {
                    throw new Exception("WriteBytes: Retry count exceeded.");
                }
                retryCount++;
            }
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
            _device.Config = _configuration;
            var read = new byte[length];
            I2CDevice.I2CTransaction[] transaction =
            {
                I2CDevice.CreateWriteTransaction(write),
                I2CDevice.CreateReadTransaction(read)
            };
            var bytesTransferred = 0;
            var retryCount = 0;

            while (_device.Execute(transaction, _transactionTimeout) != (write.Length + read.Length))
            {
                if (retryCount > 3)
                {
                    throw new Exception("WriteRead: Retry count exceeded.");
                }
                retryCount++;
            }

            //while (bytesTransferred != (write.Length + read.Length))
            //{
            //    if (retryCount > 3)
            //    {
            //        throw new Exception("WriteRead: Retry count exceeded.");
            //    }
            //    retryCount++;
            //    bytesTransferred = _device.Execute(transaction, _transactionTimeout);
            //}
            return read;
        }

        /// <summary>
        ///     Write data into a single register.
        /// </summary>
        /// <param name="address">Address of the register to write to.</param>
        /// <param name="value">Value to write into the register.</param>
        public void WriteRegister(byte address, byte value)
        {
            byte[] data = { address, value };
            WriteBytes(data);
        }

        /// <summary>
        ///     Write data to one or more registers.
        /// </summary>
        /// <remarks>
        ///     This method assumes that the register address is written first followed by the data to be
        ///     written into the first register followed by the data for subsequent registers.
        /// </remarks>
        /// <param name="address">Address of the first register to write to.</param>
        /// <param name="data">Data to write into the registers.</param>
        public void WriteRegisters(byte address, byte[] data)
        {
            var registerAndData = new byte[data.Length + 1];
            registerAndData[0] = address;
            Array.Copy(data, 0, registerAndData, 1, data.Length);
            WriteBytes(registerAndData);
        }

        /// <summary>
        ///     Read the specified number of bytes from the I2C device.
        /// </summary>
        /// <returns>The bytes.</returns>
        /// <param name="numberOfBytes">Number of bytes.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public byte[] ReadBytes(ushort numberOfBytes)
        {
            _device.Config = _configuration;
            var result = new byte[numberOfBytes];
            I2CDevice.I2CTransaction[] transaction =
            {
                I2CDevice.CreateReadTransaction(result)
            };
            var retryCount = 0;
            while (_device.Execute(transaction, _transactionTimeout) != numberOfBytes)
            {
                if (retryCount > 3)
                {
                    throw new Exception("ReadBytes: Retry count exceeded.");
                }
                retryCount++;
            }
            return result;
        }

        /// <summary>
        ///     Read a register from the device.
        /// </summary>
        /// <param name="address">Address of the register to read.</param>
        public byte ReadRegister(byte address)
        {
            byte[] registerAddress = { address };
            var result = WriteRead(registerAddress, 1);
            return result[0];
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
        ///     Read an usingned short from a pair of registers.
        /// </summary>
        /// <param name="address">Register address of the low byte (the high byte will follow).</param>
        /// <param name="order">Order of the bytes in the register (little endian is the default).</param>
        /// <returns>Value read from the register.</returns>
        public ushort ReadUShort(byte address, ByteOrder order = ByteOrder.LittleEndian)
        {
            var data = ReadRegisters(address, 2);
            ushort result = 0;
            if (order == ByteOrder.LittleEndian)
            {
                result = (ushort) ((data[1] << 8) + data[0]);
            }
            else
            {
                result = (ushort) ((data[0] << 8) + data[1]);
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
                    result[index] = (ushort) ((data[(2 * index) + 1] << 8) + data[2 * index]);
                }
                else
                {
                    result[index] = (ushort) ((data[2 * index] << 8) + data[(2 * index) + 1]);
                }
            }
            return result;
        }

        #endregion ICommunicationBus methods.
    }
}