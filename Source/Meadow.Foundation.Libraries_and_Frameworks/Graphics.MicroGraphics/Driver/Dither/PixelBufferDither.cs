using Meadow;
using System;

namespace Graphics.MicroGraphics.Dither
{
    /// <summary>
    /// Provides methods for converting pixel buffers to 4bpp indexed buffers with optional dithering.
    /// </summary>
    public static partial class PixelBufferDither
    {
        static readonly byte[,] BAYER4 = new byte[,]
        {
            {   0,128, 32,160 },
            { 192, 64,224, 96 },
            {  48,176, 16,144 },
            { 240,112,208, 80 }
        };

        static int GetColorDistance(in Color color1, in Color color2)
        {
            if (color1 == color2)
            {
                return 0;
            }

            int rDelta = color1.R - color2.R;
            int gDelta = color1.G - color2.G;
            int bDelta = color1.B - color2.B;

            return (rDelta * rDelta) + (gDelta * gDelta) + (bDelta * bDelta);
        }

        static byte NearestIndex(in Color c, Color[] palette, int paletteCount)
        {
            int best = 0, bestD = int.MaxValue;
            for (int i = 0; i < paletteCount; i++)
            {
                int d = GetColorDistance(c, palette[i]);
                if (d < bestD) { bestD = d; best = i; if (bestD == 0) break; }
            }
            return (byte)best;
        }

        static byte ClampByte(int v) => (byte)Math.Clamp(v, 0, 255);
    }
}