﻿using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Runtime.InteropServices;
using static Meadow.Foundation.ICs.IOExpanders.Native;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public sealed class Ft232I2cBus : II2cBus, IDisposable
    {
        private const byte DefaultLatencyTimer = 10;
        private const int DefaultChannelOptions = 0;

        private bool _isDisposed;

        internal bool IsOpen { get; private set; } = false;

        public int ChannelNumber { get; }
        private IntPtr Handle { get; set; }
        private FT_DEVICE_LIST_INFO_NODE InfoNode { get; }

        internal Ft232I2cBus(int channelNumber, FT_DEVICE_LIST_INFO_NODE info)
        {
            ChannelNumber = channelNumber;
            InfoNode = info;
        }

        public Frequency Frequency { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                CloseChannel();

                _isDisposed = true;
            }
        }

        ~Ft232I2cBus()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(false);
        }

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

        public void Exchange(byte peripheralAddress, Span<byte> writeBuffer, Span<byte> readBuffer)
        {
            Write(peripheralAddress, writeBuffer);
            Read(peripheralAddress, readBuffer);
        }

        public void Read(byte peripheralAddress, Span<byte> readBuffer)
        {
            uint transferred;

            var status = Functions.I2C_DeviceRead(
                Handle,
                peripheralAddress,
                readBuffer.Length,
                MemoryMarshal.GetReference(readBuffer),
                out transferred,
                I2CTransferOptions.START_BIT | I2CTransferOptions.STOP_BIT | I2CTransferOptions.NACK_LAST_BYTE
                );

            CheckStatus(status);
        }

        public void Write(byte peripheralAddress, Span<byte> writeBuffer)
        {
            uint transferred;

            var status = Functions.I2C_DeviceWrite(
                Handle,
                peripheralAddress,
                writeBuffer.Length,
                MemoryMarshal.GetReference(writeBuffer),
                out transferred,
                I2CTransferOptions.START_BIT | I2CTransferOptions.STOP_BIT | I2CTransferOptions.BREAK_ON_NACK
                );

            CheckStatus(status);
        }
    }
}