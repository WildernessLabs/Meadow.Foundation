using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Diagnostics;
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
        private FtdiExpander _expander;
        private SpiClockConfiguration _configuration;
        private const int DefaultTimeoutMs = 1000;  // 1 second default timeout
        private const int PollIntervalMs = 1;       // Poll interval for data checks

        /// <inheritdoc/>
        public override Frequency[] SupportedSpeeds =>
            new Frequency[]
            {
                1000000.Hertz()
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
            // Reset the MPSSE
            _expander.Write(new byte[] { (byte)Native.FT_OPCODE.DisconnectTDItoTDOforLoopback });
            Thread.Sleep(50);

            // Clear any pending data
            ClearInputBuffer();

            // Synchronize the MPSSE interface
            _expander.Write(new byte[] { 0xAA });
            Span<byte> response = stackalloc byte[2];
            _expander.ReadInto(response);

            if (response[0] != 0xFA || response[1] != 0xAA)
            {
                throw new IOException($"MPSSE sync failed. Got {response[0]:X2} {response[1]:X2}");
            }

            _expander.Write(new byte[] { 0xAB });
            _expander.ReadInto(response);
            if (response[0] != 0xFA || response[1] != 0xAB)
            {
                throw new IOException($"MPSSE sync failed. Got {response[0]:X2} {response[1]:X2}");
            }

            // Now configure for SPI operation
            Span<byte> config = stackalloc byte[13];
            int idx = 0;

            // Disable clock divide by 5 for 60Mhz master clock
            config[idx++] = (byte)Native.FT_OPCODE.DisableClockDivideBy5;

            // Turn off adaptive clocking
            config[idx++] = (byte)Native.FT_OPCODE.TurnOffAdaptiveClocking;

            // Disable 3 phase data clocking
            config[idx++] = (byte)Native.FT_OPCODE.Disable3PhaseDataClocking;

            // Set clock divisor for desired frequency
            config[idx++] = (byte)Native.FT_OPCODE.SetClockDivisor;
            uint clockDivisor = (uint)(60000 / (_configuration.Speed.Kilohertz * 2)) - 1;
            config[idx++] = (byte)(clockDivisor & 0x00FF);
            config[idx++] = (byte)((clockDivisor >> 8) & 0x00FF);

            // Set initial pin states and directions
            config[idx++] = (byte)Native.FT_OPCODE.SetDataBitsLowByte;
            byte initialState = (_configuration.SpiMode == SpiClockConfiguration.Mode.Mode2 ||
                              _configuration.SpiMode == SpiClockConfiguration.Mode.Mode3) ? (byte)0x01 : (byte)0x00;

            _expander.GpioStateLow = initialState;
            _expander.GpioDirectionLow = 0x0B; // SCK, MOSI, CS as outputs, MISO as input

            config[idx++] = _expander.GpioStateLow;
            config[idx++] = _expander.GpioDirectionLow;

            _expander.Write(config.Slice(0, idx));

            // Verify configuration
            _expander.Write(new byte[] { (byte)Native.FT_OPCODE.ReadDataBitsLowByte });
            Span<byte> pinState = stackalloc byte[1];
            _expander.ReadInto(pinState);
        }

        private uint GetAvailableBytes(int timeoutMs = DefaultTimeoutMs)
        {
            var sw = Stopwatch.StartNew();
            uint available = 0;

            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                Native.CheckStatus(
                    Native.Ftd2xx.FT_GetQueueStatus(_expander.Handle, ref available));

                if (available > 0)
                {
                    return available;
                }

                // Simple fixed delay instead of exponential backoff
                Thread.Sleep(PollIntervalMs);
            }

            throw new TimeoutException($"No data available after {timeoutMs}ms");
        }

        /// <inheritdoc/>
        public override void Exchange(IDigitalOutputPort? chipSelect, Span<byte> writeBuffer, Span<byte> readBuffer, ChipSelectMode csMode = ChipSelectMode.ActiveLow)
        {
            if (writeBuffer.Length != readBuffer.Length)
            {
                throw new ArgumentException("Write and read buffers must be the same length for full-duplex operation");
            }

            if (chipSelect != null)
            {
                chipSelect.State = csMode == ChipSelectMode.ActiveLow ? false : true;
            }

            try
            {
                // Verify GPIO state before transfer
                _expander.Write(new byte[] { (byte)Native.FT_OPCODE.ReadDataBitsLowByte });
                Span<byte> currentState = stackalloc byte[1];
                _expander.ReadInto(currentState);

                // Clear any existing data
                ClearInputBuffer();

                // Build and send command for combined write/read
                Span<byte> commandBuffer = stackalloc byte[3 + writeBuffer.Length];

                // Use combined read/write command
                commandBuffer[0] = (byte)Native.FT_OPCODE.ClockDataBytesOutOnMinusBytesInOnPlusVeClockMSBFirst;
                commandBuffer[1] = (byte)((writeBuffer.Length - 1) & 0xFF);
                commandBuffer[2] = (byte)(((writeBuffer.Length - 1) >> 8) & 0xFF);
                writeBuffer.CopyTo(commandBuffer.Slice(3));

                // Send complete command
                _expander.Write(commandBuffer);

                // Read response with timeout
                var sw = Stopwatch.StartNew();
                int bytesRead = 0;

                while (bytesRead < readBuffer.Length && sw.ElapsedMilliseconds < 1000)
                {
                    uint available = 0;
                    Native.CheckStatus(Native.Ftd2xx.FT_GetQueueStatus(_expander.Handle, ref available));

                    if (available > 0)
                    {
                        int toRead = Math.Min((int)available, readBuffer.Length - bytesRead);
                        _expander.ReadInto(readBuffer.Slice(bytesRead, toRead));
                        bytesRead += toRead;
                    }
                    else
                    {
                        Thread.Sleep(1);
                    }
                }

                if (bytesRead < readBuffer.Length)
                {
                    throw new TimeoutException(
                        $"Timeout waiting for data. Expected {readBuffer.Length} bytes, got {bytesRead} bytes");
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

        private void ClearInputBuffer()
        {
            uint available = 0;
            Native.CheckStatus(Native.Ftd2xx.FT_GetQueueStatus(_expander.Handle, ref available));

            if (available > 0)
            {
                Span<byte> clearBuffer = stackalloc byte[(int)available];
                _expander.ReadInto(clearBuffer);
            }
        }

        public override void Write(IDigitalOutputPort? chipSelect, Span<byte> writeBuffer, ChipSelectMode csMode = ChipSelectMode.ActiveLow)
        {
            if (writeBuffer.Length > 65536)
            {
                throw new ArgumentException("Buffer too large, maximum size is 65536 bytes");
            }

            if (chipSelect != null)
            {
                chipSelect.State = csMode == ChipSelectMode.ActiveLow ? false : true;
            }

            try
            {
                Span<byte> commandBuffer = stackalloc byte[3 + writeBuffer.Length];
                int idx = 0;

                // Select appropriate command based on SPI mode
                byte command = _configuration.SpiMode switch
                {
                    SpiClockConfiguration.Mode.Mode0 => (byte)Native.FT_OPCODE.ClockDataBytesOutOnMinusVeClockMSBFirst,
                    SpiClockConfiguration.Mode.Mode1 => (byte)Native.FT_OPCODE.ClockDataBytesOutOnPlusVeClockMSBFirst,
                    SpiClockConfiguration.Mode.Mode2 => (byte)Native.FT_OPCODE.ClockDataBytesOutOnPlusVeClockMSBFirst,
                    SpiClockConfiguration.Mode.Mode3 => (byte)Native.FT_OPCODE.ClockDataBytesOutOnMinusVeClockMSBFirst,
                    _ => throw new ArgumentException("Invalid SPI mode")
                };

                commandBuffer[idx++] = command;
                commandBuffer[idx++] = (byte)((writeBuffer.Length - 1) & 0xFF);
                commandBuffer[idx++] = (byte)(((writeBuffer.Length - 1) >> 8) & 0xFF);

                writeBuffer.CopyTo(commandBuffer.Slice(idx));
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
            if (readBuffer.Length > 65536)
            {
                throw new ArgumentException("Buffer too large, maximum size is 65536 bytes");
            }

            if (chipSelect != null)
            {
                chipSelect.State = csMode == ChipSelectMode.ActiveLow ? false : true;
            }

            try
            {
                Span<byte> commandBuffer = stackalloc byte[4];  // Command + length (2) + SendImmediate
                int idx = 0;

                // Select appropriate command based on SPI mode
                byte command = _configuration.SpiMode switch
                {
                    SpiClockConfiguration.Mode.Mode0 => (byte)Native.FT_OPCODE.ClockDataBytesInOnPlusVeClockMSBFirst,
                    SpiClockConfiguration.Mode.Mode1 => (byte)Native.FT_OPCODE.ClockDataBytesInOnMinusVeClockMSBFirst,
                    SpiClockConfiguration.Mode.Mode2 => (byte)Native.FT_OPCODE.ClockDataBytesInOnMinusVeClockMSBFirst,
                    SpiClockConfiguration.Mode.Mode3 => (byte)Native.FT_OPCODE.ClockDataBytesInOnPlusVeClockMSBFirst,
                    _ => throw new ArgumentException("Invalid SPI mode")
                };

                commandBuffer[idx++] = command;
                commandBuffer[idx++] = (byte)((readBuffer.Length - 1) & 0xFF);
                commandBuffer[idx++] = (byte)(((readBuffer.Length - 1) >> 8) & 0xFF);
                commandBuffer[idx++] = (byte)Native.FT_OPCODE.SendImmediate;

                _expander.Write(commandBuffer);
                _expander.ReadInto(readBuffer);
            }
            finally
            {
                if (chipSelect != null)
                {
                    chipSelect.State = csMode == ChipSelectMode.ActiveLow ? true : false;
                }
            }
        }
    }
}