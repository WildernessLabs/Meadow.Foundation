using Meadow.Hardware;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents an WaveShare Epd7in5 v2 ePaper display
    /// 800x480, 7.5inch e-Ink display, SPI interface 
    /// </summary>
    public class Epd7in5V2 : EPaperMonoBase
    {
        /// <summary>
        /// Create a new WaveShare Epd7in5 v2 800x480 pixel display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="busyPin">Busy pin</param>
        public Epd7in5V2(ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, IPin busyPin) :
            base(spiBus, chipSelectPin, dcPin, resetPin, busyPin, 800, 480)
        { }

        /// <summary>
        /// Create a new WaveShare Epd7in5 v2 ePaper 800x480 pixel display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="busyPort">Busy input port</param>
        public Epd7in5V2(ISpiBus spiBus,
            IDigitalOutputPort chipSelectPort,
            IDigitalOutputPort dataCommandPort,
            IDigitalOutputPort resetPort,
            IDigitalInputPort busyPort) :
            base(spiBus, chipSelectPort, dataCommandPort, resetPort, busyPort, 800, 480)
        { }

        /// <summary>
        /// Initialize the display driver
        /// </summary>
        protected override void Initialize()
        {
            Reset();

            SendCommand(POWER_SETTING);
            SendData(0x07);
            SendData(0x07);    //VGH=20V,VGL=-20V
            SendData(0x3f);     //VDH=15V
            SendData(0x3f);     //VDL=-15V

            //Enhanced display drive(Add 0x06 command)
            SendCommand(BOOSTER_SOFT_START);
            SendData(0x17);
            SendData(0x17);
            SendData(0x28);
            SendData(0x17);

            SendCommand(POWER_ON);
            DelayMs(100);
            WaitUntilIdle();

            SendCommand(PANEL_SETTING);
            SendData(0x1F);   //KW-3f   KWR-2F	BWROTP 0f	BWOTP 1f

            SendCommand(RESOLUTION_SETTING);
            SendData(0x03);     //source 800
            SendData(0x20);
            SendData(0x01);     //gate 480
            SendData(0xE0);

            SendCommand(0x15);
            SendData(0x00);

            // If the screen appears gray, use the annotated initialization command
            SendCommand(VCOM_AND_DATA_INTERVAL_SETTING);
            SendData(0x10);
            SendData(0x07);
            // SendCommand(VCOM_AND_DATA_INTERVAL_SETTING);			
            // SendData(0x10);
            // SendData(0x17);
            // SendCommand(0x52);			
            // SendData(0x03);

            SendCommand(TCON_SETTING);
            SendData(0x22);
        }

        //for 4-shade grayscale ... not supported yet
        void InitializeGrey()
        {
            Reset();

            SendCommand(PANEL_SETTING);
            SendData(0x1F);   //KW-3f   KWR-2F	BWROTP 0f	BWOTP 1f

            SendCommand(VCOM_AND_DATA_INTERVAL_SETTING);
            SendData(0x10);
            SendData(0x07);

            SendCommand(POWER_ON);
            DelayMs(100);
            WaitUntilIdle();        //waiting for the electronic paper IC to release the idle signal

            SendCommand(0x06);          //Booster Soft Start 
            SendData(0x27);
            SendData(0x27);
            SendData(0x18);
            SendData(0x17);

            SendCommand(0xE0);
            SendData(0x02);
            SendCommand(0xE5);
            SendData(0x5F);
        }

        /// <summary>
        /// Reset the display
        /// </summary>
        protected override void Reset()
        {
            if (resetPort != null)
            {
                resetPort.State = true;
                DelayMs(20);
                resetPort.State = false;
                DelayMs(2);
                resetPort.State = true;
                DelayMs(20);
            }
        }

        /// <summary>
        /// Set partial address window to update display
        /// </summary>
        /// <param name="x">X start position in pixels</param>
        /// <param name="y">Y start position in pixels</param>
        /// <param name="width">Width in pixels</param>
        /// <param name="height">Height in pixels</param>
        protected void SetPartialWindow(int x, int y, int width, int height)
        {
            SendCommand(VCOM_AND_DATA_INTERVAL_SETTING);
            SendData(0xA9);
            SendData(0x07);

            SendCommand(PARTIAL_IN);
            SendCommand(PARTIAL_WINDOW);
            SendData(x / 256);
            SendData(x % 256); //x start    

            SendData(x + width / 256);
            SendData(x + width % 256 - 1); //x end	

            SendData(y / 256);
            SendData(y % 256); //y start    

            SendData(y + height / 256);
            SendData(y + height % 256 - 1); //y end
            SendData(0x01);
        }

        /// <summary>
        /// Copy the display buffer to the display for a set region
        /// </summary>
        /// <param name="left">left bounds of region in pixels</param>
        /// <param name="top">top bounds of region in pixels</param>
        /// <param name="right">right bounds of region in pixels</param>
        /// <param name="bottom">bottom bounds of region in pixels</param>
        public override void Show(int left, int top, int right, int bottom)
        {
            int width = right - left;
            int height = top - bottom;

            SetPartialWindow(left, top, width, height);



            DisplayFrame();
        }

        /// <summary>
        /// Copy the display buffer to the display
        /// </summary>
        public override void Show()
        {
            DisplayFrame(imageBuffer.Buffer);
        }

        /// <summary>
        /// Clear the frame data from the SRAM, this doesn't update the display 
        /// </summary>
        protected virtual void ClearFrame()
        {
            SendCommand(DATA_START_TRANSMISSION_1);
            DelayMs(2);
            for (int i = 0; i < Width / 8 * Height; i++)
            {
                SendData(0xFF);
            }
            DelayMs(2);
            SendCommand(DATA_START_TRANSMISSION_2);
            DelayMs(2);
            for (int i = 0; i < Width / 8 * Height; i++)
            {
                SendData(0xFF);
            }
            DelayMs(2);
        }

        void DisplayFrame(byte[] buffer)
        {
            if (buffer == null)
            {
                return;
            }

            SendCommand(DATA_START_TRANSMISSION_1);
            for (int i = 0; i < Width / 8 * Height; i++)
            {
                SendData(buffer[i]);
            }

            SendCommand(DATA_START_TRANSMISSION_2);
            for (int i = 0; i < Width / 8 * Height; i++)
            {
                SendData(~buffer[i]);
            }

            DisplayFrame();
        }

        /// <summary>
        /// Send a refresh command to the display 
        /// Does not transfer new data
        /// </summary>
        public override void DisplayFrame()
        {
            SendCommand(DISPLAY_REFRESH);
            DelayMs(100);
            WaitUntilIdle();
        }

        /// <summary>
        /// Set the device to low power mode
        /// </summary>
        protected override void Sleep()
        {
            SendCommand(VCOM_AND_DATA_INTERVAL_SETTING);
            SendData(0XF7);                     //border floating    
            SendCommand(POWER_OFF);          //power off
            WaitUntilIdle();
            SendCommand(DEEP_SLEEP);         //deep sleep
            SendData(0xA5);
        }

        readonly byte PANEL_SETTING = 0x00;
        readonly byte POWER_SETTING = 0x01;
        readonly byte POWER_OFF = 0x02;
        readonly byte POWER_ON = 0x04;
        readonly byte BOOSTER_SOFT_START = 0x06;
        readonly byte DEEP_SLEEP = 0x07;
        readonly byte DATA_START_TRANSMISSION_1 = 0x10;
        readonly byte DISPLAY_REFRESH = 0x12;
        readonly byte DATA_START_TRANSMISSION_2 = 0x13;
        readonly byte VCOM_AND_DATA_INTERVAL_SETTING = 0x50;
        readonly byte TCON_SETTING = 0x60;
        readonly byte RESOLUTION_SETTING = 0x61;
        readonly byte VCM_DC_SETTING = 0x82;
        readonly byte PARTIAL_WINDOW = 0x90;
        readonly byte PARTIAL_IN = 0x91;
        readonly byte PARTIAL_OUT = 0x92;
    }
}