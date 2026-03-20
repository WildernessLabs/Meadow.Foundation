using Meadow;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Peripherals.Displays;
using System;

namespace Graphics.MicroGraphics.Dither
{
    public static partial class PixelBufferDither
    {
        /// <summary>
        /// Convert any IPixelBuffer to a new BufferIndexed4 using a provided palette and dithering.
        /// </summary>
        public static BufferIndexed4 ToIndexed4(
            IPixelBuffer sourceBuffer,
            Color[] palette,
            DitherMode mode,
            bool serpentine = true)
        {
            if (sourceBuffer is null)
            {
                throw new ArgumentNullException(nameof(sourceBuffer));
            }
            if (palette is null || palette.Length == 0)
            {
                throw new ArgumentException("Palette must contain at least 2 colors.", nameof(palette));
            }
            if (palette.Length > 16)
            {
                throw new ArgumentException("Palette cannot exceed 16 entries for 4bpp buffers.", nameof(palette));
            }

            var ditheredBuffer = new BufferIndexed4(sourceBuffer.Width, sourceBuffer.Height);

            int slots = Math.Min(ditheredBuffer.IndexedColors.Length, palette.Length);

            for (int i = 0; i < slots; i++)
            {
                ditheredBuffer.IndexedColors[i] = palette[i];
            }

            switch (mode)
            {
                case DitherMode.Ordered4x4:
                    ConvertOrdered4x4(sourceBuffer, ditheredBuffer);
                    break;
                case DitherMode.FloydSteinberg:
                    ConvertFloydSteinberg(sourceBuffer, ditheredBuffer, serpentine);
                    break;
            }

            return ditheredBuffer;
        }

        static void ConvertOrdered4x4(IPixelBuffer sourceBuffer, BufferIndexed4 destinationBuffer)
        {
            var palette = destinationBuffer.IndexedColors;
            int paletteLength = palette.Length;

            for (int y = 0; y < destinationBuffer.Height; y++)
            {
                int yMatrixIndex = y & 3;
                for (int x = 0; x < destinationBuffer.Width; x++)
                {
                    var sourceColor = sourceBuffer.GetPixel(x, y);
                    int threshold = BAYER4[yMatrixIndex, x & 3] - 128;

                    var n = new Color(
                        ClampByte(sourceColor.R + threshold),
                        ClampByte(sourceColor.G + threshold),
                        ClampByte(sourceColor.B + threshold));

                    byte paletteIndex = NearestIndex(n, palette, paletteLength);
                    destinationBuffer.SetPixel(x, y, paletteIndex);
                }
            }
        }

