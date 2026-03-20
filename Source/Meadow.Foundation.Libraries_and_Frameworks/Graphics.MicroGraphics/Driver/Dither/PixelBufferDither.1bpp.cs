using Meadow;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Peripherals.Displays;
using System;

namespace Graphics.MicroGraphics.Dither
{
    public static partial class PixelBufferDither
    {
        /// <summary>
        /// Provides methods for converting pixel buffers to single-color 1bpp buffers with optional dithering.
        /// </summary>
        public static Buffer1bpp To1bpp(
            IPixelBuffer sourceBuffer,
            Color color1,
            Color color2,
            DitherMode mode,
            bool serpentine = true)
        {
            if (sourceBuffer is null)
            {
                throw new ArgumentNullException(nameof(sourceBuffer));
            }

            var ditheredBuffer = new Buffer1bpp(sourceBuffer.Width, sourceBuffer.Height);

            // Create a 2-color palette for the NearestIndex method
            var palette = new Color[] { color1, color2 };

            switch (mode)
            {
                case DitherMode.Ordered4x4:
                    ConvertOrdered4x4_1bpp(sourceBuffer, ditheredBuffer, palette);
                    break;
                case DitherMode.FloydSteinberg:
                    ConvertFloydSteinberg_1bpp(sourceBuffer, ditheredBuffer, palette, serpentine);
                    break;
            }

            return ditheredBuffer;
        }

        static void ConvertOrdered4x4_1bpp(IPixelBuffer sourceBuffer, Buffer1bpp destinationBuffer, Color[] palette)
        {
            int paletteCount = palette.Length;
            for (int y = 0; y < destinationBuffer.Height; y++)
            {
                int yMatrixIndex = y & 3;
                for (int x = 0; x < destinationBuffer.Width; x++)
                {
                    var sourceColor = sourceBuffer.GetPixel(x, y);
                    int threshold = BAYER4[yMatrixIndex, x & 3] - 128;

                    var adjustedColor = new Color(
                        ClampByte(sourceColor.R + threshold),
                        ClampByte(sourceColor.G + threshold),
                        ClampByte(sourceColor.B + threshold));

                    byte paletteIndex = NearestIndex(adjustedColor, palette, paletteCount);
                    destinationBuffer.SetPixel(x, y, (bool)(paletteIndex > 0));
                }
            }
        }

