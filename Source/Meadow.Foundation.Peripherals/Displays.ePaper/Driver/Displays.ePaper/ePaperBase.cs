using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Displays.ePaper
{
    /// <summary>
    ///     Provide an interface for ePaper monochrome displays
    /// </summary>
    public abstract class EpdBase : SpiDisplayBase
    {
        public override DisplayColorMode ColorMode => DisplayColorMode.Format1bpp;

        protected readonly byte[] imageBuffer;

        int xRefreshStart, yRefreshStart, xRefreshEnd, yRefreshEnd;

        public override int Width { get; }
        public override int Height { get; }

        private EpdBase()
        { }

        public EpdBase(IIODevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, IPin busyPin,
            int width, int height)
        {
            Width = width;
            Height = height;

            dataCommandPort = device.CreateDigitalOutputPort(dcPin, false);
            resetPort = device.CreateDigitalOutputPort(resetPin, true);
            busyPort = device.CreateDigitalInputPort(busyPin);

            spi = new SpiPeripheral(spiBus, device.CreateDigitalOutputPort(chipSelectPin));

            imageBuffer = new byte[Width * Height / 8];

            for (int i = 0; i < Width * Height / 8; i++)
            {
                imageBuffer[i] = 0xff;
            }

            Initialize();
        }

        protected abstract void Initialize();

        public override void Clear(bool updateDisplay = false)
        {
            Clear(false, updateDisplay);
        }

        /// <summary>
        ///     Clear the display.
        /// </summary>
        /// <param name="color">Color to set the display (not used on ePaper displays)</param>
        /// <param name="updateDisplay">Update the dipslay once the buffer has been cleared when true.</param>
        public void Clear(Color color, bool updateDisplay = false)
        {
            bool colored = false;
            
            if (color.B > 0 || color.R > 0 || color.G > 0)
            {
                colored = true;
            }

            Clear(colored, updateDisplay);
        }

        /// <summary>
        ///     Clear the display.
        /// </summary>
        /// <param name="colored">Set the display dark when true</param>
        /// <param name="updateDisplay">Update the dipslay once the buffer has been cleared when true.</param>
        public void Clear(bool colored, bool updateDisplay = false)
        {
            //   ClearFrameMemory((byte)(colored ? 0 : 0xFF));
            //   DisplayFrame();

            for (int i = 0; i < imageBuffer.Length; i++)
            {
                imageBuffer[i] = colored ? (byte)0 : (byte)255;
            }

            if (updateDisplay)
            {
                Refresh();
            }
        }

        /// <summary>
        ///     Draw a single pixel 
        /// </summary>
        /// <param name="x">x location </param>
        /// <param name="y">y location</param>
        /// <param name="colored">Turn the pixel on (true) or off (false).</param>
        public override void DrawPixel(int x, int y, bool colored)
        {
            xRefreshStart = Math.Min(x, xRefreshStart);
            xRefreshEnd = Math.Max(x, xRefreshEnd);
            yRefreshStart = Math.Min(y, yRefreshStart);
            yRefreshEnd = Math.Max(y, yRefreshEnd);

            if (colored)
            {
                imageBuffer[(x + y * Width) / 8] &= (byte)~(0x80 >> (x % 8));
            }
            else
            {
                imageBuffer[(x + y * Width) / 8] |= (byte)(0x80 >> (x % 8));
            }
        }

        public override void DrawPixel(int x, int y)
        {
            DrawPixel(x, y, currentPen);
        }

        /// <summary>
        ///     Draw a single pixel 
        /// </summary>
        /// <param name="x">x location </param>
        /// <param name="y">y location</param>
        /// <param name="color">Color of pixel.</param>
        public override void DrawPixel(int x, int y, Color color)
        {
            bool colored = false;
            if (color.B > 0 || color.G > 0 || color.R > 0)
            {
                colored = true;
            }

            DrawPixel(x, y, colored);
        }

        public override void InvertPixel(int x, int y)
        {
            xRefreshStart = Math.Min(x, xRefreshStart);
            xRefreshEnd = Math.Max(x, xRefreshEnd);
            yRefreshStart = Math.Min(y, yRefreshStart);
            yRefreshEnd = Math.Max(y, yRefreshEnd);

            imageBuffer[(x + y * Width) / 8] ^= (byte)(0x80 >> (x % 8));
        }

        /// <summary>
        ///     Draw a single pixel 
        /// </summary>
        /// <param name="x">x location </param>
        /// <param name="y">y location</param>
        /// <param name="r">y location</param>
        /// <param name="g">y location</param>
        /// <param name="b">y location</param>
        public void DrawPixel(int x, int y, byte r, byte g, byte b)
        {
            bool colored = false;
            if (r > 0 || g > 0 || b > 0)
            {
                colored = true;
            }

            DrawPixel(x, y, colored);
        }

        /// <summary>
        ///     Draw the display buffer to screen
        /// </summary>
        public void Refresh()
        {
            if (xRefreshStart == -1)
            {
                SetFrameMemory(imageBuffer);
            }
            else
            {
                SetFrameMemory(imageBuffer, xRefreshStart, yRefreshStart, xRefreshEnd - xRefreshStart, yRefreshEnd - yRefreshStart);
            }

            DisplayFrame();

            xRefreshStart = yRefreshStart = xRefreshEnd = yRefreshEnd = -1;
        }

        /// <summary>
        ///     Draw the display buffer to screen
        /// </summary>
        public override void Show()
        {
            Refresh();
        }

        public virtual void SetFrameMemory(byte[] image_buffer,
                                int x,
                                int y,
                                int image_width,
                                int image_height)
        {
            int x_end;
            int y_end;

            if (image_buffer == null ||
                x < 0 || image_width < 0 ||
                y < 0 || image_height < 0)
            {
                return;
            }

            /* x point must be the multiple of 8 or the last 3 bits will be ignored */
            x &= 0xF8;
            image_width &= 0xF8;
            if (x + image_width >= Width)
            {
                x_end = (int)Width - 1;
            }
            else
            {
                x_end = x + image_width - 1;
            }

            if (y + image_height >= Height)
            {
                y_end = (int)Height - 1;
            }
            else
            {
                y_end = y + image_height - 1;
            }

            SetMemoryArea(x, y, x_end, y_end);
            SetMemoryPointer(x, y);
            SendCommand(WRITE_RAM);
            /* send the image data */
            for (int j = 0; j < y_end - y + 1; j++)
            {
                for (int i = 0; i < (x_end - x + 1) / 8; i++)
                {
                    SendData(image_buffer[i + j * (image_width / 8)]);
                }
            }
        }

        public virtual void SetFrameMemory(byte[] image_buffer)
        {
            SetMemoryArea(0, 0, (int)Width - 1, (int)Height - 1);
            SetMemoryPointer(0, 0);
            SendCommand(WRITE_RAM);
            /* send the image data */
            for (int i = 0; i < Width / 8 * Height; i++)
            {
                SendData(image_buffer[i]);
            }
        }

        public virtual void ClearFrameMemory(byte color)
        {
            SetMemoryArea(0, 0, (int)Width - 1, (int)Height - 1);
            SetMemoryPointer(0, 0);
            SendCommand(WRITE_RAM);
            /* send the color data */
            for (int i = 0; i < Width / 8 * Height; i++)
            {
                SendData(color);
            }
        }

        public virtual void DisplayFrame()
        {
            SendCommand(DISPLAY_UPDATE_CONTROL_2);
            SendData(0xC4);
            SendCommand(MASTER_ACTIVATION);
            SendCommand(TERMINATE_FRAME_READ_WRITE);
            WaitUntilIdle();
        }

        void SetMemoryArea(int x_start, int y_start, int x_end, int y_end)
        {
            SendCommand(SET_RAM_X_ADDRESS_START_END_POSITION);
            /* x point must be the multiple of 8 or the last 3 bits will be ignored */
            SendData((x_start >> 3) & 0xFF);
            SendData((x_end >> 3) & 0xFF);
            SendCommand(SET_RAM_Y_ADDRESS_START_END_POSITION);
            SendData(y_start & 0xFF);
            SendData((y_start >> 8) & 0xFF);
            SendData(y_end & 0xFF);
            SendData((y_end >> 8) & 0xFF);
        }

        void SetMemoryPointer(int x, int y)
        {
            SendCommand(SET_RAM_X_ADDRESS_COUNTER);
            /* x point must be the multiple of 8 or the last 3 bits will be ignored */
            SendData((x >> 3) & 0xFF);
            SendCommand(SET_RAM_Y_ADDRESS_COUNTER);
            SendData(y & 0xFF);
            SendData((y >> 8) & 0xFF);
            WaitUntilIdle();
        }

        protected void Sleep()
        {
            SendCommand(DEEP_SLEEP_MODE);
            WaitUntilIdle();
        }

        // EPD1IN54 commands
        protected static byte DRIVER_OUTPUT_CONTROL = 0x01;
        protected static byte BOOSTER_SOFT_START_CONTROL = 0x0C;
        protected static byte GATE_SCAN_START_POSITION = 0x0F;
        protected static byte DEEP_SLEEP_MODE = 0x10;
        protected static byte DATA_ENTRY_MODE_SETTING = 0x11;
        protected static byte SW_RESET = 0x12;
        protected static byte TEMPERATURE_SENSOR_CONTROL = 0x1A;
        protected static byte MASTER_ACTIVATION = 0x20;
        protected static byte DISPLAY_UPDATE_CONTROL_1 = 0x21;
        protected static byte DISPLAY_UPDATE_CONTROL_2 = 0x22;
        protected static byte WRITE_RAM = 0x24;
        protected static byte WRITE_VCOM_REGISTER = 0x2C;
        protected static byte WRITE_LUT_REGISTER = 0x32;
        protected static byte SET_DUMMY_LINE_PERIOD = 0x3A;
        protected static byte SET_GATE_TIME = 0x3B;
        protected static byte BORDER_WAVEFORM_CONTROL = 0x3C;
        protected static byte SET_RAM_X_ADDRESS_START_END_POSITION = 0x44;
        protected static byte SET_RAM_Y_ADDRESS_START_END_POSITION = 0x45;
        protected static byte SET_RAM_X_ADDRESS_COUNTER = 0x4E;
        protected static byte SET_RAM_Y_ADDRESS_COUNTER = 0x4F;
        protected static byte TERMINATE_FRAME_READ_WRITE = 0xFF;
    }
}