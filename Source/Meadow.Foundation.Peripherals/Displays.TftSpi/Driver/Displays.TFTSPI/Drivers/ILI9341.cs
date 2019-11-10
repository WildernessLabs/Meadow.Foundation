using System.Threading;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays.Tft
{
    public class ILI9341 : DisplayTftSpiBase
    {
        private ILI9341() { }

        public ILI9341(IIODevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            uint width, uint height) : base(device, spiBus, chipSelectPin, dcPin, resetPin, width, height)
        {
            Initialize();
        }

        protected override void Initialize()
        {
            resetPort.State = (true);
            Thread.Sleep(50);
            resetPort.State = (false);
            Thread.Sleep(50);
            resetPort.State = (true);
            Thread.Sleep(50);

            SendCommand(0xEF, new byte[] { 0x03, 0x80, 0x02 });
            SendCommand(0xCF, new byte[] { 0x00, 0xC1, 0x30 });
            SendCommand(0xED, new byte[] { 0x64, 0x03, 0x12, 0x81 });
            SendCommand(0xe8, new byte[] { 0x85, 0x00, 0x78 });

            SendCommand(0xCB, new byte[] { 0x39, 0x2C, 0x00, 0x34, 0x02 });
            SendCommand(0xF7, new byte[] { 0x20 });
            SendCommand(0xEA, new byte[] { 0x00, 0x00 });
            SendCommand(ILI9341_PWCTR1, new byte[] { 0x23 });
            SendCommand(ILI9341_PWCTR2, new byte[] { 0x10 });
            SendCommand(ILI9341_VMCTR1, new byte[] { 0x3e, 0x28 });
            SendCommand(ILI9341_VMCTR2, new byte[] { 0x86 });
            SendCommand(ILI9341_MADCTL, new byte[] { (byte)(ILI9341_MADCTL_MX | ILI9341_MADCTL_BGR) }); //13
            SendCommand(ILI9341_PIXFMT, new byte[] { 0x55 });
            SendCommand(ILI9341_FRMCTR1, new byte[] { 0x00, 0x18 });
            SendCommand(ILI9341_DFUNCTR, new byte[] { 0x08, 0x82, 0x27 });
            SendCommand(0xF2, new byte[] { 0x00 });
            SendCommand(ILI9341_GAMMASET, new byte[] { 0x01 });
            SendCommand(ILI9341_GMCTRP1, new byte[] { 0x0F, 0x31, 0x2B, 0x0C, 0x0E, 0x08, 0x4E, 0xF1, 0x37, 0x07, 0x10, 0x03, 0x0E, 0x09, 0x00 });
            SendCommand(ILI9341_GMCTRN1, new byte[] { 0x00, 0x0E, 0x14, 0x03, 0x11, 0x07, 0x31, 0xC1, 0x48, 0x08, 0x0F, 0x0C, 0x31, 0x36, 0x0F });
            SendCommand(ILI9341_SLPOUT, null);
            Thread.Sleep(120);
            SendCommand(ILI9341_DISPON, null);

            SetAddressWindow(0, 0, width - 1,  height - 1);

            dataCommandPort.State = (Data);
        }

        protected override void SetAddressWindow(uint x0, uint y0, uint x1, uint y1)
        {
            SendCommand((byte)LcdCommand.CASET);  // column addr set
            dataCommandPort.State = Data;
            Write((byte)(x0 >> 8));
            Write((byte)(x0 & 0xff));   // XSTART 
            Write((byte)(x1 >> 8));
            Write((byte)(x1 & 0xff));   // XEND

            SendCommand((byte)LcdCommand.RASET);  // row addr set
            dataCommandPort.State = Data;
            Write((byte)(y0 >> 8));
            Write((byte)(y0 & 0xff));    // YSTART
            Write((byte)(y1 >> 8));
            Write((byte)(y1 & 0xff));    // YEND

            dataCommandPort.State = (Command);
            Write((byte)LcdCommand.RAMWR);  // write to RAM */
        }

        void SendCommand(byte command, byte[] data)
        {
            dataCommandPort.State = (Command);
            Write(command);

            if (data != null)
            {
                dataCommandPort.State = (Data);
                Write(data);
            }
        }

        //static byte ILI9341_SLPIN      = 0x10;
        static byte ILI9341_SLPOUT = 0x11;
        //static byte ILI9341_PTLON      = 0x12;
        //static byte ILI9341_NORON      = 0x13;
        //static byte ILI9341_RDMODE     = 0x0A;
        //static byte ILI9341_RDMADCTL   = 0x0B;
        //static byte ILI9341_RDPIXFMT   = 0x0C;
        //static byte ILI9341_RDIMGFMT   = 0x0A;
        //static byte ILI9341_RDSELFDIAG = 0x0F;
        //static byte ILI9341_INVOFF     = 0x20;
        //static byte ILI9341_INVON      = 0x21;
        static byte ILI9341_GAMMASET = 0x26;
        //static byte ILI9341_DISPOFF    = 0x28;
        static byte ILI9341_DISPON = 0x29;
        //static byte ILI9341_CASET      = 0x2A;
        //static byte ILI9341_PASET      = 0x2B;
        //static byte ILI9341_RAMWR      = 0x2C;
        //static byte ILI9341_RAMRD      = 0x2E;
        //static byte ILI9341_PTLAR      = 0x30;
        //static byte ILI9341_VSCRDEF    = 0x33;
        static byte ILI9341_MADCTL = 0x36;
        //static byte ILI9341_VSCRSADD   = 0x37;
        static byte ILI9341_PIXFMT = 0x3A;

        static byte ILI9341_FRMCTR1 = 0xB1;
        //static byte ILI9341_FRMCTR2 = 0xB2;
        //static byte ILI9341_FRMCTR3 = 0xB3;
        //static byte ILI9341_INVCTR =  0xB4;
        static byte ILI9341_DFUNCTR = 0xB6;

        static byte ILI9341_PWCTR1 = 0xC0;
        static byte ILI9341_PWCTR2 = 0xC1;
        //static byte ILI9341_PWCTR3 = 0xC2;
        //static byte ILI9341_PWCTR4 = 0xC3;
        //static byte ILI9341_PWCTR5 = 0xC4;
        static byte ILI9341_VMCTR1 = 0xC5;
        static byte ILI9341_VMCTR2 = 0xC7;

        static byte ILI9341_GMCTRP1 = 0xE0;
        static byte ILI9341_GMCTRN1 = 0xE1;

        static byte ILI9341_MADCTL_MX = 0x40;
        //static byte ILI9341_MADCTL_MY  = 0x80;
        //static byte ILI9341_MADCTL_MV  = 0x20;
        //static byte ILI9341_MADCTL_ML  = 0x10;
        //static byte ILI9341_MADCTL_RGB = 0x00;
        static byte ILI9341_MADCTL_BGR = 0x08;
        //static byte ILI9341_MADCTL_MH  = 0x04;
    }
}