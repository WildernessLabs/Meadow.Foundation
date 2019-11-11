using Meadow.Hardware;

namespace Meadow.Foundation.Displays.ePaper
{
    //similar to IL91874 ... appears to be an old version v0.3
    //GxGDEW027W3
    //currently hard coded to the avaliable display size 
    public class IL91874V03 : EPDBase
    {
        public IL91874V03(IIODevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, IPin busyPin,
            uint width = 176, uint height = 264) :
            base(device, spiBus, chipSelectPin, dcPin, resetPin, busyPin, width, height)
        { }

        protected override void Initialize()
        {
            Reset();

            SendData(0x00);                  // VCOM_HV, VGHL_LV[1], VGHL_LV[0]
            SendData(0x2b);                  // VDH
            SendData(0x2b);                  // VDL
            SendData(0x09);                  // VDHR
            SendCommand(BOOSTER_SOFT_START);
            SendData(0x07);
            SendData(0x07);
            SendData(0x17);
            // Power optimization
            SendCommand(0xF8);
            SendData(0x60);
            SendData(0xA5);
            // Power optimization
            SendCommand(0xF8);
            SendData(0x89);
            SendData(0xA5);
            // Power optimization
            SendCommand(0xF8);
            SendData(0x90);
            SendData(0x00);
            // Power optimization
            SendCommand(0xF8);
            SendData(0x93);
            SendData(0x2A);
            // Power optimization
            SendCommand(0xF8);
            SendData(0xA0);
            SendData(0xA5);
            // Power optimization
            SendCommand(0xF8);
            SendData(0xA1);
            SendData(0x00);
            // Power optimization
            SendCommand(0xF8);
            SendData(0x73);
            SendData(0x41);
            SendCommand(PARTIAL_DISPLAY_REFRESH);
            SendData(0x00);
            SendCommand(POWER_ON);
            WaitUntilIdle();

            SendCommand(PANEL_SETTING);
            SendData(0xAF);        //KW-BF   KWR-AF    BWROTP 0f
            SendCommand(PLL_CONTROL);
            SendData(0x3A);       //3A 100HZ   29 150Hz 39 200HZ    31 171HZ
            SendCommand(VCM_DC_SETTING_REGISTER);
            SendData(0x12);
            DelayMs(2);
            SetLookupTable();
        }

        void SetLookupTable()
        {
            int count;
            SendCommand(LUT_FOR_VCOM);                            //vcom
            for (count = 0; count < 44; count++)
            {
                SendData(lut_vcom_dc[count]);
            }

            SendCommand(LUT_WHITE_TO_WHITE);                      //ww --
            for (count = 0; count < 42; count++)
            {
                SendData(lut_ww[count]);
            }

            SendCommand(LUT_BLACK_TO_WHITE);                      //bw r
            for (count = 0; count < 42; count++)
            {
                SendData(lut_bw[count]);
            }

            SendCommand(LUT_WHITE_TO_BLACK);                      //wb w
            for (count = 0; count < 42; count++)
            {
                SendData(lut_bb[count]);
            }

            SendCommand(LUT_BLACK_TO_BLACK);                      //bb b
            for (count = 0; count < 42; count++)
            {
                SendData(lut_wb[count]);
            }
        }

        public override void SetFrameMemory(byte[] image_buffer)
        {
            SetFrameMemory(image_buffer, 0, 0, (int)Width, (int)Height);
        }

        public override void SetFrameMemory(byte[] buffer, int x, int y, int width, int height)
        {
            //hack for now - we need to update the code to copy properly from the entire buffer
            //code expects the buffer to be the exact size we need
            x = 0;
            y = 0;
            width = (int)Width;
            height = (int)Height;

            if (buffer != null)
            {
                SendCommand(PARTIAL_DATA_START_TRANSMISSION_2);
                SendData(x >> 8);
                SendData(x & 0xf8);     // x should be the multiple of 8, the last 3 bit will always be ignored
                SendData(y >> 8);
                SendData(y & 0xff);
                SendData(width >> 8);
                SendData(width & 0xf8);     // w (width) should be the multiple of 8, the last 3 bit will always be ignored
                SendData(height >> 8);
                SendData(height & 0xff);

                DelayMs(2);

                for (int i = 0; i < width / 8 * height; i++)
                {
                    SendData(buffer[i]);
                }
                    
                DelayMs(2);
            }
        }

        void RefreshPartial(int x, int y, int w, int l)
        {
            SendCommand(PARTIAL_DISPLAY_REFRESH);
            SendData(x >> 8);
            SendData(x & 0xf8);     // x should be the multiple of 8, the last 3 bit will always be ignored
            SendData(y >> 8);
            SendData(y & 0xff);
            SendData(w >> 8);
            SendData(w & 0xf8);     // w (width) should be the multiple of 8, the last 3 bit will always be ignored
            SendData(l >> 8);
            SendData(l & 0xff);

            WaitUntilIdle();
        }

        void DisplayFrame(byte[] frame_buffer) 
        {
            if (frame_buffer == null)
                return;
            
            SendCommand(DATA_START_TRANSMISSION_1);
            DelayMs(2);
            for(int i = 0; i< Width / 8 * Height; i++)
                SendData(0xFF);

            DelayMs(2);
            SendCommand(DATA_START_TRANSMISSION_2);
            DelayMs(2);

            SendData(frame_buffer);
  
            DelayMs(2);
            SendCommand(DISPLAY_REFRESH);
            WaitUntilIdle();
        }

