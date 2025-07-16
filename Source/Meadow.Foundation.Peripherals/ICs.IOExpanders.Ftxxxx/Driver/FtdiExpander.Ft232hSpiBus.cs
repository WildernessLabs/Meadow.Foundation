using Meadow.Hardware;
using Meadow.Units;
using System;
using System.IO;
using System.Threading;

namespace Meadow.Foundation.ICs.IOExpanders;

public abstract partial class FtdiExpander
{
    /// <summary>
    /// Represents an SPI bus using the FT232H
    /// </summary>
    public class Ft232hSpiBus : SpiBus
    {
        private readonly FtdiExpander _expander;
        private readonly SpiClockConfiguration _configuration;
        private const int DefaultTimeoutMs = 5000;
        private const int PollIntervalMs = 1;

        /// <inheritdoc/>
        public override Frequency[] SupportedSpeeds =>
            new Frequency[]
            {
                    100.Hertz(), 1000.Hertz(), 10000.Hertz(), 100000.Hertz(), 1000000.Hertz()
            };

        /// <inheritdoc/>
        public override SpiClockConfiguration Configuration => _configuration;

        internal Ft232hSpiBus(FtdiExpander expander, SpiClockConfiguration configuration)
        {
            _configuration = configuration;
            _expander = expander;
        }

        internal override void Configure()
        {
            // Proper MPSSE reset sequence
            _expander.Write(new byte[] { (byte)Native.FT_OPCODE.DisconnectTDItoTDOforLoopback });
            Thread.Sleep(50);

            // Clear any pending data
            ClearInputBuffer();

            // Synchronize the MPSSE interface properly
            if (!SynchronizeMpsse())
            {
                throw new IOException("Failed to synchronize MPSSE interface");
            }

            // Configure MPSSE for SPI
            var configBytes = new byte[20];
            int idx = 0;

            // Disable clock divide by 5 for 60MHz master clock
            configBytes[idx++] = (byte)Native.FT_OPCODE.DisableClockDivideBy5;

            // Turn off adaptive clocking
            configBytes[idx++] = (byte)Native.FT_OPCODE.TurnOffAdaptiveClocking;

            // Disable 3-phase clocking (not needed for SPI)
            configBytes[idx++] = (byte)Native.FT_OPCODE.Disable3PhaseDataClocking;

            // Set clock divisor - CORRECTED FORMULA
            configBytes[idx++] = (byte)Native.FT_OPCODE.SetClockDivisor;

            // Correct clock calculation: TCK = 60MHz / ((1 + divisor) * 2)
            uint targetFreqHz = (uint)_configuration.Speed.Hertz;
            uint clockDivisor = (60000000 / (targetFreqHz * 2)) - 1;

            // Ensure minimum divisor
            if (clockDivisor > 65535) clockDivisor = 65535;

            configBytes[idx++] = (byte)(clockDivisor & 0xFF);
            configBytes[idx++] = (byte)((clockDivisor >> 8) & 0xFF);

            // Configure pins BUT exclude D3 (CS) from SPI control
            configBytes[idx++] = (byte)Native.FT_OPCODE.SetDataBitsLowByte;

            byte initialClockState = (_configuration.SpiMode == SpiClockConfiguration.Mode.Mode2 ||
                              _configuration.SpiMode == SpiClockConfiguration.Mode.Mode3) ? (byte)0x01 : (byte)0x00;

            // CS high (inactive), MOSI low, clock according to mode  
            byte initialState = (byte)(0x08 | initialClockState); // CS=1, SCK=mode dependent, MOSI=0

            _expander.GpioStateLow = (byte)((initialState & 0x0F) | (_expander.GpioStateLow & 0xF0));

            // CRITICAL: Set pin directions WITHOUT including CS in SPI control
            // 0x03 = SCK=out, MOSI=out, MISO=in, CS=NOT controlled by SPI
            _expander.GpioDirectionLow = (byte)(0x03 | (_expander.GpioDirectionLow & 0xF0));

            configBytes[idx++] = _expander.GpioStateLow;
            configBytes[idx++] = _expander.GpioDirectionLow;

            // Send configuration
            _expander.Write(new ReadOnlySpan<byte>(configBytes, 0, idx));
            Thread.Sleep(10);

            // Verify configuration by reading pin states
            _expander.Write(new byte[] { (byte)Native.FT_OPCODE.ReadDataBitsLowByte, (byte)Native.FT_OPCODE.SendImmediate });
            var pinState = new byte[1];
            if (!ReadWithTimeout(pinState, 1000))
            {
                throw new IOException("Failed to verify SPI pin configuration");
            }
        }

