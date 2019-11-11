using System;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays.ePaper
{
    /// <summary>
    ///     Provide an interface to the WaveShare ePaper color displays
    ///     264x176, 2.7inch tri color e-Ink display / SPI interface 
    /// </summary>
    public class IL91874 : EPDColorBase
    {
        public IL91874(IIODevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, IPin busyPin,
            uint width = 176, uint height = 264) :
            base(device, spiBus, chipSelectPin, dcPin, resetPin, busyPin, width, height)
        {
        }

        protected override bool IsBlackInverted => false;

        protected override bool IsColorInverted => true;

        protected override void Initialize()
        {
            /* EPD hardware init start */
            Reset();

            SendCommand(POWER_ON);
            WaitUntilIdle();

            SendCommand(PANEL_SETTING);
            SendData(0xaf);        //KW-BF   KWR-AF    BWROTP 0f

            SendCommand(PLL_CONTROL);
            SendData(0x3a);       //3A 100HZ   29 150Hz 39 200HZ    31 171HZ

            SendCommand(POWER_SETTING);
            SendData(0x03);                  // VDS_EN, VDG_EN
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
            SendData(0x73);
            SendData(0x41);

            SendCommand(VCM_DC_SETTING);
            SendData(0x12);

            SendCommand(VCOM_AND_DATA_INTERVAL_SETTING);
            SendData(0x87);        // define by OTP

            SetLut();

            SendCommand(RESOLUTION_SETTING);
            SendData((int)Width >> 8);
            SendData((int)Width & 0xff);        //176      
            SendData((int)Height >> 8);
            SendData((int)Height & 0xff);         //264

            SendCommand(PARTIAL_DISPLAY_REFRESH);
            SendData(0x00);
            /* EPD hardware init end */
        }

        protected override void Refresh()
        {
            xRefreshStart = -1;
            if (xRefreshStart == -1)
            {
                DisplayFrame(blackImageBuffer, colorImageBuffer);
            }
            else
            {
                TransmitPartial(blackImageBuffer, colorImageBuffer,
                        xRefreshStart, yRefreshStart,
                        xRefreshEnd - xRefreshStart,
                        yRefreshEnd - yRefreshStart);

                RefreshPartial(xRefreshStart, yRefreshStart,
                    xRefreshEnd - xRefreshStart, yRefreshEnd - yRefreshStart);

               // DisplayFrame();
            }

            xRefreshStart = yRefreshStart = xRefreshEnd = yRefreshEnd = -1;
        }

        void SetLut()
        {   //should probably just loop over the array length
            //or transmit the data in one SendData call
            SendCommand(LUT_FOR_VCOM);                     //vcom
            for (int i = 0; i < 44; i++)
            {
                SendData(LUT_VCOM_DC[i]);
            }

            SendCommand(LUT_WHITE_TO_WHITE);                      //ww --
            for (int i = 0; i < 42; i++)
            {
                SendData(LUT_WW[i]);
            }

            SendCommand(LUT_BLACK_TO_WHITE);                      //bw r
            for (int i = 0; i < 42; i++)
            {
                SendData(LUT_BW[i]);
            }
            //data for WB & BB are swapped here in the arduino driver
            SendCommand(LUT_WHITE_TO_BLACK);                      //wb w
            for (int i = 0; i < 42; i++)
            {
                SendData(LUT_WB[i]);
            }

            SendCommand(LUT_BLACK_TO_BLACK);                      //bb b
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
                SendCommand(PARTIAL_DATA_START_TRANSMISSION_1);
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
            SendCommand(PARTIAL_DISPLAY_REFRESH);
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
            var sendBuffer = new byte[Width / 8];

            if (bufferBlack != null)
            {
                Console.WriteLine("Send black data");

                SendCommand(DATA_START_TRANSMISSION_1);
                DelayMs(2);

              //  SendData(bufferBlack);
                for (int i = 0; i < Width * Height / 8; i++) 
                {   //I bet we can optimize this .... seems silly to send a byte at a time
                     SendData(bufferBlack[i]);
                    
                } 
                DelayMs(2);

                Console.WriteLine("Send black data complete");
            }

            if (bufferRed != null)
            {
                SendCommand(DATA_START_TRANSMISSION_2);
                DelayMs(2);

                for(int i = 0; i < Width * Height / 8; i++) 
                {
                    SendData(bufferRed[i]);  
                }  
                DelayMs(2);

                Console.WriteLine("Send red data complete");
            }

            SendCommand(DISPLAY_REFRESH);

            WaitUntilIdle();
        }

        /**
         * @brief: clear the frame data from the SRAM, this won't refresh the display
         * clear the display to white
         */
        public void ClearFrame()
        {
            SendCommand(RESOLUTION_SETTING);
            SendData((int)Width >> 8);
            SendData((int)Width & 0xff);        //176      
            SendData((int)Height >> 8);
            SendData((int)Height & 0xff);         //264

            SendCommand(DATA_START_TRANSMISSION_1);
            DelayMs(2);

            for (int i = 0; i < Width * Height / 8; i++)
            {
               // SendData(0x00);//black?
                SendData(0xFF);//white
            }

            DelayMs(2);
            SendCommand(DATA_START_TRANSMISSION_2);
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
            SendCommand(DISPLAY_REFRESH);
            WaitUntilIdle();
        }

        /**
         * @brief: After this command is transmitted, the chip would enter the deep-sleep mode to save power. 
         *         The deep sleep mode would return to standby by hardware reset. The only one parameter is a 
         *         check code, the command would be executed if check code = 0xA5. 
         *         You can use Epd::Reset() to awaken and use Epd::Init() to initialize.
         */
        void Sleep()
        {
            SendCommand(DEEP_SLEEP);
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