using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Displays.ePaper
{
    public class EPD1i54c : EPDColorBase
    {
        public EPD1i54c(IIODevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, IPin busyPin) :
            base(device, spiBus, chipSelectPin, dcPin, resetPin, busyPin)
        { }

        public override uint Width => 152;
        public override uint Height => 152;

        protected override void Initialize()
        {
            Reset();
            SendCommand(POWER_SETTING);
            SendData(0x07);
            SendData(0x00);
            SendData(0x08);
            SendData(0x00);
            SendCommand(BOOSTER_SOFT_START);
            SendData(0x17);
            SendData(0x17);
            SendData(0x17);
            SendCommand(POWER_ON);

            WaitUntilIdle();

            SendCommand(PANEL_SETTING);
            SendData(0x0F);
            SendData(0x0D);
            SendCommand(VCOM_AND_DATA_INTERVAL_SETTING);
            SendData(0xF7);

            SendCommand(RESOLUTION_SETTING);
            SendData(0x98); //152
            SendData(0x00); 
            SendData(0x98); //152
            SendCommand(VCM_DC_SETTING);
            SendData(0xF7);

            SetLutBlack();
            SetLutRed();
        }

        protected void DisplayFrame()
        {
            if (blackImageBuffer != null)
            {
                SendCommand(DATA_START_TRANSMISSION_1);
                DelayMs(2);
                SendData(blackImageBuffer);
                DelayMs(2);
            }

            if (colorImageBuffer != null)
            {
                SendCommand(DATA_START_TRANSMISSION_2);
                DelayMs(2);
                SendData(colorImageBuffer);
                DelayMs(2);
            }
            SendCommand(DISPLAY_REFRESH);
            WaitUntilIdle();
        }

        protected void SetLutBlack()
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

        protected void SetLutRed()
        {
            SendCommand(0x25);
            SendData(lut_vcom1);
            SendCommand(0x26);
            SendData(lut_red0);
            SendCommand(0x27);
            SendData(lut_red1);
        }

        protected void Sleep()
        {
            SendCommand(VCOM_AND_DATA_INTERVAL_SETTING);
            SendData(0x17);
            SendCommand(VCM_DC_SETTING);         //to solve Vcom drop
            SendData(0x00);
            SendCommand(POWER_SETTING);         //power setting
            SendData(0x02);        //gate switch to external
            SendData(0x00);
            SendData(0x00);
            SendData(0x00);
            WaitUntilIdle();
            SendCommand(POWER_OFF);         //power off
        }

        protected override void Refresh()
        {
            DisplayFrame();
        }

        byte[] lut_vcom0 =
        {
            0x0E, 0x14, 0x01, 0x0A, 0x06, 0x04, 0x0A, 0x0A,
            0x0F, 0x03, 0x03, 0x0C, 0x06, 0x0A, 0x00
        };

        byte[] lut_w =
        {
            0x0E, 0x14, 0x01, 0x0A, 0x46, 0x04, 0x8A, 0x4A,
            0x0F, 0x83, 0x43, 0x0C, 0x86, 0x0A, 0x04
        };

        byte[] lut_b =
        {
            0x0E, 0x14, 0x01, 0x8A, 0x06, 0x04, 0x8A, 0x4A,
            0x0F, 0x83, 0x43, 0x0C, 0x06, 0x4A, 0x04
        };

        byte[] lut_g1 =
        {
            0x8E, 0x94, 0x01, 0x8A, 0x06, 0x04, 0x8A, 0x4A,
            0x0F, 0x83, 0x43, 0x0C, 0x06, 0x0A, 0x04
        };

        byte[] lut_g2 =
        {
            0x8E, 0x94, 0x01, 0x8A, 0x06, 0x04, 0x8A, 0x4A,
            0x0F, 0x83, 0x43, 0x0C, 0x06, 0x0A, 0x04
        };

        byte[] lut_vcom1 =
        {
            0x03, 0x1D, 0x01, 0x01, 0x08, 0x23, 0x37, 0x37,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };

        byte[] lut_red0 =
        {
            0x83, 0x5D, 0x01, 0x81, 0x48, 0x23, 0x77, 0x77,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };

        byte[] lut_red1 =
        {
            0x03, 0x1D, 0x01, 0x01, 0x08, 0x23, 0x37, 0x37,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };
    }
}
