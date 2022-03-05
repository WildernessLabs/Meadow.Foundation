/// <summary>
/// ENUMs for Neopixels connected to Adafruit Seesaw
/// Author: Frederick M Meyer
/// Date: 2022-03-03
/// Copyright: 2022 (c) Frederick M Meyer for Wilderness Labs
/// License: MIT
/// </summary>
/// <remarks>
/// For hardware, this works with either Seesaw device:
/// Adafruit ATSAMD09 Breakout with seesaw <see href="https://www.adafruit.com/product/3657"</see>
/// -or-
/// Adafruit ATtiny8x7 Breakout with seesaw - STEMMA QT / Qwiic <see href="https://www.adafruit.com/product/5233"</see>
/// </remarks>

namespace Meadow.Foundation.ICs.IOExpanders.Seesaw
{
    public partial class Neopixel
    {
        /// <summary>
        /// Output color neopixel hardware byte order from PixelArray (rr gg bb ww) using PixelOrder
        /// </summary>

        // Red Green Blue
        public static byte[] RGB = new byte[] { 0, 1, 2 };

        // Green Red Blue
        public static byte[] GRB = new byte[] { 1, 0, 2 };
            
        // Red Green Blue White
        public static byte[] RGBW = new byte[] { 0, 1, 2, 3 };
            
        // Green Red Blue White
        public static byte[] GRBW = new byte[] { 1, 0, 2, 3 };
    }
}
