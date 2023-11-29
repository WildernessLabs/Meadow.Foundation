using Meadow.Hardware;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static Meadow.Foundation.ICs.IOExpanders.Native;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represents an I2C bus using the FT232H USB to I2C bridge.
    /// </summary>
    public sealed class Ft232I2cBus : IFt232Bus, II2cBus, IDisposable
    {
        private const byte DefaultLatencyTimer = 10;
        private const I2CChannelOptions DefaultChannelOptions = I2CChannelOptions.None;

        private bool _isDisposed;

        /// <summary>
        /// Gets the handle for the FT232H I2C bus.
        /// </summary>
        public IntPtr Handle { get; private set; }

        /// <summary>
        /// Gets or sets the GPIO direction mask for the FT232H I2C bus.
        /// </summary>
        public byte GpioDirectionMask { get; set; }

        /// <summary>
        /// Gets or sets the GPIO state for the FT232H I2C bus.
        /// </summary>
        public byte GpioState { get; set; }

        internal bool IsOpen { get; private set; } = false;
        internal int ChannelNumber { get; }
        private FT_DEVICE_LIST_INFO_NODE InfoNode { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Ft232I2cBus"/> class.
        /// </summary>
        internal Ft232I2cBus(int channelNumber, FT_DEVICE_LIST_INFO_NODE info)
        {
            ChannelNumber = channelNumber;
            InfoNode = info;
        }

        /// <summary>
        /// Gets or sets the bus speed for the FT232H I2C bus.
        /// </summary>
        public I2cBusSpeed BusSpeed { get; set; }

        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                CloseChannel();

                _isDisposed = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Ft232I2cBus"/> class.
        /// </summary>
        ~Ft232I2cBus()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(false);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="Ft232I2cBus"/> object.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void Open(I2CClockRate clockRate = I2CClockRate.Standard)
        {
            if (CheckStatus(Functions.I2C_OpenChannel(ChannelNumber, out IntPtr handle)))
            {
                Handle = handle;

                var config = new I2CChannelConfig
                {
                    ClockRate = clockRate,
                    LatencyTimer = DefaultLatencyTimer,
                    Options = DefaultChannelOptions
                };

                CheckStatus(Functions.I2C_InitChannel(Handle, ref config));

                IsOpen = true;
            }
        }

        private void CloseChannel()
        {
            if (Handle != IntPtr.Zero)
            {
                CheckStatus(Functions.I2C_CloseChannel(Handle));
                Handle = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Exchanges data with a peripheral on the I2C bus.
        /// </summary>
        /// <param name="peripheralAddress">The address of the peripheral device.</param>
        /// <param name="writeBuffer">The data to write to the peripheral.</param>
        /// <param name="readBuffer">The data to read from the peripheral.</param>
        public void Exchange(byte peripheralAddress, Span<byte> writeBuffer, Span<byte> readBuffer)
        {
            Write(peripheralAddress, writeBuffer);
            Read(peripheralAddress, readBuffer);
        }

        /// <summary>
        /// Reads data from a peripheral on the I2C bus.
        /// </summary>
        /// <param name="peripheralAddress">The address of the peripheral device.</param>
        /// <param name="readBuffer">The buffer to store the read data.</param>
        public void Read(byte peripheralAddress, Span<byte> readBuffer)
        {
            var status = Functions.I2C_DeviceRead(
                Handle,
                peripheralAddress,
                readBuffer.Length,
                MemoryMarshal.GetReference(readBuffer),
                out int transferred,
                I2CTransferOptions.FAST_TRANSFER | I2CTransferOptions.FAST_TRANSFER_BYTES
                //I2CTransferOptions.START_BIT | I2CTransferOptions.STOP_BIT | I2CTransferOptions.NACK_LAST_BYTE
                //I2CTransferOptions.START_BIT | I2CTransferOptions.STOP_BIT | I2CTransferOptions.FAST_TRANSFER | I2CTransferOptions.NACK_LAST_BYTE
                );

            Debug.WriteLine($"transferred: {transferred}");
            CheckStatus(status);
        }

        /// <summary>
        /// Writes data to a peripheral on the I2C bus.
        /// </summary>
        /// <param name="peripheralAddress">The address of the peripheral device.</param>
        /// <param name="writeBuffer">The data to write to the peripheral.</param>
        public void Write(byte peripheralAddress, Span<byte> writeBuffer)
        {
            var status = Functions.I2C_DeviceWrite(
                Handle,
                peripheralAddress,
                writeBuffer.Length,
                MemoryMarshal.GetReference(writeBuffer),
                out int transferred,
                I2CTransferOptions.FAST_TRANSFER | I2CTransferOptions.FAST_TRANSFER_BYTES
                //I2CTransferOptions.START_BIT | I2CTransferOptions.BREAK_ON_NACK
                //I2CTransferOptions.START_BIT | I2CTransferOptions.STOP_BIT | I2CTransferOptions.NACK_LAST_BYTE
                );

            Debug.WriteLine($"transferred: {transferred}");
            //            CheckStatus(status);
        }
    }
}
