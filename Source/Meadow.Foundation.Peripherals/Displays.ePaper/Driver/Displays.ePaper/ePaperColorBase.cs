using System;
using Meadow.Devices;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays.ePaper
{
    /// <summary>
    ///     Provide an interface for ePaper 3 color displays
    /// </summary>
    public abstract class EpdColorBase : SpiDisplayBase
    {
        protected abstract bool IsBlackInverted { get; }
        protected abstract bool IsColorInverted { get; }

        public override DisplayColorMode ColorMode => DisplayColorMode.Format2bpp;

        protected readonly byte[] blackImageBuffer;
        protected readonly byte[] colorImageBuffer;

        public override int Width => width;
        public override int Height => height;

        int width;
        int height;

        private EpdColorBase()
        { }

        public EpdColorBase(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, IPin busyPin,
            int width, int height)
        {
            this.width = width;
            this.height = height;

            dataCommandPort = device.CreateDigitalOutputPort(dcPin, false);
            resetPort = device.CreateDigitalOutputPort(resetPin, true);
            busyPort = device.CreateDigitalInputPort(busyPin);

            spiPeripheral = new SpiPeripheral(spiBus, device.CreateDigitalOutputPort(chipSelectPin));

            blackImageBuffer = new byte[Width * Height / 8];
            colorImageBuffer = new byte[Width * Height / 8];

            for (int i = 0; i < blackImageBuffer.Length; i++)
            {
                blackImageBuffer[i] = 0xFF;
                colorImageBuffer[i] = 0xFF;
            }

            Initialize();
        }

        protected abstract void Initialize();

        public override void Clear(bool updateDisplay = false)
        {
            Clear(false, updateDisplay);
        }

        public void Clear(Color color, bool updateDisplay = false)
        {
            bool colored = false;
            if (color.B > 0 || color.R > 0 || color.G > 0)
                colored = true;

            Clear(colored, updateDisplay);
        }

        public void Clear(bool colored, bool updateDisplay = false)
        {
            //   ClearFrameMemory((byte)(colored ? 0 : 0xFF));
            //   DisplayFrame();

            for (int i = 0; i < blackImageBuffer.Length; i++)
            {
                blackImageBuffer[i] = colored ? (byte)0 : (byte)255;

                colorImageBuffer[i] = IsColorInverted ? (byte)0 : (byte)255;//first val should be 0
            }

            if (updateDisplay)
            {
                Show();
            }
        }

        public override void DrawPixel(int x, int y, bool colored)
        {
            if (colored)
            {   //0x80 = 128 = 0b_10000000
                blackImageBuffer[(x + y * Width) / 8] &= (byte)~(0x80 >> (x % 8));
            }
            else
            {
                blackImageBuffer[(x + y * Width) / 8] |= (byte)(0x80 >> (x % 8));
            }

            if (IsColorInverted)
            {
                colorImageBuffer[(x + y * Width) / 8] &= (byte)~(0x80 >> (x % 8));
            }
            else
            {
                colorImageBuffer[(x + y * Width) / 8] |= (byte)(0x80 >> (x % 8));
            }
        }

        public override void InvertPixel(int x, int y)
        {
            blackImageBuffer[(x + y * Width) / 8] ^= (byte)~(0x80 >> (x % 8));
        }

        public void DrawColoredPixel(int x, int y, bool colored)
        {
            if ((colored && !IsColorInverted) ||
                (!colored && IsColorInverted))
            {
                colorImageBuffer[(x + y * Width) / 8] &= (byte)~(0x80 >> (x % 8));
            }
            else
            {
                colorImageBuffer[(x + y * Width) / 8] |= (byte)(0x80 >> (x % 8));
            }
            blackImageBuffer[(x + y * Width) / 8] |= (byte)(0x80 >> (x % 8));
        }

        public override void DrawPixel(int x, int y, Color color)
        {
            if (color.B == 0 && color.G == 0 && color.R > 0.5)
            {
                DrawColoredPixel(x, y, true);
            }
            else
            {
                DrawPixel(x, y, color.Color1bpp);
            }
        }

        public void DrawPixel(int x, int y, byte r, byte g, byte b)
        {
            if (g == 0 && b == 0 && r > 127)
            {
                DrawColoredPixel(x, y, true);
            }
            else
            {
                DrawPixel(x, y, r > 0 || g > 0 || b > 0);
            }
        }

        protected void SendCommand(Command command)
        {
            SendCommand((byte)command);
        }

        // 2.13b + 2.7b (red) commands

        public enum Command : byte
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
            TEMPERATURE_SENSOR_CALIBRATION = 0x40,
            TEMPERATURE_SENSOR_SELECTION = 0x41,
            TEMPERATURE_SENSOR_WRITE = 0x42,
            TEMPERATURE_SENSOR_READ = 0x43,
            VCOM_AND_DATA_INTERVAL_SETTING = 0x50,
            LOW_POWER_DETECTION = 0x51,
            TCON_SETTING = 0x60,
            RESOLUTION_SETTING = 0x61,
            SOURCE_AND_GATE_START_SETTING = 0x62,
            GET_STATUS = 0x71,
            AUTO_MEASURE_VCOM = 0x80,
            READ_VCOM_VALUE = 0x81,
            VCM_DC_SETTING = 0x82,
            PARTIAL_WINDOW = 0x90,
            PARTIAL_IN = 0x91,
            PARTIAL_OUT = 0x92,
            PROGRAM_MODE = 0xA0,
            ACTIVE_PROGRAM = 0xA1,
            READ_OTP_DATA = 0xA2,
            POWER_SAVING = 0xE3,
        }
    }
}