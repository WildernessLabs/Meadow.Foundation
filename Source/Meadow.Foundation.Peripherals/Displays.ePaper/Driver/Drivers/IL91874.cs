using Meadow.Devices;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays.ePaper
{
    /// <summary>
    /// Represents IL91874 ePaper color displays
    /// 264x176, 2.7inch tri color e-Ink display / SPI interface 
    /// </summary>
    public class Il91874 : EPaperTriColorBase
    {
        /// <summary>
        /// Create a new Il91874 object
        /// </summary>
        /// <param name="device">Meadow device</param>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="busyPin">Busy pin</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        public Il91874(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, IPin busyPin,
            int width = 176, int height = 264) :
            base(device, spiBus, chipSelectPin, dcPin, resetPin, busyPin, width, height)
        {
        }

        /// <summary>
        /// Create a new Il91874 ePaper display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="busyPort">Busy input port</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        public Il91874(ISpiBus spiBus,
            IDigitalOutputPort chipSelectPort,
            IDigitalOutputPort dataCommandPort,
            IDigitalOutputPort resetPort,
            IDigitalInputPort busyPort,
            int width, int height) :
            base(spiBus, chipSelectPort, dataCommandPort, resetPort, busyPort, width, height)
        {
        }

        protected override bool IsBlackInverted => true;

        protected override bool IsColorInverted => false;

        protected override void Initialize()
        {
            /* EPD hardware init start */
            Reset();

            SendCommand(Command.POWER_ON);
            WaitUntilIdle();

            SendCommand(Command.PANEL_SETTING);
            SendData(0xaf);        //KW-BF   KWR-AF    BWROTP 0f

            SendCommand(Command.PLL_CONTROL);
            SendData(0x3a);       //3A 100HZ   29 150Hz 39 200HZ    31 171HZ

            SendCommand(Command.POWER_SETTING);
            SendData(0x03);                  // VDS_EN, VDG_EN
            SendData(0x00);                  // VCOM_HV, VGHL_LV[1], VGHL_LV[0]
            SendData(0x2b);                  // VDH
            SendData(0x2b);                  // VDL
            SendData(0x09);                  // VDHR

            SendCommand(Command.BOOSTER_SOFT_START);
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
            SendData(0x73);
            SendData(0x41);

            SendCommand(Command.VCM_DC_SETTING);
            SendData(0x12);

            SendCommand(Command.VCOM_AND_DATA_INTERVAL_SETTING);
            SendData(0x87);        // define by OTP

            SetLut();

            SendCommand(Command.RESOLUTION_SETTING);
            SendData(Width >> 8);
            SendData(Width & 0xff);        //176      
            SendData(Height >> 8);
            SendData(Height & 0xff);         //264

            SendCommand(Command.PARTIAL_DISPLAY_REFRESH);
            SendData(0x00);
            /* EPD hardware init end */
        }

        public override void Show(int left, int top, int right, int bottom)
        {
            TransmitPartial(blackImageBuffer.Buffer, colorImageBuffer.Buffer,
                        left, top,
                        right - left,
                        top - bottom);

            RefreshPartial(left, top,
                        right - left,
                        top - bottom);
        }

        public override void Show()
        {
            DisplayFrame(blackImageBuffer.Buffer, colorImageBuffer.Buffer);
        }

        void SetLut()
        {   //should probably just loop over the array length
            //or transmit the data in one SendData call
            SendCommand(Command.LUT_FOR_VCOM);                     //vcom
            for (int i = 0; i < 44; i++)
            {
                SendData(LUT_VCOM_DC[i]);
            }

            SendCommand(Command.LUT_WHITE_TO_WHITE);               //ww --
            for (int i = 0; i < 42; i++)
            {
                SendData(LUT_WW[i]);
            }

            SendCommand(Command.LUT_BLACK_TO_WHITE);               //bw r
            for (int i = 0; i < 42; i++)
            {
                SendData(LUT_BW[i]);
            }
            //data for WB & BB are swapped here in the arduino driver
            SendCommand(Command.LUT_WHITE_TO_BLACK);               //wb w
            for (int i = 0; i < 42; i++)
            {
                SendData(LUT_WB[i]);
            }

            SendCommand(Command.LUT_BLACK_TO_BLACK);               //bb b
            for (int i = 0; i < 42; i++)
            {
                SendData(LUT_BB[i]);
            }
        }

        void TransmitPartial(byte[] bufferBlack, byte[] bufferRed, int x, int y, int width, int height)
        {
            TransmitPartialRed(bufferRed, x, y, width, height);

            TransmitPartialBlack(bufferBlack, x, y, width, height);
        }

        /**
         *  @brief: transmit partial data to the black part of SRAM
         */
        void TransmitPartialBlack(byte[] bufferBlack, int x, int y, int width, int height)
        {
            if (bufferBlack != null)
            {
                SendCommand(Command.PARTIAL_DATA_START_TRANSMISSION_1);
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
                    SendData(bufferBlack[i]);
                }
                DelayMs(2);
            }
        }

        /**
         *  @brief: transmit partial data to the red part of SRAM
         */
        void TransmitPartialRed(byte[] bufferRed, int x, int y, int width, int height)
        {
            if (bufferRed != null)
            {
                SendCommand(Command.PARTIAL_DATA_START_TRANSMISSION_2);
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
                    SendData(bufferRed[i]);
                }
                DelayMs(2);
            }
        }

        /**
         * @brief: refreshes a specific part of the display
         */
        void RefreshPartial(int x, int y, int width, int height)
        {
            SendCommand(Command.PARTIAL_DISPLAY_REFRESH);
            SendData(x >> 8);
            SendData(x & 0xf8);     // x should be the multiple of 8, the last 3 bit will always be ignored
            SendData(y >> 8);
            SendData(y & 0xff);
            SendData(width >> 8);
            SendData(width & 0xf8);     // w (width) should be the multiple of 8, the last 3 bit will always be ignored
            SendData(height >> 8);
            SendData(height & 0xff);

            WaitUntilIdle();
        }

        /**
         * @brief: refresh and displays the frame
         */
        void DisplayFrame(byte[] bufferBlack, byte[] bufferRed)
        {
            if (bufferBlack != null)
            {
                SendCommand(Command.DATA_START_TRANSMISSION_1);
                DelayMs(2);

                dataCommandPort.State = DataState;

                for (int i = 0; i < Width * Height / 8; i++)
                {   //I bet we can optimize this .... seems silly to send a byte at a time
                    spiPeripheral.Write(bufferBlack[i]);
                }

                DelayMs(2);
            }

            if (bufferRed != null)
            {
                SendCommand(Command.DATA_START_TRANSMISSION_2);
                DelayMs(2);

                dataCommandPort.State = DataState;

                for (int i = 0; i < Width * Height / 8; i++)
                {
                    spiPeripheral.Write(bufferRed[i]);
                }
                DelayMs(2);
            }

            SendCommand(Command.DISPLAY_REFRESH);

            WaitUntilIdle();
        }

        /**
         * @brief: clear the frame data from the SRAM, this won't refresh the display
         * clear the display to white
         */
        public void ClearFrame()
        {
            SendCommand(Command.RESOLUTION_SETTING);
            SendData(Width >> 8);
            SendData(Width & 0xff);        //176      
            SendData(Height >> 8);
            SendData(Height & 0xff);         //264

            SendCommand(Command.DATA_START_TRANSMISSION_1);
            DelayMs(2);

            for (int i = 0; i < Width * Height / 8; i++)
            {
                // SendData(0x00);//black?
                SendData(0xFF);//white
            }

            DelayMs(2);
            SendCommand(Command.DATA_START_TRANSMISSION_2);
            DelayMs(2);

            for (int i = 0; i < Width * Height / 8; i++)
            {
                SendData(0x00);//nothing?
                               //  SendData(0xFF); //red 
            }
            DelayMs(2);
        }

        /**
         * @brief: This displays the frame data from SRAM
         */
        public void DisplayFrame()
        {
            SendCommand(Command.DISPLAY_REFRESH);
            WaitUntilIdle();
        }

        /**
         * @brief: After this command is transmitted, the chip would enter the deep-sleep mode to save power. 
         *         The deep sleep mode would return to standby by hardware reset. The only one parameter is a 
         *         check code, the command would be executed if check code = 0xA5. 
         *         You can use Epd::Reset() to awaken and use Epd::Init() to initialize.
         */
        public void Sleep()
        {
            SendCommand(Command.DEEP_SLEEP);
            SendData(0xa5);
        }

        readonly byte[] LUT_VCOM_DC =
        {
            0x00, 0x00,
            0x00, 0x1A, 0x1A, 0x00, 0x00, 0x01,
            0x00, 0x0A, 0x0A, 0x00, 0x00, 0x08,
            0x00, 0x0E, 0x01, 0x0E, 0x01, 0x10,
            0x00, 0x0A, 0x0A, 0x00, 0x00, 0x08,
            0x00, 0x04, 0x10, 0x00, 0x00, 0x05,
            0x00, 0x03, 0x0E, 0x00, 0x00, 0x0A,
            0x00, 0x23, 0x00, 0x00, 0x00, 0x01
        };

        //R21H
        readonly byte[] LUT_WW =
        {
            0x90, 0x1A, 0x1A, 0x00, 0x00, 0x01,
            0x40, 0x0A, 0x0A, 0x00, 0x00, 0x08,
            0x84, 0x0E, 0x01, 0x0E, 0x01, 0x10,
            0x80, 0x0A, 0x0A, 0x00, 0x00, 0x08,
            0x00, 0x04, 0x10, 0x00, 0x00, 0x05,
            0x00, 0x03, 0x0E, 0x00, 0x00, 0x0A,
            0x00, 0x23, 0x00, 0x00, 0x00, 0x01
        };

        //R22H    r
        readonly byte[] LUT_BW =
        {
            0xA0, 0x1A, 0x1A, 0x00, 0x00, 0x01,
            0x00, 0x0A, 0x0A, 0x00, 0x00, 0x08,
            0x84, 0x0E, 0x01, 0x0E, 0x01, 0x10,
            0x90, 0x0A, 0x0A, 0x00, 0x00, 0x08,
            0xB0, 0x04, 0x10, 0x00, 0x00, 0x05,
            0xB0, 0x03, 0x0E, 0x00, 0x00, 0x0A,
            0xC0, 0x23, 0x00, 0x00, 0x00, 0x01
        };

        //R23H    w
        readonly byte[] LUT_BB =
        {
            0x90, 0x1A, 0x1A, 0x00, 0x00, 0x01,
            0x40, 0x0A, 0x0A, 0x00, 0x00, 0x08,
            0x84, 0x0E, 0x01, 0x0E, 0x01, 0x10,
            0x80, 0x0A, 0x0A, 0x00, 0x00, 0x08,
            0x00, 0x04, 0x10, 0x00, 0x00, 0x05,
            0x00, 0x03, 0x0E, 0x00, 0x00, 0x0A,
            0x00, 0x23, 0x00, 0x00, 0x00, 0x01
        };

        //R24H    b
        readonly byte[] LUT_WB =
        {
            0x90, 0x1A, 0x1A, 0x00, 0x00, 0x01,
            0x20, 0x0A, 0x0A, 0x00, 0x00, 0x08,
            0x84, 0x0E, 0x01, 0x0E, 0x01, 0x10,
            0x10, 0x0A, 0x0A, 0x00, 0x00, 0x08,
            0x00, 0x04, 0x10, 0x00, 0x00, 0x05,
            0x00, 0x03, 0x0E, 0x00, 0x00, 0x0A,
            0x00, 0x23, 0x00, 0x00, 0x00, 0x01
        };
    }
}