        private bool SynchronizeMpsse()
        {
            // Send bad command 0xAA
            _expander.Write(new byte[] { 0xAA });
            var response = new byte[2];
            if (!ReadWithTimeout(response, 1000) || response[0] != 0xFA || response[1] != 0xAA)
            {
                return false;
            }

            // Send bad command 0xAB
            _expander.Write(new byte[] { 0xAB });
            if (!ReadWithTimeout(response, 1000) || response[0] != 0xFA || response[1] != 0xAB)
            {
                return false;
            }

            return true;
        }

        private byte GetSpiCommand(bool isRead, bool isWrite)
        {
            // Select correct MPSSE command based on SPI mode
            return _configuration.SpiMode switch
            {
                SpiClockConfiguration.Mode.Mode0 when isRead && isWrite =>
                    (byte)Native.FT_OPCODE.ClockDataBytesOutOnMinusBytesInOnPlusVeClockMSBFirst,
                SpiClockConfiguration.Mode.Mode0 when isWrite =>
                    (byte)Native.FT_OPCODE.ClockDataBytesOutOnMinusVeClockMSBFirst,
                SpiClockConfiguration.Mode.Mode0 when isRead =>
                    (byte)Native.FT_OPCODE.ClockDataBytesInOnPlusVeClockMSBFirst,

                SpiClockConfiguration.Mode.Mode1 when isRead && isWrite =>
                    (byte)Native.FT_OPCODE.ClockDataBytesOutOnPlusBytesInOnMinusVeClockMSBFirst,
                SpiClockConfiguration.Mode.Mode1 when isWrite =>
                    (byte)Native.FT_OPCODE.ClockDataBytesOutOnPlusVeClockMSBFirst,
                SpiClockConfiguration.Mode.Mode1 when isRead =>
                    (byte)Native.FT_OPCODE.ClockDataBytesInOnMinusVeClockMSBFirst,

                SpiClockConfiguration.Mode.Mode2 when isRead && isWrite =>
                    (byte)Native.FT_OPCODE.ClockDataBytesOutOnPlusBytesInOnMinusVeClockMSBFirst,
                SpiClockConfiguration.Mode.Mode2 when isWrite =>
                    (byte)Native.FT_OPCODE.ClockDataBytesOutOnPlusVeClockMSBFirst,
                SpiClockConfiguration.Mode.Mode2 when isRead =>
                    (byte)Native.FT_OPCODE.ClockDataBytesInOnMinusVeClockMSBFirst,

                SpiClockConfiguration.Mode.Mode3 when isRead && isWrite =>
                    (byte)Native.FT_OPCODE.ClockDataBytesOutOnMinusBytesInOnPlusVeClockMSBFirst,
                SpiClockConfiguration.Mode.Mode3 when isWrite =>
                    (byte)Native.FT_OPCODE.ClockDataBytesOutOnMinusVeClockMSBFirst,
                SpiClockConfiguration.Mode.Mode3 when isRead =>
                    (byte)Native.FT_OPCODE.ClockDataBytesInOnPlusVeClockMSBFirst,

                _ => throw new ArgumentException($"Unsupported SPI mode: {_configuration.SpiMode}")
            };
        }

        /// <inheritdoc/>
        public override void Exchange(IDigitalOutputPort? chipSelect, Span<byte> writeBuffer, Span<byte> readBuffer, ChipSelectMode csMode = ChipSelectMode.ActiveLow)
        {
            if (writeBuffer.Length != readBuffer.Length)
            {
                throw new ArgumentException("Write and read buffers must be the same length for full-duplex operation");
            }

            if (writeBuffer.Length == 0) return;

            ClearInputBuffer();

            // Improved CS timing for BME680
            if (chipSelect != null)
            {
                // Assert CS with longer setup time
                chipSelect.State = csMode == ChipSelectMode.ActiveLow ? false : true;
                Thread.Sleep(2); // Longer CS setup time
            }

            try
            {
                var commandBuffer = new byte[3 + writeBuffer.Length];
                int idx = 0;

                commandBuffer[idx++] = GetSpiCommand(true, true);
                int length = writeBuffer.Length - 1;
                commandBuffer[idx++] = (byte)(length & 0xFF);
                commandBuffer[idx++] = (byte)((length >> 8) & 0xFF);
                writeBuffer.CopyTo(new Span<byte>(commandBuffer, idx, writeBuffer.Length));

                _expander.Write(commandBuffer);

                // Add delay before reading response
                Thread.Sleep(1);

                if (!ReadWithTimeout(readBuffer, DefaultTimeoutMs))
                {
                    throw new TimeoutException($"SPI read timeout. Expected {readBuffer.Length} bytes.");
                }
            }
            finally
            {
                if (chipSelect != null)
                {
                    // Longer hold time before deasserting CS
                    Thread.Sleep(2);
                    chipSelect.State = csMode == ChipSelectMode.ActiveLow ? true : false;
                }
            }
        }

