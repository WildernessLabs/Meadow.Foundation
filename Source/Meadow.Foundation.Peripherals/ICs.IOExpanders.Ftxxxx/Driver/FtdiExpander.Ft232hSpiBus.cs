using Meadow.Hardware;
using Meadow.Units;
using System;

namespace Meadow.Foundation.ICs.IOExpanders;

public abstract partial class FtdiExpander
{
    public class Ft232hSpiBus : SpiBus
    {
        private FtdiExpander _expander;
        private SpiClockConfiguration _configuration;

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
            // Setup the clock and other elements
            Span<byte> toSend = stackalloc byte[5];
            int idx = 0;
            // Disable clock divide by 5 for 60Mhz master clock
            toSend[idx++] = (byte)Native.FT_OPCODE.DisableClockDivideBy5;
            // Turn off adaptive clocking
            toSend[idx++] = (byte)Native.FT_OPCODE.TurnOffAdaptiveClocking;
            // set SPI clock rate
            toSend[idx++] = (byte)Native.FT_OPCODE.SetClockDivisor;
            uint clockDivisor = (uint)(12000 / (_configuration.Speed.Kilohertz * 2)) - 1;
            toSend[idx++] = (byte)(clockDivisor & 0x00FF);
            toSend[idx++] = (byte)((clockDivisor >> 8) & 0x00FF);

            _expander.Write(toSend);

            // make the SCK and SDO lines outputs
            _expander.SetGpioDirectionAndState(true, _expander.GpioDirectionLow |= 0x03, _expander.GpioStateLow);
        }

        /// <inheritdoc/>
        public override void Exchange(IDigitalOutputPort? chipSelect, Span<byte> writeBuffer, Span<byte> readBuffer, ChipSelectMode csMode = ChipSelectMode.ActiveLow)
        {
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
            _expander.Write(toSend);
            _expander.ReadInto(readBuffer);

            if (chipSelect != null)
            {
                chipSelect.State = csMode == ChipSelectMode.ActiveLow ? true : false;
            }
        }

        /// <inheritdoc/>
        public override void Read(IDigitalOutputPort? chipSelect, Span<byte> readBuffer, ChipSelectMode csMode = ChipSelectMode.ActiveLow)
        {
            byte clock;
            switch (_configuration.SpiMode)
            {
                default:
                case SpiClockConfiguration.Mode.Mode3:
                case SpiClockConfiguration.Mode.Mode0:
                    clock = (byte)Native.FT_OPCODE.ClockDataBytesInOnPlusVeClockMSBFirst;
                    break;
                case SpiClockConfiguration.Mode.Mode2:
                case SpiClockConfiguration.Mode.Mode1:
                    clock = (byte)Native.FT_OPCODE.ClockDataBytesInOnMinusVeClockMSBFirst;
                    break;
            }

            if (chipSelect != null)
            {
                chipSelect.State = csMode == ChipSelectMode.ActiveLow ? false : true;
            }

            Span<byte> toSend = stackalloc byte[4];
            var idx = 0;
            toSend[idx++] = clock;
            toSend[idx++] = (byte)((readBuffer.Length - 1) & 0xff); // LSB of length to read 
            toSend[idx++] = (byte)((readBuffer.Length - 1) >> 8); ; // MSB of length to read
            toSend[idx++] = (byte)Native.FT_OPCODE.SendImmediate; // read now
            _expander.Write(toSend);
            var readCount = _expander.ReadInto(readBuffer);

            if (chipSelect != null)
            {
                chipSelect.State = csMode == ChipSelectMode.ActiveLow ? true : false;
            }
        }

        /// <inheritdoc/>
        public override void Write(IDigitalOutputPort? chipSelect, Span<byte> writeBuffer, ChipSelectMode csMode = ChipSelectMode.ActiveLow)
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
            _expander.Write(toSend);

            if (chipSelect != null)
            {
                chipSelect.State = csMode == ChipSelectMode.ActiveLow ? true : false;
            }
        }
    }
}