        static void ConvertFloydSteinberg_1bpp(
            IPixelBuffer sourceBuffer,
            Buffer1bpp destinationBuffer,
            Color[] palette,
            bool serpentine)
        {
            int width = destinationBuffer.Width;
            int height = destinationBuffer.Height;
            int paletteCount = palette.Length;

            var currentRowErrorR = new int[width + 2];
            var currentRowErrorG = new int[width + 2];
            var currentRowErrorB = new int[width + 2];
            var nextRowErrorR = new int[width + 2];
            var nextRowErrorG = new int[width + 2];
            var nextRowErrorB = new int[width + 2];

            for (int y = 0; y < height; y++)
            {
                if (!serpentine || (y & 1) == 0)
                {
                    for (int x = 0; x < width; x++)
                    {
                        DitherPixelAndDiffuseError_1bpp(x, y, palette);
                    }
                }
                else
                {
                    for (int x = width - 1; x >= 0; x--)
                    {
                        DitherPixelAndDiffuseErrorReversed_1bpp(x, y, palette);
                    }
                }

                (currentRowErrorR, nextRowErrorR) = (nextRowErrorR, currentRowErrorR);
                (currentRowErrorG, nextRowErrorG) = (nextRowErrorG, currentRowErrorG);
                (currentRowErrorB, nextRowErrorB) = (nextRowErrorB, currentRowErrorB);

                Array.Clear(nextRowErrorR, 0, nextRowErrorR.Length);
                Array.Clear(nextRowErrorG, 0, nextRowErrorG.Length);
                Array.Clear(nextRowErrorB, 0, nextRowErrorB.Length);
            }

            void DitherPixelAndDiffuseError_1bpp(int x, int y, Color[] palette)
            {
                int errorArrayIndex = x + 1;
                var sourceColor = sourceBuffer.GetPixel(x, y);

                int adjustedR = ClampByte(sourceColor.R + ((currentRowErrorR[errorArrayIndex]) >> 8));
                int adjustedG = ClampByte(sourceColor.G + ((currentRowErrorG[errorArrayIndex]) >> 8));
                int adjustedB = ClampByte(sourceColor.B + ((currentRowErrorB[errorArrayIndex]) >> 8));

                var adjustedColor = new Color((byte)adjustedR, (byte)adjustedG, (byte)adjustedB);
                byte paletteIndex = NearestIndex(adjustedColor, palette, paletteCount);
                var quantizedColor = palette[paletteIndex];

                destinationBuffer.SetPixel(x, y, (bool)(paletteIndex > 0));

                int errorR = (adjustedR - quantizedColor.R) << 8;
                int errorG = (adjustedG - quantizedColor.G) << 8;
                int errorB = (adjustedB - quantizedColor.B) << 8;

                currentRowErrorR[errorArrayIndex + 1] += (7 * errorR) >> 4;
                currentRowErrorG[errorArrayIndex + 1] += (7 * errorG) >> 4;
                currentRowErrorB[errorArrayIndex + 1] += (7 * errorB) >> 4;

                nextRowErrorR[errorArrayIndex - 1] += (3 * errorR) >> 4;
                nextRowErrorG[errorArrayIndex - 1] += (3 * errorG) >> 4;
                nextRowErrorB[errorArrayIndex - 1] += (3 * errorB) >> 4;
                nextRowErrorR[errorArrayIndex] += (5 * errorR) >> 4;
                nextRowErrorG[errorArrayIndex] += (5 * errorG) >> 4;
                nextRowErrorB[errorArrayIndex] += (5 * errorB) >> 4;
                nextRowErrorR[errorArrayIndex + 1] += (1 * errorR) >> 4;
                nextRowErrorG[errorArrayIndex + 1] += (1 * errorG) >> 4;
                nextRowErrorB[errorArrayIndex + 1] += (1 * errorB) >> 4;
            }

            void DitherPixelAndDiffuseErrorReversed_1bpp(int x, int y, Color[] palette)
            {
                int errorArrayIndex = x + 1;
                var sourceColor = sourceBuffer.GetPixel(x, y);

                int adjustedR = ClampByte(sourceColor.R + ((currentRowErrorR[errorArrayIndex]) >> 8));
                int adjustedG = ClampByte(sourceColor.G + ((currentRowErrorG[errorArrayIndex]) >> 8));
                int adjustedB = ClampByte(sourceColor.B + ((currentRowErrorB[errorArrayIndex]) >> 8));

                var adjustedColor = new Color((byte)adjustedR, (byte)adjustedG, (byte)adjustedB);
                byte paletteIndex = NearestIndex(adjustedColor, palette, paletteCount);
                var quantizedColor = palette[paletteIndex];

                destinationBuffer.SetPixel(x, y, (bool)(paletteIndex > 0));

                int errorR = (adjustedR - quantizedColor.R) << 8;
                int errorG = (adjustedG - quantizedColor.G) << 8;
                int errorB = (adjustedB - quantizedColor.B) << 8;

                currentRowErrorR[errorArrayIndex - 1] += (7 * errorR) >> 4;
                currentRowErrorG[errorArrayIndex - 1] += (7 * errorG) >> 4;
                currentRowErrorB[errorArrayIndex - 1] += (7 * errorB) >> 4;

                nextRowErrorR[errorArrayIndex + 1] += (3 * errorR) >> 4;
                nextRowErrorG[errorArrayIndex + 1] += (3 * errorG) >> 4;
                nextRowErrorB[errorArrayIndex + 1] += (3 * errorB) >> 4;
                nextRowErrorR[errorArrayIndex] += (5 * errorR) >> 4;
                nextRowErrorG[errorArrayIndex] += (5 * errorG) >> 4;
                nextRowErrorB[errorArrayIndex] += (5 * errorB) >> 4;
                nextRowErrorR[errorArrayIndex - 1] += (1 * errorR) >> 4;
                nextRowErrorG[errorArrayIndex - 1] += (1 * errorG) >> 4;
                nextRowErrorB[errorArrayIndex - 1] += (1 * errorB) >> 4;
            }
        }
    }
}