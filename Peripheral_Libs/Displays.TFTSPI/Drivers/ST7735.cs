using System.Threading;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays
{
    public class ST7735 : DisplayTFTSPIBase
    {
        private ST7735() { }

        public ST7735(Pins chipSelectPin, Pins dcPin, Pins resetPin,
            uint width, uint height,
            SPI.SPI_module spiModule = SPI.SPI_module.SPI1,
            uint speedKHz = 9500) : base(chipSelectPin, dcPin, resetPin, width, height, spiModule, speedKHz)
        { }

        new public enum LcdCommand
        {
            NOP = 0x0,
            SWRESET = 0x01,
            RDDID = 0x04,
            RDDST = 0x09,
            SLPIN = 0x10,
            SLPOUT = 0x11,
            PTLON = 0x12,
            NORON = 0x13,
            INVOFF = 0x20,
            INVON = 0x21,
            DISPOFF = 0x28,
            DISPON = 0x29,
            CASET = 0x2A,
            RASET = 0x2B,
            RAMWR = 0x2C,
            RAMRD = 0x2E,
            COLMOD = 0x3A,
            MADCTL = 0x36,
            FRMCTR1 = 0xB1,
            FRMCTR2 = 0xB2,
            FRMCTR3 = 0xB3,
            INVCTR = 0xB4,
            DISSET5 = 0xB6,
            PWCTR1 = 0xC0,
            PWCTR2 = 0xC1,
            PWCTR3 = 0xC2,
            PWCTR4 = 0xC3,
            PWCTR5 = 0xC4,
            VMCTR1 = 0xC5,
            RDID1 = 0xDA,
            RDID2 = 0xDB,
            RDID3 = 0xDC,
            RDID4 = 0xDD,
            PWCTR6 = 0xFC,
            GMCTRP1 = 0xE0,
            GMCTRN1 = 0xE1
        }

        protected override void Initialize()
        {
            resetPort.State = (true);
            Thread.Sleep(50);
            resetPort.State = (false);
            Thread.Sleep(50);
            resetPort.State = (true);
            Thread.Sleep(50);

            dataCommandPort.State = (Command);
            Write((byte)LcdCommand.SWRESET); // software reset

            dataCommandPort.State = (Command);
            Write((byte)LcdCommand.SLPOUT);  // exit sleep mode

            dataCommandPort.State = (Command);
            Write((byte)LcdCommand.FRMCTR1);  // frame rate control - normal mode
            dataCommandPort.State = (Data);
            Write(0x01);  // frame rate = fosc / (1 x 2 + 40) * (LINE + 2C + 2D)
            Write(0x2C);
            Write(0x2D);

            dataCommandPort.State = (Command);
            Write((byte)LcdCommand.FRMCTR2);  // frame rate control - idle mode
            dataCommandPort.State = (Data);
            Write(0x01);  // frame rate = fosc / (1 x 2 + 40) * (LINE + 2C + 2D)
            Write(0x2C);
            Write(0x2D);

            dataCommandPort.State = (Command);
            Write((byte)LcdCommand.FRMCTR3);  // frame rate control - partial mode
            dataCommandPort.State = (Data);
            Write(0x01); // dot inversion mode
            Write(0x2C);
            Write(0x2D);
            Write(0x01); // line inversion mode
            Write(0x2C);
            Write(0x2D);

            dataCommandPort.State = (Command);
            Write((byte)LcdCommand.INVCTR);  // display inversion control
            dataCommandPort.State = (Data);
            Write(0x07);  // no inversion

            dataCommandPort.State = (Command);
            Write((byte)LcdCommand.PWCTR1);  // power control
            dataCommandPort.State = (Data);
            Write(0xA2);
            Write(0x02);      // -4.6V
            Write(0x84);      // AUTO mode

            dataCommandPort.State = (Command);
            Write((byte)LcdCommand.PWCTR2);  // power control
            dataCommandPort.State = (Data);
            Write(0xC5);      // VGH25 = 2.4C VGSEL = -10 VGH = 3 * AVDD

            dataCommandPort.State = (Command);
            Write((byte)LcdCommand.PWCTR3);  // power control
            dataCommandPort.State = (Data);
            Write(0x0A);      // Opamp current small 
            Write(0x00);      // Boost frequency

            dataCommandPort.State = (Command);
            Write((byte)LcdCommand.PWCTR4);  // power control
            dataCommandPort.State = (Data);
            Write(0x8A);      // BCLK/2, Opamp current small & Medium low
            Write(0x2A);

            Write((byte)LcdCommand.PWCTR5);  // power control
            dataCommandPort.State = (Data);
            Write(0x8A);
            Write(0xEE);

            dataCommandPort.State = (Command);
            Write((byte)LcdCommand.VMCTR1);  // power control
            dataCommandPort.State = (Data);
            Write(0x0E);

            dataCommandPort.State = (Command);
            Write((byte)LcdCommand.INVOFF);    // don't invert display

            dataCommandPort.State = (Command);
            Write((byte)LcdCommand.MADCTL);  // memory access control (directions)
            dataCommandPort.State = (Data);
            Write(0xC8);  // row address/col address, bottom to top refresh

            dataCommandPort.State = (Command);
            Write((byte)LcdCommand.COLMOD);  // set color mode
            dataCommandPort.State = (Data);
            Write(0x05);  // 16-bit color

            dataCommandPort.State = (Command);
            Write((byte)LcdCommand.CASET);  // column addr set
            dataCommandPort.State = (Data);
            Write(0x00);
            Write(0x00);   // XSTART = 0
            Write(0x00);
            Write(0x7F);   // XEND = 127

            dataCommandPort.State = (Command);
            Write((byte)LcdCommand.RASET);  // row addr set
            dataCommandPort.State = (Data);
            Write(0x00);
            Write(0x00);    // XSTART = 0
            Write(0x00);
            Write(0x9F);    // XEND = 159

            dataCommandPort.State = (Command);
            Write((byte)LcdCommand.GMCTRP1);
            dataCommandPort.State = (Data);
            Write(0x02);
            Write(0x1c);
            Write(0x07);
            Write(0x12);
            Write(0x37);
            Write(0x32);
            Write(0x29);
            Write(0x2d);
            Write(0x29);
            Write(0x25);
            Write(0x2B);
            Write(0x39);
            Write(0x00);
            Write(0x01);
            Write(0x03);
            Write(0x10);

            dataCommandPort.State = (Command);
            Write((byte)LcdCommand.GMCTRN1);
            dataCommandPort.State = (Data);
            Write(0x03);
            Write(0x1d);
            Write(0x07);
            Write(0x06);
            Write(0x2E);
            Write(0x2C);
            Write(0x29);
            Write(0x2D);
            Write(0x2E);
            Write(0x2E);
            Write(0x37);
            Write(0x3F);
            Write(0x00);
            Write(0x00);
            Write(0x02);
            Write(0x10);

            dataCommandPort.State = (Command);
            Write((byte)LcdCommand.DISPON);
            Thread.Sleep(50);

            dataCommandPort.State = (Command);
            Write((byte)LcdCommand.NORON);  // normal display on
            Thread.Sleep(10);

            SetAddressWindow(0, 0, (byte)(_width - 1), (byte)(_height - 1));

            dataCommandPort.State = (Data);
        }

        private void SetAddressWindow(byte x0, byte y0, byte x1, byte y1)
        {
            dataCommandPort.State = (Command);
            Write((byte)LcdCommand.CASET);  // column addr set
            dataCommandPort.State = (Data);
            Write(0x00);
            Write(x0);   // XSTART 
            Write(0x00);
            Write(x1);   // XEND

            dataCommandPort.State = (Command);
            Write((byte)LcdCommand.RASET);  // row addr set
            dataCommandPort.State = (Data);
            Write(0x00);
            Write(y0);    // YSTART
            Write(0x00);
            Write(y1);    // YEND

            dataCommandPort.State = (Command);
            Write((byte)LcdCommand.RAMWR);  // write to RAM
        }
    }
}