        public override void DisplayFrame ()
        {
            SendCommand(DISPLAY_REFRESH);
            WaitUntilIdle();
        }

        public void ClearFrame()
        {
            SendCommand(DATA_START_TRANSMISSION_1);
            DelayMs(2);
            for (int i = 0; i < Width * Height / 8; i++)
            {
                SendData(0xFF);
            }

            DelayMs(2);
            SendCommand(DATA_START_TRANSMISSION_2);
            DelayMs(2);

            for (int i = 0; i < Width * Height / 8; i++)
            {
                SendData(0xFF);
            }
            DelayMs(2);
        }

        // EPD2IN7 commands
        protected static byte PANEL_SETTING                               = 0x00;
        protected static byte POWER_SETTING                               = 0x01;
        protected static byte POWER_OFF                                   = 0x02;
        protected static byte POWER_OFF_SEQUENCE_SETTING                  = 0x03;
        protected static byte POWER_ON                                    = 0x04;
        protected static byte POWER_ON_MEASURE                            = 0x05;
        protected static byte BOOSTER_SOFT_START                          = 0x06;
        protected static byte DEEP_SLEEP                                  = 0x07;
        protected static byte DATA_START_TRANSMISSION_1                   = 0x10;
        protected static byte DATA_STOP                                   = 0x11;
        protected static byte DISPLAY_REFRESH                             = 0x12;
        protected static byte DATA_START_TRANSMISSION_2                   = 0x13;
        protected static byte PARTIAL_DATA_START_TRANSMISSION_1           = 0x14; 
        protected static byte PARTIAL_DATA_START_TRANSMISSION_2           = 0x15; 
        protected static byte PARTIAL_DISPLAY_REFRESH                     = 0x16;
        protected static byte LUT_FOR_VCOM                                = 0x20; 
        protected static byte LUT_WHITE_TO_WHITE                          = 0x21;
        protected static byte LUT_BLACK_TO_WHITE                          = 0x22;
        protected static byte LUT_WHITE_TO_BLACK                          = 0x23;
        protected static byte LUT_BLACK_TO_BLACK                          = 0x24;
        protected static byte PLL_CONTROL                                 = 0x30;
        protected static byte TEMPERATURE_SENSOR_COMMAND                  = 0x40;
        protected static byte TEMPERATURE_SENSOR_CALIBRATION              = 0x41;
        protected static byte TEMPERATURE_SENSOR_WRITE                    = 0x42;
        protected static byte TEMPERATURE_SENSOR_READ                     = 0x43;
        protected static byte VCOM_AND_DATA_INTERVAL_SETTING              = 0x50;
        protected static byte LOW_POWER_DETECTION                         = 0x51;
        protected static byte TCON_SETTING                                = 0x60;
        protected static byte TCON_RESOLUTION                             = 0x61;
        protected static byte SOURCE_AND_GATE_START_SETTING               = 0x62;
        protected static byte GET_STATUS                                  = 0x71;
        protected static byte AUTO_MEASURE_VCOM                           = 0x80;
        protected static byte VCOM_VALUE                                  = 0x81;
        protected static byte VCM_DC_SETTING_REGISTER                     = 0x82;
        protected static byte PROGRAM_MODE                                = 0xA0;
        protected static byte ACTIVE_PROGRAM                              = 0xA1;
        protected static byte READ_OTP_DATA                               = 0xA2;

        protected static byte[] lut_vcom_dc = {
            0x00, 0x00,
            0x00, 0x0F, 0x0F, 0x00, 0x00, 0x05,
            0x00, 0x32, 0x32, 0x00, 0x00, 0x02,
            0x00, 0x0F, 0x0F, 0x00, 0x00, 0x05,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        };

        //R21H
        protected static byte[] lut_ww = {
            0x50, 0x0F, 0x0F, 0x00, 0x00, 0x05,
            0x60, 0x32, 0x32, 0x00, 0x00, 0x02,
            0xA0, 0x0F, 0x0F, 0x00, 0x00, 0x05,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        };

        //R22H    r
        protected static byte[] lut_bw =
        {
            0x50, 0x0F, 0x0F, 0x00, 0x00, 0x05,
            0x60, 0x32, 0x32, 0x00, 0x00, 0x02,
            0xA0, 0x0F, 0x0F, 0x00, 0x00, 0x05,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        };

        //R24H    b
        protected static byte[] lut_bb =
        {
            0xA0, 0x0F, 0x0F, 0x00, 0x00, 0x05,
            0x60, 0x32, 0x32, 0x00, 0x00, 0x02,
            0x50, 0x0F, 0x0F, 0x00, 0x00, 0x05,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        };

        //R23H    w
        protected static byte[] lut_wb =
        {
            0xA0, 0x0F, 0x0F, 0x00, 0x00, 0x05,
            0x60, 0x32, 0x32, 0x00, 0x00, 0x02,
            0x50, 0x0F, 0x0F, 0x00, 0x00, 0x05,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        };
    }
}