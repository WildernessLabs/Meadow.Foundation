using Meadow.Foundation.Graphics.Buffers;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.mikroBUS.Displays
{
    /// <summary>
    /// Represents a mikroBUS 16x9 Click board
    /// </summary>
    public class C16x9 : IPixelDisplay
    {
        /// <summary>
        /// Is31fl3731 object to manage the leds
        /// </summary>
        readonly Is31fl3731 iS31FL3731;

        readonly IDigitalOutputPort onOffPort;

        /// <summary>
        /// The buffer to hold the display data
        /// </summary>
        public IPixelBuffer PixelBuffer { get; protected set; }

        /// <summary>
        /// Color mode of display
        /// </summary>
        public ColorMode ColorMode => ColorMode.Format8bppGray;

        /// <summary>
        /// Color mode of display
        /// </summary>
        public ColorMode SupportedColorModes => ColorMode.Format8bppGray;

        /// <summary>
        /// Width of display in pixels
        /// </summary>
        public int Width => 16;

        /// <summary>
        /// Height of display in pixels
        /// </summary>
        public int Height => 9;

        /// <summary>
        /// The Is31fl3731 active frame 
        /// </summary>
        byte frame;

        /// <summary>
        /// Gets/Sets property to ignore boundaries when drawing outside of the LED matrix
        /// </summary>
        public bool IgnoreOutOfBoundsPixels { get; set; }

        /// <summary>
        /// Turn the display on or off
        /// </summary>
        public bool DisplayOn
        {
            get => onOffPort.State;
            set => onOffPort.State = value;
        }

        /// <summary>
        /// Creates a CharlieWing driver
        /// </summary>
        /// <param name="onOffPort">On/Off port</param>
        /// <param name="i2cBus">I2C bus</param>
        /// <param name="address">I2C address</param>
        public C16x9(IDigitalOutputPort onOffPort, II2cBus i2cBus, byte address = (byte)Is31fl3731.Addresses.Default)
        {
            frame = 0;

            this.onOffPort = onOffPort;

            iS31FL3731 = new Is31fl3731(i2cBus, address);
            iS31FL3731.Initialize();

            PixelBuffer = new BufferGray8(Width, Height);

            //enable the display
            this.onOffPort.State = true;

            //clear the display
            for (byte i = 0; i <= 7; i++)
            {
                iS31FL3731.SetLedState(i, true);
                iS31FL3731.Clear(i);
            }
        }

        /// <summary>
        /// Creates a CharlieWing driver
        /// </summary>
        /// <param name="onOffPin">IO pin to controller display on/off state</param>
        /// <param name="i2cBus">I2C bus</param>
        public C16x9(IPin onOffPin, II2cBus i2cBus) :
            this(onOffPin.CreateDigitalOutputPort(), i2cBus, (byte)Is31fl3731.Addresses.Default)
        {
        }

        /// <summary>
        /// Clear display
        /// </summary>
        /// <param name="updateDisplay"></param>
        public void Clear(bool updateDisplay = false)
        {
            PixelBuffer.Clear();

            if (updateDisplay == true)
            {
                Show();
            }
        }

        /// <summary>
        /// Turn on an RGB LED with the specified color on (x,y) coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        public void DrawPixel(int x, int y, Color color)
        {
            if (IgnoreOutOfBoundsPixels)
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                { return; }
            }

            PixelBuffer.SetPixel(x, y, color);
        }

        /// <summary>
        /// Turn on a LED on (x,y) coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="colored"></param>
        public void DrawPixel(int x, int y, bool colored)
        {
            DrawPixel(x, y, colored ? Color.White : Color.Black);
        }

        /// <summary>
        /// Invert the color of the pixel at the given location
        /// </summary>
        /// <param name="x">x location in pixels</param>
        /// <param name="y">y location in pixels</param>
        public void InvertPixel(int x, int y)
        {
            var brightness = 255 - PixelBuffer.GetPixel(x, y).Color8bppGray;

            PixelBuffer.SetPixel(x, y, new Color(brightness, brightness, brightness));
        }

        /// <summary>
        /// Draw a buffer to the display
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="displayBuffer"></param>
        public void DrawBuffer(int x, int y, IPixelBuffer displayBuffer)
        {
            this.PixelBuffer.WriteBuffer(x, y, displayBuffer);
        }

        /// <summary>
        /// Clear the display.
        /// </summary>
        /// <param name="clearColor"></param>
        /// <param name="updateDisplay"></param>
        public void Fill(Color clearColor, bool updateDisplay = false)
        {
            PixelBuffer.Fill(clearColor);

            if (updateDisplay)
            {
                Show();
            }
        }

        /// <summary>
        /// Clear the display
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="fillColor"></param>
        public void Fill(int x, int y, int width, int height, Color fillColor)
        {
            PixelBuffer.Fill(x, y, width, height, fillColor);
        }

        /// <summary>
        /// Show changes on the display
        /// </summary>
        public void Show()
        {
            //we'll swap frames every update
            frame = (byte)((frame == 0) ? 1 : 0);

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    iS31FL3731.SetLedPwm(frame, (byte)(x + y * Width), PixelBuffer.GetPixel(x, y).Color8bppGray);
                }
            }

            iS31FL3731.DisplayFrame(frame);
        }

        /// <summary>
        /// Show changes on the display
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="right"></param>
        /// <param name="bottom"></param>
        public void Show(int left, int top, int right, int bottom)
        {
            Show();
        }

        /// <summary>
        /// Show changes on the display
        /// </summary>
        /// <param name="frame"></param>
        public void Show(byte frame)
        {
            iS31FL3731.DisplayFrame(frame);
        }

        /// <summary>
        /// Write an external buffer to the display buffer
        /// </summary>
        /// <param name="x">X postion to write buffer in pixels</param>
        /// <param name="y">Y postion to write buffer in pixels</param>
        /// <param name="buffer">The buffer to write</param>
        public void WriteBuffer(int x, int y, IPixelBuffer buffer)
        {
            PixelBuffer.WriteBuffer(x, y, buffer);
        }
    }
}