using Meadow.Hardware;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents an WaveShare Epd4in2 ePaper display
    /// 400x300, 4.2inch e-Ink display, SPI interface 
    /// </summary>
    public class Epd4in2 : EPaperMonoBase
    {
        /// <summary>
        /// Create a new WaveShare Epd4in2 400x300 pixel display object
        /// </summary>
        /// <param name="device">Meadow device</param>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="busyPin">Busy pin</param>
        public Epd4in2(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, IPin busyPin) :
            base(device, spiBus, chipSelectPin, dcPin, resetPin, busyPin, 400, 300)
        { }

        /// <summary>
        /// Create a new WaveShare Epd4in2 ePaper 400x300 pixel display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="busyPort">Busy input port</param>
        public Epd4in2(ISpiBus spiBus,
            IDigitalOutputPort chipSelectPort,
            IDigitalOutputPort dataCommandPort,
            IDigitalOutputPort resetPort,
            IDigitalInputPort busyPort) :
            base(spiBus, chipSelectPort, dataCommandPort, resetPort, busyPort, 400, 300)
        { }

        /// <summary>
        /// Initialize the display driver
        /// </summary>
        protected override void Initialize()
        {
            Reset();

            SendCommand(0x01);
            SendData(0x03);  // VDS_EN, VDG_EN
            SendData(0x00);  // VCOM_HV, VGHL_LV[1], VGHL_LV[0]
            SendData(0x2b);  // VDH
            SendData(0x2b);  // VDL

            SendCommand(0x06);
            SendData(0x17);
            SendData(0x17);
            SendData(0x17);  //07 0f 17 1f 27 2F 37 2f
            SendCommand(0x04);
            WaitUntilIdle();
            SendCommand(0x00);
            SendData(0xbf);  // KW-BF   KWR-AF  BWROTP 0f

            SendCommand(0x30);
            SendData(0x3c);  // 3A 100HZ   29 150Hz 39 200HZ  31 171HZ

            SendCommand(0x61); // resolution setting
            SendData(0x01);    // 400 
            SendData(0x90); 
            SendData(0x01);    // 300
            SendData(0x2c);

            SendCommand(0x82); // vcom_DC setting
            SendData(0x12);

            SendCommand(0X50); // VCOM AND DATA INTERVAL SETTING
            SendData(0x97);    // 97 white border 77 black border    VBDF 17|D7 VBDW 97 VBDB 57    VBDF F7 VBDW 77 VBDB 37  VBDR B7

            SetLookupTable();
        }

        //for 4-shade greyscale ... not supported yet
        void InitializeGrey()
        {
            Reset();
            SendCommand(0x01);  //POWER SETTING
            SendData(0x03);
            SendData(0x00);     //VGH=20V,VGL=-20V
            SendData(0x2b);     //VDH=15V															 
            SendData(0x2b);     //VDL=-15V
            SendData(0x13);

            SendCommand(0x06);  //booster soft start
            SendData(0x17);     //A
            SendData(0x17);     //B
            SendData(0x17);     //C 

            SendCommand(0x04);
            WaitUntilIdle();

            SendCommand(0x00);  //panel setting
            SendData(0x3f);     //KW-3f   KWR-2F	BWROTP 0f	BWOTP 1f

            SendCommand(0x30);  //PLL setting
            SendData(0x3c);     //100hz 

            SendCommand(0x61);  // resolution setting
            SendData(0x01);     //400
            SendData(0x90);
            SendData(0x01);     //300
            SendData(0x2c);

            SendCommand(0x82);  //vcom_DC setting
            SendData(0x12);

            SendCommand(0X50);  //VCOM AND DATA INTERVAL SETTING			
            SendData(0x97);
        }

        void SetLookupTable()
        {
            SendCommand(0x20);                      //vcom
            int count;
            for (count = 0; count < 36; count++)
            {
                SendData(lut_vcom0[count]);
            }

            SendCommand(0x21);                      //ww --
            for (count = 0; count < 36; count++)
            {
                SendData(lut_ww[count]);
            }

            SendCommand(0x22);                      //bw r
            for (count = 0; count < 36; count++)
            {
                SendData(lut_bw[count]);
            }

            SendCommand(0x23);                      //wb w
            for (count = 0; count < 36; count++)
            {
                SendData(lut_bb[count]);
            }

            SendCommand(0x24);                      //bb b
            for (count = 0; count < 36; count++)
            {
                SendData(lut_wb[count]);
            }
        }

        /// <summary>
        /// Reset the display
        /// </summary>
        protected override void Reset()
        {
            resetPort.State = false;
            DelayMs(2);
            resetPort.State = true;
            DelayMs(20);
            resetPort.State = false;
            DelayMs(2);
            resetPort.State = true;
            DelayMs(20);
            resetPort.State = false;
            DelayMs(2);
            resetPort.State = true;
            DelayMs(20);
        }

        /// <summary>
        /// Set partial address window to update display
        /// </summary>
        /// <param name="buffer">The interal display buffer</param>
        /// <param name="x">X start position in pixels</param>
        /// <param name="y">Y start position in pixels</param>
        /// <param name="width">Width in pixels</param>
        /// <param name="height">Height in pixels</param>
        protected void SetPartialWindow(byte[] buffer, int x, int y, int width, int height)
        {
            SendCommand(PARTIAL_IN);
            SendCommand(PARTIAL_WINDOW);
            SendData(x >> 8);
            SendData(x & 0xf8);     // x should be the multiple of 8, the last 3 bit will always be ignored
            SendData(((x & 0xf8) + width - 1) >> 8);
            SendData(((x & 0xf8) + width - 1) | 0x07);
            SendData(y >> 8);
            SendData(y & 0xff);
            SendData((y + height - 1) >> 8);
            SendData((y + height - 1) & 0xff);
            SendData(0x01);         // Gates scan both inside and outside of the partial window. (default) 
            DelayMs(2);
            SendCommand(DATA_START_TRANSMISSION_2);

            if (buffer != null)
            {
                for (int i = 0; i < width / 8 * height; i++)
                {
                    SendData(buffer[i]);
                }
            }
            else
            {
                for (int i = 0; i < width / 8 * height; i++)
                {
                    SendData(0x00);
                }
            }
            DelayMs(2);
            SendCommand(PARTIAL_OUT);
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
            SetPartialWindow(imageBuffer.Buffer, left, top, right - left, top - bottom);

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
            SendCommand(RESOLUTION_SETTING);
            SendData(Width >> 8);
            SendData(Width & 0xff);
            SendData(Height >> 8);
            SendData(Height & 0xff);

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
            SendCommand(RESOLUTION_SETTING);
            SendData(Width >> 8);
            SendData(Width & 0xff);
            SendData(Height >> 8);
            SendData(Height & 0xff);

            SendCommand(VCM_DC_SETTING);
            SendData(0x12);

            SendCommand(VCOM_AND_DATA_INTERVAL_SETTING);
            SendCommand(0x97);    //VBDF 17|D7 VBDW 97  VBDB 57  VBDF F7  VBDW 77  VBDB 37  VBDR B7

            if (buffer != null)
            {
                SendCommand(DATA_START_TRANSMISSION_1);
                for (int i = 0; i < Width / 8 * Height; i++)
                {   // bit set: white, bit reset: black
                    SendData(0xFF);      
                }
                DelayMs(2);
                SendCommand(DATA_START_TRANSMISSION_2);
                for (int i = 0; i < Width / 8 * Height; i++)
                {   //Set this to 0xFF for white when in single color mode
                    SendData(buffer[i]);
                }
            
                DelayMs(2);
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
            SendData(0x17);                       //border floating    
            SendCommand(VCM_DC_SETTING);          //VCOM to 0V
            SendCommand(PANEL_SETTING);
            DelayMs(100);

            SendCommand(POWER_SETTING);           //VG&VS to 0V fast
            SendData(0x00);
            SendData(0x00);
            SendData(0x00);
            SendData(0x00);
            SendData(0x00);
            DelayMs(100);

            SendCommand(POWER_OFF);          //power off
            WaitUntilIdle();
            SendCommand(DEEP_SLEEP);         //deep sleep
            SendData(0xA5);
        }

        readonly byte[] lut_vcom0 = {
              0x00, 0x08, 0x08, 0x00, 0x00, 0x02,
              0x00, 0x0F, 0x0F, 0x00, 0x00, 0x01,
              0x00, 0x08, 0x08, 0x00, 0x00, 0x02,
              0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
              0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
              0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
              0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
              0x00, 0x00,
        };

        readonly byte[] lut_ww = {
              0x50, 0x08, 0x08, 0x00, 0x00, 0x02,
              0x90, 0x0F, 0x0F, 0x00, 0x00, 0x01,
              0xA0, 0x08, 0x08, 0x00, 0x00, 0x02,
              0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
              0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
              0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
              0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        };

        readonly byte[] lut_bw = {
              0x50, 0x08, 0x08, 0x00, 0x00, 0x02,
              0x90, 0x0F, 0x0F, 0x00, 0x00, 0x01,
              0xA0, 0x08, 0x08, 0x00, 0x00, 0x02,
              0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
              0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
              0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
              0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        };

        readonly byte[] lut_bb = {
              0xA0, 0x08, 0x08, 0x00, 0x00, 0x02,
              0x90, 0x0F, 0x0F, 0x00, 0x00, 0x01,
              0x50, 0x08, 0x08, 0x00, 0x00, 0x02,
              0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
              0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
              0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
              0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        };

        readonly byte[] lut_wb = {
              0x20, 0x08, 0x08, 0x00, 0x00, 0x02,
              0x90, 0x0F, 0x0F, 0x00, 0x00, 0x01,
              0x10, 0x08, 0x08, 0x00, 0x00, 0x02,
              0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
              0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
              0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
              0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        };

        readonly byte PANEL_SETTING = 0x00;
        readonly byte POWER_SETTING                              = 0x01;
        readonly byte POWER_OFF                                  = 0x02;
        //readonly byte POWER_OFF_SEQUENCE_SETTING                 = 0x03;
        //readonly byte POWER_ON                                   = 0x04;
        //readonly byte POWER_ON_MEASURE                           = 0x05;
        //readonly byte BOOSTER_SOFT_START                         = 0x06;
        readonly byte DEEP_SLEEP                                 = 0x07;
        readonly byte DATA_START_TRANSMISSION_1                  = 0x10;
        //readonly byte DATA_STOP                                  = 0x11;
        readonly byte DISPLAY_REFRESH                            = 0x12;
        readonly byte DATA_START_TRANSMISSION_2                  = 0x13;
        //readonly byte LUT_FOR_VCOM                               = 0x20; 
        //readonly byte LUT_WHITE_TO_WHITE                         = 0x21;
        //readonly byte LUT_BLACK_TO_WHITE                         = 0x22;
        //readonly byte LUT_WHITE_TO_BLACK                         = 0x23;
        //readonly byte LUT_BLACK_TO_BLACK                         = 0x24;
        //readonly byte PLL_CONTROL                                = 0x30;
        //readonly byte TEMPERATURE_SENSOR_COMMAND                 = 0x40;
        //readonly byte TEMPERATURE_SENSOR_SELECTION               = 0x41;
        //readonly byte TEMPERATURE_SENSOR_WRITE                   = 0x42;
        //readonly byte TEMPERATURE_SENSOR_READ                    = 0x43;
        readonly byte VCOM_AND_DATA_INTERVAL_SETTING             = 0x50;
        //readonly byte LOW_POWER_DETECTION                        = 0x51;
        //readonly byte TCON_SETTING                               = 0x60;
        readonly byte RESOLUTION_SETTING                         = 0x61;
        //readonly byte GSST_SETTING                               = 0x65;
        //readonly byte GET_STATUS                                 = 0x71;
        //readonly byte AUTO_MEASUREMENT_VCOM                      = 0x80;
        //readonly byte READ_VCOM_VALUE                            = 0x81;
        readonly byte VCM_DC_SETTING                             = 0x82;
        readonly byte PARTIAL_WINDOW                             = 0x90;
        readonly byte PARTIAL_IN                                 = 0x91;
        readonly byte PARTIAL_OUT                                = 0x92;
        //readonly byte PROGRAM_MODE                               = 0xA0;
        //readonly byte ACTIVE_PROGRAMMING                         = 0xA1;
        //readonly byte READ_OTP                                   = 0xA2;
        //readonly byte POWER_SAVING                               = 0xE3;
    }
}