using Meadow.Devices;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays.ePaper
{
    //similar to IL91874 ... appears to be an old version v0.3
    //GxGDEW027W3
    //currently hard coded to the avaliable display size
    /// <summary>
    /// Represents the older v0.3 Il91874V03 ePaper color displays
    /// 264x176, 2.7inch tri color e-Ink display / SPI interface 
    /// </summary>
    public class Il91874V03 : EPaperMonoBase
    {
        /// <summary>
        /// Create a new Il91874V03 object
        /// </summary>
        /// <param name="device">Meadow device</param>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="busyPin">Busy pin</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        public Il91874V03(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, IPin busyPin,
            int width = 176, int height = 264) :
            base(device, spiBus, chipSelectPin, dcPin, resetPin, busyPin, width, height)
        { }

        /// <summary>
        /// Create a new Il91874V03 ePaper display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="busyPort">Busy input port</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        public Il91874V03(ISpiBus spiBus,
            IDigitalOutputPort chipSelectPort,
            IDigitalOutputPort dataCommandPort,
            IDigitalOutputPort resetPort,
            IDigitalInputPort busyPort,
            int width, int height) :
            base(spiBus, chipSelectPort, dataCommandPort, resetPort, busyPort, width, height)
        {
        }

        protected override void Initialize()
        {
            Reset();

            SendData(0x00);                  // VCOM_HV, VGHL_LV[1], VGHL_LV[0]
            SendData(0x2b);                  // VDH
            SendData(0x2b);                  // VDL
            SendData(0x09);                  // VDHR
            SendCommand(CommandIL91874V03.BOOSTER_SOFT_START);
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
            SendCommand(CommandIL91874V03.PARTIAL_DISPLAY_REFRESH);
            SendData(0x00);
            SendCommand(CommandIL91874V03.POWER_ON);
            WaitUntilIdle();

            SendCommand(CommandIL91874V03.PANEL_SETTING);
            SendData(0xAF);        //KW-BF   KWR-AF    BWROTP 0f
            SendCommand(CommandIL91874V03.PLL_CONTROL);
            SendData(0x3A);       //3A 100HZ   29 150Hz 39 200HZ    31 171HZ
            SendCommand(CommandIL91874V03.VCM_DC_SETTING_REGISTER);
            SendData(0x12);
            DelayMs(2);
            SetLookupTable();
        }

        void SetLookupTable()
        {
            int count;
            SendCommand(CommandIL91874V03.LUT_FOR_VCOM);                            //vcom
            for (count = 0; count < 44; count++)
            {
                SendData(lut_vcom_dc[count]);
            }

            SendCommand(CommandIL91874V03.LUT_WHITE_TO_WHITE);                      //ww --
            for (count = 0; count < 42; count++)
            {
                SendData(lut_ww[count]);
            }

            SendCommand(CommandIL91874V03.LUT_BLACK_TO_WHITE);                      //bw r
            for (count = 0; count < 42; count++)
            {
                SendData(lut_bw[count]);
            }

            SendCommand(CommandIL91874V03.LUT_WHITE_TO_BLACK);                      //wb w
            for (count = 0; count < 42; count++)
            {
                SendData(lut_bb[count]);
            }

            SendCommand(CommandIL91874V03.LUT_BLACK_TO_BLACK);                      //bb b
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
            width = (int)base.Width;
            height = (int)base.Height;

            if (buffer != null)
            {
                SendCommand(CommandIL91874V03.PARTIAL_DATA_START_TRANSMISSION_2);
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
            SendCommand(CommandIL91874V03.PARTIAL_DISPLAY_REFRESH);
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

            SendCommand(CommandIL91874V03.DATA_START_TRANSMISSION_1);
            DelayMs(2);
            for (int i = 0; i < Width / 8 * Height; i++)
                SendData(0xFF);

            DelayMs(2);
            SendCommand(CommandIL91874V03.DATA_START_TRANSMISSION_2);
            DelayMs(2);

            SendData(frame_buffer);

            DelayMs(2);
            SendCommand(CommandIL91874V03.DISPLAY_REFRESH);
            WaitUntilIdle();
        }

        public override void DisplayFrame()
        {
            SendCommand(CommandIL91874V03.DISPLAY_REFRESH);
            WaitUntilIdle();
        }

        public void ClearFrame()
        {
            SendCommand(CommandIL91874V03.DATA_START_TRANSMISSION_1);
            DelayMs(2);
            for (int i = 0; i < Width * Height / 8; i++)
            {
                SendData(0xFF);
            }

            DelayMs(2);
            SendCommand(CommandIL91874V03.DATA_START_TRANSMISSION_2);
            DelayMs(2);

            for (int i = 0; i < Width * Height / 8; i++)
            {
                SendData(0xFF);
            }
            DelayMs(2);
        }

        protected void SendCommand(CommandIL91874V03 command)
        {
            SendCommand((byte)command);
        }

        protected enum CommandIL91874V03 : byte
        {
            PANEL_SETTING = 0x00,
            POWER_SETTING = 0x01,
            POWER_OFF = 0x02,
            POWER_OFF_SEQUENCE_SETTING = 0x03,
            POWER_ON = 0x04,
            POWER_ON_MEASURE = 0x05,
            BOOSTER_SOFT_START = 0x06,
            DEEP_SLEEP = 0x07,
            DATA_START_TRANSMISSION_1 = 0x10,
            DATA_STOP = 0x11,
            DISPLAY_REFRESH = 0x12,
            DATA_START_TRANSMISSION_2 = 0x13,
            PARTIAL_DATA_START_TRANSMISSION_1 = 0x14,
            PARTIAL_DATA_START_TRANSMISSION_2 = 0x15,
            PARTIAL_DISPLAY_REFRESH = 0x16,
            LUT_FOR_VCOM = 0x20,
            LUT_WHITE_TO_WHITE = 0x21,
            LUT_BLACK_TO_WHITE = 0x22,
            LUT_WHITE_TO_BLACK = 0x23,
            LUT_BLACK_TO_BLACK = 0x24,
            PLL_CONTROL = 0x30,
            TEMPERATURE_SENSOR_COMMAND = 0x40,
            TEMPERATURE_SENSOR_CALIBRATION = 0x41,
            TEMPERATURE_SENSOR_WRITE = 0x42,
            TEMPERATURE_SENSOR_READ = 0x43,
            VCOM_AND_DATA_INTERVAL_SETTING = 0x50,
            LOW_POWER_DETECTION = 0x51,
            TCON_SETTING = 0x60,
            TCON_RESOLUTION = 0x61,
            SOURCE_AND_GATE_START_SETTING = 0x62,
            GET_STATUS = 0x71,
            AUTO_MEASURE_VCOM = 0x80,
            VCOM_VALUE = 0x81,
            VCM_DC_SETTING_REGISTER = 0x82,
            PROGRAM_MODE = 0xA0,
            ACTIVE_PROGRAM = 0xA1,
            READ_OTP_DATA = 0xA2,
        }

        

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