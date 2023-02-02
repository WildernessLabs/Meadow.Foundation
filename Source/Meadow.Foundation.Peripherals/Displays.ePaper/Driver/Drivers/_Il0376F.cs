using Meadow.Hardware;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents an Il0376F ePaper color display
    /// 200x200, e-Ink three-color display, SPI interface 
    /// </summary>
    public class Il0376F : EPaperTriColorBase
    {
        /// <summary>
        /// Create a new Il0376F object
        /// </summary>
        /// <param name="device">Meadow device</param>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="busyPin">Busy pin</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        public Il0376F(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, IPin busyPin,
            int width = 200, int height = 200) :
            base(device, spiBus, chipSelectPin, dcPin, resetPin, busyPin, width, height)
        { }

        /// <summary>
        /// Create a new Il0376F ePaper display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="busyPort">Busy input port</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        public Il0376F(ISpiBus spiBus,
            IDigitalOutputPort chipSelectPort,
            IDigitalOutputPort dataCommandPort,
            IDigitalOutputPort resetPort,
            IDigitalInputPort busyPort,
            int width, int height) :
            base(spiBus, chipSelectPort, dataCommandPort, resetPort, busyPort, width, height)
        { }

        /// <summary>
        /// Does the display invert data for black pixels
        /// </summary>
        protected override bool IsBlackInverted => false;

        /// <summary>
        /// Does the display invert data for color pixels
        /// </summary>
        protected override bool IsColorInverted => false;

        /// <summary>
        /// Initalize the display
        /// </summary>
        protected override void Initialize()
        {
            Reset();
            SendCommand(Command.POWER_SETTING);
            SendData(0x07);
            SendData(0x00);
            SendData(0x08);
            SendData(0x00);
            SendCommand(Command.BOOSTER_SOFT_START);
            SendData(0x07);
            SendData(0x07);
            SendData(0x07);
            SendCommand(Command.POWER_ON);

            WaitUntilIdle();

            SendCommand(Command.PANEL_SETTING);
            SendData(0xcf);
            SendCommand(Command.VCOM_AND_DATA_INTERVAL_SETTING);
            SendData(0x17);
            SendCommand(Command.PLL_CONTROL);
            SendData(0x39);
            SendCommand(Command.RESOLUTION_SETTING);
            SendData((byte)(Height >> 8) & 0xFF);
            SendData((byte)(Height & 0xFF));//width 128
            SendData((byte)(Width >> 8) & 0xFF);
            SendData((byte)(Width & 0xFF));
            SendCommand(Command.VCM_DC_SETTING);
            SendData(0x0E);

            SetLutBlack();
            SetLutRed();
        }

        /// <summary>
        /// Display data from the display controller SRAM
        /// </summary>
        protected void DisplayFrame()
        {
            byte temp;
            if (imageBuffer.BlackBuffer != null)
            {
                SendCommand(Command.DATA_START_TRANSMISSION_1);
                DelayMs(2);
                for (int i = 0; i < Width * Height / 8; i++)
                {
                    temp = 0x00;
                    for (int bit = 0; bit < 4; bit++)
                    {
                        if ((imageBuffer.BlackBuffer[i] & (0x80 >> bit)) != 0)
                        {
                            temp |= (byte)(0xC0 >> (bit * 2));
                        }
                    }
                    SendData(temp);
                    temp = 0x00;
                    for (int bit = 4; bit < 8; bit++)
                    {
                        if ((imageBuffer.BlackBuffer[i] & (0x80 >> bit)) != 0)
                        {
                            temp |= (byte)(0xC0 >> ((bit - 4) * 2));
                        }
                    }
                    SendData(temp);
                }
                DelayMs(2);
            }

            if (imageBuffer.ColorBuffer != null)
            {
                SendCommand(Command.DATA_START_TRANSMISSION_2);
                DelayMs(2);
                SendData(imageBuffer.ColorBuffer);
                DelayMs(2);
            }
            SendCommand(Command.DISPLAY_REFRESH);
            WaitUntilIdle();
        }

        /// <summary>
        /// Set the black lookup table (LUT)
        /// </summary>
        void SetLutBlack()
        {
            SendCommand(0x20);         //g vcom
            SendData(lut_vcom0);
            SendCommand(0x21);         //g ww --
            SendData(lut_w);
            SendCommand(0x22);         //g bw r
            SendData(lut_b);
            SendCommand(0x23);         //g wb w
            SendData(lut_g1);
        }

        /// <summary>
        /// Set the red lookup table (LUT)
        /// </summary>
        void SetLutRed()
        {
            SendCommand(0x25);
            SendData(lut_vcom1);
            SendCommand(0x26);
            SendData(lut_red0);
            SendCommand(0x27);
            SendData(lut_red1);
        }

        /// <summary>
        /// Set the display to sleep state
        /// </summary>
        protected void Sleep()
        {
            SendCommand(Command.VCOM_AND_DATA_INTERVAL_SETTING);
            SendData(0x17);
            SendCommand(Command.VCM_DC_SETTING);         //to solve Vcom drop
            SendData(0x00);
            SendCommand(Command.POWER_SETTING);         //power setting
            SendData(0x02);        //gate switch to external
            SendData(0x00);
            SendData(0x00);
            SendData(0x00);
            WaitUntilIdle();
            SendCommand(Command.POWER_OFF);         //power off
        }

        /// <summary>
        /// Update the display from the offscreen buffer
        /// </summary>
        public override void Show()
        {
            DisplayFrame();
        }

        /// <summary>
        /// Update a region of the display from the offscreen buffer
        /// </summary>
        /// <param name="left">Left bounds in pixels</param>
        /// <param name="top">Top bounds in pixels</param>
        /// <param name="right">Right bounds in pixels</param>
        /// <param name="bottom">Bottom bounds in pixels</param>
        public override void Show(int left, int top, int right, int bottom)
        {
            DisplayFrame();
        }

        readonly byte[] lut_vcom0 =
        {
            0x0E, 0x14, 0x01, 0x0A, 0x06, 0x04, 0x0A, 0x0A,
            0x0F, 0x03, 0x03, 0x0C, 0x06, 0x0A, 0x00
        };

        readonly byte[] lut_w =
        {
            0x0E, 0x14, 0x01, 0x0A, 0x46, 0x04, 0x8A, 0x4A,
            0x0F, 0x83, 0x43, 0x0C, 0x86, 0x0A, 0x04
        };

        readonly byte[] lut_b =
        {
            0x0E, 0x14, 0x01, 0x8A, 0x06, 0x04, 0x8A, 0x4A,
            0x0F, 0x83, 0x43, 0x0C, 0x06, 0x4A, 0x04
        };

        readonly byte[] lut_g1 =
        {
            0x8E, 0x94, 0x01, 0x8A, 0x06, 0x04, 0x8A, 0x4A,
            0x0F, 0x83, 0x43, 0x0C, 0x06, 0x0A, 0x04
        };

        /*
        readonly byte[] lut_g2 =
        {
            0x8E, 0x94, 0x01, 0x8A, 0x06, 0x04, 0x8A, 0x4A,
            0x0F, 0x83, 0x43, 0x0C, 0x06, 0x0A, 0x04
        };*/

        readonly byte[] lut_vcom1 =
        {
            0x03, 0x1D, 0x01, 0x01, 0x08, 0x23, 0x37, 0x37,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };

        readonly byte[] lut_red0 =
        {
            0x83, 0x5D, 0x01, 0x81, 0x48, 0x23, 0x77, 0x77,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };

        readonly byte[] lut_red1 =
        {
            0x03, 0x1D, 0x01, 0x01, 0x08, 0x23, 0x37, 0x37,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };
    }
}