        public override void Write(IDigitalOutputPort? chipSelect, Span<byte> writeBuffer, ChipSelectMode csMode = ChipSelectMode.ActiveLow)
        {
            if (writeBuffer.Length == 0) return;

            if (chipSelect != null)
            {
                chipSelect.State = csMode == ChipSelectMode.ActiveLow ? false : true;
            }

            try
            {
                var commandBuffer = new byte[3 + writeBuffer.Length];
                int idx = 0;

                commandBuffer[idx++] = GetSpiCommand(false, true);
                int length = writeBuffer.Length - 1;
                commandBuffer[idx++] = (byte)(length & 0xFF);
                commandBuffer[idx++] = (byte)((length >> 8) & 0xFF);

                writeBuffer.CopyTo(new Span<byte>(commandBuffer, idx, writeBuffer.Length));
                _expander.Write(commandBuffer);
            }
            finally
            {
                if (chipSelect != null)
                {
                    chipSelect.State = csMode == ChipSelectMode.ActiveLow ? true : false;
                }
            }
        }

        public override void Read(IDigitalOutputPort? chipSelect, Span<byte> readBuffer, ChipSelectMode csMode = ChipSelectMode.ActiveLow)
        {
            if (readBuffer.Length == 0) return;

            if (chipSelect != null)
            {
                chipSelect.State = csMode == ChipSelectMode.ActiveLow ? false : true;
            }

            try
            {
                ClearInputBuffer();

                var commandBuffer = new byte[4];
                int idx = 0;

                commandBuffer[idx++] = GetSpiCommand(true, false);
                int length = readBuffer.Length - 1;
                commandBuffer[idx++] = (byte)(length & 0xFF);
                commandBuffer[idx++] = (byte)((length >> 8) & 0xFF);
                commandBuffer[idx++] = (byte)Native.FT_OPCODE.SendImmediate;

                _expander.Write(commandBuffer);

                if (!ReadWithTimeout(readBuffer, DefaultTimeoutMs))
                {
                    throw new TimeoutException($"SPI read timeout. Expected {readBuffer.Length} bytes.");
                }
            }
            finally
            {
                if (chipSelect != null)
                {
                    chipSelect.State = csMode == ChipSelectMode.ActiveLow ? true : false;
                }
            }
        }

        private bool ReadWithTimeout(Span<byte> buffer, int timeoutMs)
        {
            var startTime = Environment.TickCount;
            int totalRead = 0;

            while (totalRead < buffer.Length)
            {
                uint available = 0;
                Native.CheckStatus(Native.Ftd2xx.FT_GetQueueStatus(_expander.Handle, ref available));

                if (available > 0)
                {
                    int toRead = Math.Min((int)available, buffer.Length - totalRead);
                    uint read = 0;

                    Native.CheckStatus(Native.Ftd2xx.FT_Read(
                        _expander.Handle,
                        ref buffer[totalRead],
                        (uint)toRead,
                        ref read));

                    totalRead += (int)read;
                }
                else
                {
                    if (Environment.TickCount - startTime > timeoutMs)
                    {
                        return false;
                    }
                    Thread.Sleep(PollIntervalMs);
                }
            }

            return true;
        }

        private void ClearInputBuffer()
        {
            uint available = 0;
            Native.CheckStatus(Native.Ftd2xx.FT_GetQueueStatus(_expander.Handle, ref available));

            if (available > 0)
            {
                var clearBuffer = new byte[available];
                uint read = 0;
                Native.CheckStatus(Native.Ftd2xx.FT_Read(_expander.Handle, ref clearBuffer[0], available, ref read));
            }
        }
    }
}