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

        public override Frequency[] SupportedSpeeds => throw new NotImplementedException();

        public override SpiClockConfiguration Configuration => _configuration;

        internal Ft232hSpiBus(FtdiExpander expander)
        {
            _configuration = new SpiClockConfiguration(1000000.Hertz());
            _expander = expander;
        }

        public override void Configure()
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
            uint clockDivisor = (12000 / (_expander.SpiBusFrequencyKbps * 2)) - 1;
            toSend[idx++] = (byte)(clockDivisor & 0x00FF);
            toSend[idx++] = (byte)((clockDivisor >> 8) & 0x00FF);

            _expander.Write(toSend);

            // make the SCK and SDO lines outputs
            _expander.SetGpioDirectionAndState(true, _expander.GpioDirectionLow |= 0x03, _expander.GpioStateLow);
        }

        /*
        # Set SPI clock rate
        def set_spi_clock(d, hz):
            div = int((12000000 / (hz * 2)) - 1)  # Set SPI clock
            ft_write(d, (0x86, div%256, div//256)) 

if dev:
        print("FTDI device opened")
        set_bitmode(dev, OPS, 2)              # Set SPI mode
        set_spi_clock(dev, 1000000)           # Set SPI clock
        ft_write(dev, (0x80, 0, OPS+OE+LE))   # Set outputs
        data = dig_segs[DIG1], dig_segs[DIG2] # Convert digits to segs
        ft_write_cmd_bytes(dev, 0x11, data)   # Write seg bit data
        ft_write(dev, (0x80, LE, OPS+OE+LE))  # Latch = 1
        ft_write(dev, (0x80, OE, OPS+OE+LE))  # Latch = 0, disp = 1
        print("Displaying '%u%u'" % (DIG2, DIG1))
        time.sleep(1)
        ft_write(dev, (0x80, 0,  OPS+OE+LE))  # Latch = disp = 0
        print("Display off")
        dev.close()
        */

        public override void Exchange(IDigitalOutputPort? chipSelect, Span<byte> writeBuffer, Span<byte> readBuffer, ChipSelectMode csMode = ChipSelectMode.ActiveLow)
        {
            throw new NotImplementedException();
        }

        public override void Read(IDigitalOutputPort? chipSelect, Span<byte> readBuffer, ChipSelectMode csMode = ChipSelectMode.ActiveLow)
        {
            if (chipSelect != null)
            {
                chipSelect.State = csMode == ChipSelectMode.ActiveLow ? false : true;
            }

            Span<byte> toSend = stackalloc byte[4];
            var idx = 0;
            toSend[idx++] = (byte)Native.FT_OPCODE.ClockDataBytesInOnMinusVeClockMSBFirst; // clock in on falling edge
            toSend[idx++] = (byte)(readBuffer.Length % 256 - 1); // LSB of length to read
            toSend[idx++] = 0; // MSB of length to read
            toSend[idx++] = (byte)Native.FT_OPCODE.SendImmediate; // read now
            _expander.Write(toSend);
            var readCount = _expander.ReadInto(readBuffer);

            if (chipSelect != null)
            {
                chipSelect.State = csMode == ChipSelectMode.ActiveLow ? true : false;
            }
        }

        public override void Write(IDigitalOutputPort? chipSelect, Span<byte> writeBuffer, ChipSelectMode csMode = ChipSelectMode.ActiveLow)
        {
            if (chipSelect != null)
            {
                chipSelect.State = csMode == ChipSelectMode.ActiveLow ? false : true;
            }

            if (chipSelect != null)
            {
                chipSelect.State = csMode == ChipSelectMode.ActiveLow ? true : false;
            }
        }
    }
}