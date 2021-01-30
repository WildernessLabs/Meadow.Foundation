using System.Threading;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays.Tft
{
    public class St7796s : TftSpiBase
    {
		public override DisplayColorMode DefautColorMode => DisplayColorMode.Format12bppRgb444;

		public St7796s(IIODevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            int width = 320, int height = 480, DisplayColorMode displayColorMode = DisplayColorMode.Format12bppRgb444)
			: base(device, spiBus, chipSelectPin, dcPin, resetPin, width, height, displayColorMode)
        {
            Initialize();

            SetRotation(Rotation.Normal);
        }

        protected override void Initialize()
        {
			Thread.Sleep(120);

			SendCommand(0x01); //Software reset
			Thread.Sleep(120);

			SendCommand(0x11); //Sleep exit                                            
			Thread.Sleep(120);

			SendCommand(0xF0); //Command Set control                                 
			SendData(0xC3);    //Enable extension command 2 partI

			SendCommand(0xF0); //Command Set control                                 
			SendData(0x96);    //Enable extension command 2 partII

			SendCommand(0x36); //Memory Data Access Control MX, MY, RGB mode                                    
			SendData(0x48);    //X-Mirror, Top-Left to right-Buttom, RGB  

			SendCommand(COLOR_MODE);  // set color mode
			if (ColorMode == DisplayColorMode.Format16bppRgb565)
				SendData(0x05);  // 16-bit color RGB565
			else
				SendData(0x03); //12-bit color RGB444

			SendCommand(0xB4); //Column inversion 
			SendData(0x01);    //1-dot inversion

			SendCommand(0xB6); //Display Function Control
			SendData(0x80);    //Bypass
			SendData(0x02);    //Source Output Scan from S1 to S960, Gate Output scan from G1 to G480, scan cycle=2
			SendData(0x3B);    //LCD Drive Line=8*(59+1)

			SendCommand(0xE8); //Display Output Ctrl Adjust
			SendData(0x40);
			SendData(0x8A);
			SendData(0x00);
			SendData(0x00);
			SendData(0x29);    //Source eqaulizing period time= 22.5 us
			SendData(0x19);    //Timing for "Gate start"=25 (Tclk)
			SendData(0xA5);    //Timing for "Gate End"=37 (Tclk), Gate driver EQ function ON
			SendData(0x33);

			SendCommand(0xC1); //Power control2                          
			SendData(0x06);    //VAP(GVDD)=3.85+( vcom+vcom offset), VAN(GVCL)=-3.85+( vcom+vcom offset)

			SendCommand(0xC2); //Power control 3                                      
			SendData(0xA7);    //Source driving current level=low, Gamma driving current level=High

			SendCommand(0xC5); //VCOM Control
			SendData(0x18);    //VCOM=0.9

			Thread.Sleep(120);

			//ST7796 Gamma Sequence
			SendCommand(0xE0); //Gamma"+"                                             
			SendData(0xF0);
			SendData(0x09);
			SendData(0x0b);
			SendData(0x06);
			SendData(0x04);
			SendData(0x15);
			SendData(0x2F);
			SendData(0x54);
			SendData(0x42);
			SendData(0x3C);
			SendData(0x17);
			SendData(0x14);
			SendData(0x18);
			SendData(0x1B);

			SendCommand(0xE1); //Gamma"-"                                             
			SendData(0xE0);
			SendData(0x09);
			SendData(0x0B);
			SendData(0x06);
			SendData(0x04);
			SendData(0x03);
			SendData(0x2B);
			SendData(0x43);
			SendData(0x42);
			SendData(0x3B);
			SendData(0x16);
			SendData(0x14);
			SendData(0x17);
			SendData(0x1B);

			Thread.Sleep(120);

			SendCommand(0xF0); //Command Set control                                 
			SendData(0x3C);    //Disable extension command 2 partI

			SendCommand(0xF0); //Command Set control                                 
			SendData(0x69);    //Disable extension command 2 partII

			//end_tft_write();
			Thread.Sleep(120);
			//begin_tft_write();

			SendCommand(0x29); //Display on
		}

		protected override void SetAddressWindow(int x0, int y0, int x1, int y1)
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

			SendCommand((byte)LcdCommand.RAMWR);  // write to RAM
		}

		public void SetRotation(Rotation rotation)
        {
            SendCommand(MADCTL);

            switch (rotation)
            {
                case Rotation.Normal:
                    SendData(MADCTL_MX | MADCTL_BGR);
                    break;
                case Rotation.Rotate_90:
                    SendData(MADCTL_MV | MADCTL_BGR);
                    break;
                case Rotation.Rotate_180:
                    SendData(MADCTL_BGR | MADCTL_MY);
                    break;
                case Rotation.Rotate_270:
                    SendData(MADCTL_BGR | MADCTL_MV | MADCTL_MX | MADCTL_MY);
                    break;
            }
        }
    }
}