        static void ConvertFloydSteinberg(IPixelBuffer sourceBuffer, BufferIndexed4 destinationBuffer, bool serpentine)
        {
            int width = destinationBuffer.Width;
            int height = destinationBuffer.Height;
            var palette = destinationBuffer.IndexedColors;
            int paletteCount = palette.Length;

            var rowErrorR = new int[width + 2];
            var rowErrorG = new int[width + 2];
            var rowErrorB = new int[width + 2];
            var nextRowErrorR = new int[width + 2];
            var nextRowErrorG = new int[width + 2];
            var nextRowErrorB = new int[width + 2];

            for (int y = 0; y < height; y++)
            {
                if (!serpentine || (y & 1) == 0)
                {
                    for (int x = 0; x < width; x++)
                    {
                        DitherPixelAndDiffuseError_4bpp(x, y);
                    }
                }
                else
                {
                    for (int x = width - 1; x >= 0; x--)
                    {
                        DitherPixelAndDiffuseErrorReversed_4bpp(x, y);
                    }
                }

                (rowErrorR, nextRowErrorR) = (nextRowErrorR, rowErrorR);
                Array.Clear(nextRowErrorR, 0, nextRowErrorR.Length);
                (rowErrorG, nextRowErrorG) = (nextRowErrorG, rowErrorG);
                Array.Clear(nextRowErrorG, 0, nextRowErrorG.Length);
                (rowErrorB, nextRowErrorB) = (nextRowErrorB, rowErrorB);
                Array.Clear(nextRowErrorB, 0, nextRowErrorB.Length);
            }

            void DitherPixelAndDiffuseError_4bpp(int x, int y)
            {
                int errorArrayIndex = x + 1;
                var sourceColor = sourceBuffer.GetPixel(x, y);

                int r = ClampByte(sourceColor.R + ((rowErrorR[errorArrayIndex]) >> 8));
                int g = ClampByte(sourceColor.G + ((rowErrorG[errorArrayIndex]) >> 8));
                int b = ClampByte(sourceColor.B + ((rowErrorB[errorArrayIndex]) >> 8));

                var adjustedColor = new Color((byte)r, (byte)g, (byte)b);
                byte paletteIndex = NearestIndex(adjustedColor, palette, paletteCount);
                var quantizedColor = palette[paletteIndex];

                destinationBuffer.SetPixel(x, y, paletteIndex);

                int errorR = (r - quantizedColor.R) << 8;
                int errorG = (g - quantizedColor.G) << 8;
                int errorB = (b - quantizedColor.B) << 8;

                rowErrorR[errorArrayIndex + 1] += (7 * errorR) >> 4;
                rowErrorG[errorArrayIndex + 1] += (7 * errorG) >> 4;
                rowErrorB[errorArrayIndex + 1] += (7 * errorB) >> 4;

                nextRowErrorR[errorArrayIndex - 1] += (3 * errorR) >> 4;
                nextRowErrorG[errorArrayIndex - 1] += (3 * errorG) >> 4;
                nextRowErrorB[errorArrayIndex - 1] += (3 * errorB) >> 4;
                nextRowErrorR[errorArrayIndex + 0] += (5 * errorR) >> 4;
                nextRowErrorG[errorArrayIndex + 0] += (5 * errorG) >> 4;
                nextRowErrorB[errorArrayIndex + 0] += (5 * errorB) >> 4;
                nextRowErrorR[errorArrayIndex + 1] += (1 * errorR) >> 4;
                nextRowErrorG[errorArrayIndex + 1] += (1 * errorG) >> 4;
                nextRowErrorB[errorArrayIndex + 1] += (1 * errorB) >> 4;
            }

            void DitherPixelAndDiffuseErrorReversed_4bpp(int x, int y)
            {
                int errorArrayIndex = x + 1;
                var s = sourceBuffer.GetPixel(x, y);

                int r = ClampByte(s.R + ((rowErrorR[errorArrayIndex]) >> 8));
                int g = ClampByte(s.G + ((rowErrorG[errorArrayIndex]) >> 8));
                int b = ClampByte(s.B + ((rowErrorB[errorArrayIndex]) >> 8));

                var adjustedColor = new Color((byte)r, (byte)g, (byte)b);
                byte paletteIndex = NearestIndex(adjustedColor, palette, paletteCount);
                var quantizedColor = palette[paletteIndex];

                destinationBuffer.SetPixel(x, y, paletteIndex);

                int er = (r - quantizedColor.R) << 8;
                int eg = (g - quantizedColor.G) << 8;
                int eb = (b - quantizedColor.B) << 8;

                rowErrorR[errorArrayIndex - 1] += (7 * er) >> 4;
                rowErrorG[errorArrayIndex - 1] += (7 * eg) >> 4;
                rowErrorB[errorArrayIndex - 1] += (7 * eb) >> 4;

                nextRowErrorR[errorArrayIndex + 1] += (3 * er) >> 4;
                nextRowErrorG[errorArrayIndex + 1] += (3 * eg) >> 4;
                nextRowErrorB[errorArrayIndex + 1] += (3 * eb) >> 4;
                nextRowErrorR[errorArrayIndex + 0] += (5 * er) >> 4;
                nextRowErrorG[errorArrayIndex + 0] += (5 * eg) >> 4;
                nextRowErrorB[errorArrayIndex + 0] += (5 * eb) >> 4;
                nextRowErrorR[errorArrayIndex - 1] += (1 * er) >> 4;
                nextRowErrorG[errorArrayIndex - 1] += (1 * eg) >> 4;
                nextRowErrorB[errorArrayIndex - 1] += (1 * eb) >> 4;
            }
        }
    }
}