using FTD2XX;
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading;

namespace Meadow.Foundation.ICs.IOExpanders;

public abstract partial class FtdiExpander
{
    /// <summary>
    /// Represents an SPI bus using the FT232H
    /// </summary>
    public class Ft232hSpiBus : ISpiBus
    {
        private readonly FTDI _device;
        private SpiClockConfiguration _configuration;

        /// <inheritdoc/>
        public Frequency[] SupportedSpeeds =>
            new Frequency[]
            {
                1000000.Hertz()
            };

        /// <inheritdoc/>
        public SpiClockConfiguration Configuration => _configuration;

        internal Ft232hSpiBus(FTDI device, int channel, SpiClockConfiguration configuration)
        {
            _device = device;
            _configuration = configuration;
            ConfigureMpsse();
        }

        private void ConfigureMpsse()
        {
            _device.ResetDevice().ThrowIfNotOK();
            _device.SetBitMode(0, 0).ThrowIfNotOK(); // reset
            _device.SetBitMode(0, 0x02).ThrowIfNotOK(); // MPSSE 
            _device.SetLatency(16).ThrowIfNotOK();
            _device.SetTimeouts(1000, 1000).ThrowIfNotOK(); // long
            Thread.Sleep(50);

            // Configure the MPSSE for SPI communication (app note FT_000109 section 6)
            byte[] bytes1 = new byte[]
            {
                0x8A, // disable clock divide by 5 for 60Mhz master clock
                0x97, // turn off adaptive clocking
                0x8d // disable 3 phase data clock
            };
            _device.Write(bytes1).ThrowIfNotOK();

            // The SK clock frequency can be worked out by below algorithm with divide by 5 set as off
            // SCL Frequency (MHz) = 60 / ((1 + DIVISOR) * 2)
            var clockDivisor = 29; // for 1 MHz

            // increase clock divisor to slow down signaling
            var slowDownFactor = 1;
            clockDivisor *= slowDownFactor;

            // set the SCK and MOSI as outputs, with a value of low
            byte[] bytes2 = new byte[]
            {
                0x80, // Set directions of lower 8 pins
                0b00000000, // all low
                0x00000011, // MOSI and SCK output, others input
                0x86, // use clock divisor
                (byte)(clockDivisor & 0xFF), // clock divisor low byte
                (byte)(clockDivisor >> 8), // clock divisor high byte
            };
            _device.Write(bytes2).ThrowIfNotOK();
            Thread.Sleep(50);

            // disable loopback
            _device.Write(new byte[] { 0x85 }).ThrowIfNotOK();
            Thread.Sleep(50);

            //_expander.SetGpioDirectionAndState(true, _expander.GpioDirectionLow |= 0x03, _expander.GpioStateLow);
        }

        /// <inheritdoc/>
        public void Exchange(IDigitalOutputPort? chipSelect, Span<byte> writeBuffer, Span<byte> readBuffer, ChipSelectMode csMode = ChipSelectMode.ActiveLow)
        {
            if (writeBuffer.Length > 65535)
            {
                throw new ArgumentException("Buffer too large, maximum size if 65535");
            }

            if (chipSelect != null)
            {
                chipSelect.State = csMode == ChipSelectMode.ActiveLow ? false : true;
            }

            var shiftOut = new byte[3 + writeBuffer.Length];
            shiftOut[0] = _configuration.SpiMode switch
            {
                SpiClockConfiguration.Mode.Mode0 => 0x31,
                SpiClockConfiguration.Mode.Mode3 => 0x31,
                _ => 0x34
            };
            shiftOut[1] = (byte)writeBuffer.Length;
            shiftOut[2] = (byte)(writeBuffer.Length >> 8);
            writeBuffer.CopyTo(shiftOut[3..]);

            _device.FlushBuffer();
            _device.Write(shiftOut).ThrowIfNotOK();

            byte[] rx = new byte[readBuffer.Length];
            int bytesRead = 0;
            _device.Read(rx, writeBuffer.Length, ref bytesRead).ThrowIfNotOK();
            rx.CopyTo(readBuffer);

            if (chipSelect != null)
            {
                chipSelect.State = csMode == ChipSelectMode.ActiveLow ? true : false;
            }
        }

        /// <inheritdoc/>
        public void Read(IDigitalOutputPort? chipSelect, Span<byte> readBuffer, ChipSelectMode csMode = ChipSelectMode.ActiveLow)
        {
            if (chipSelect != null)
            {
                chipSelect.State = csMode == ChipSelectMode.ActiveLow ? false : true;
            }

            byte[] shiftOut = new byte[3];
            shiftOut[0] = _configuration.SpiMode switch
            {
                SpiClockConfiguration.Mode.Mode0 => 0x20,
                SpiClockConfiguration.Mode.Mode3 => 0x20,
                _ => 0x24
            };
            shiftOut[1] = (byte)readBuffer.Length;
            shiftOut[2] = (byte)(readBuffer.Length >> 8);

            _device.Write(shiftOut).ThrowIfNotOK();

            int bytesRead = 0;
            var read = new byte[readBuffer.Length];
            _device.Read(read, readBuffer.Length, ref bytesRead).ThrowIfNotOK();
            read.CopyTo(readBuffer);

            if (chipSelect != null)
            {
                chipSelect.State = csMode == ChipSelectMode.ActiveLow ? true : false;
            }
        }

        /// <inheritdoc/>
        public void Write(IDigitalOutputPort? chipSelect, Span<byte> writeBuffer, ChipSelectMode csMode = ChipSelectMode.ActiveLow)
        {
            if (writeBuffer.Length > 65535)
            {
                throw new ArgumentException("Buffer too large, maximum size if 65535");
            }

            byte clock;
            switch (_configuration.SpiMode)
            {
                default:
                case SpiClockConfiguration.Mode.Mode3:
                case SpiClockConfiguration.Mode.Mode0:
                    clock = (byte)Native.FT_OPCODE.ClockDataBytesOutOnMinusVeClockMSBFirst;
                    break;
                case SpiClockConfiguration.Mode.Mode2:
                case SpiClockConfiguration.Mode.Mode1:
                    clock = (byte)Native.FT_OPCODE.ClockDataBytesOutOnPlusVeClockMSBFirst;
                    break;
            }

            if (chipSelect != null)
            {
                chipSelect.State = csMode == ChipSelectMode.ActiveLow ? false : true;
            }

            int idx = 0;
            Span<byte> toSend = stackalloc byte[3 + writeBuffer.Length];
            toSend[idx++] = clock;
            toSend[idx++] = (byte)((writeBuffer.Length - 1) & 0xff); // LSB of length to write 
            toSend[idx++] = (byte)((writeBuffer.Length - 1) >> 8); ; // MSB of length to write
            writeBuffer.CopyTo(toSend[3..]);
            //            _expander.Write(toSend);

            if (chipSelect != null)
            {
                chipSelect.State = csMode == ChipSelectMode.ActiveLow ? true : false;
            }
        }
    }
}