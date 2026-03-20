using Meadow;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Peripherals.Displays;
using System;

namespace Graphics.MicroGraphics.Dither
{
    public static partial class PixelBufferDither
    {
        private static readonly byte[,] BAYER4_12bpp = new byte[,]
        {
            { 0, 8, 2, 10 },
            { 12, 4, 14, 6 },
            { 3, 11, 1, 9 },
            { 15, 7, 13, 5 }
        };

        /// <summary>
        /// Convert any IPixelBuffer to a new BufferRgb444 with optional dithering.
        /// </summary>
        public static BufferRgb444 ToRgb444(
            IPixelBuffer sourceBuffer,
            DitherMode mode,
            bool serpentine = true)
        {
            if (sourceBuffer is null)
            {
                throw new ArgumentNullException(nameof(sourceBuffer));
            }

            var ditheredBuffer = new BufferRgb444(sourceBuffer.Width, sourceBuffer.Height);

            switch (mode)
            {
                case DitherMode.Ordered4x4:
                    ConvertOrdered4x4_Rgb444(sourceBuffer, ditheredBuffer);
                    break;
                case DitherMode.FloydSteinberg:
                    ConvertFloydSteinberg_Rgb444(sourceBuffer, ditheredBuffer, serpentine);
                    break;
            }

            return ditheredBuffer;
        }

        private static void ConvertOrdered4x4_Rgb444(IPixelBuffer sourceBuffer, BufferRgb444 destinationBuffer)
        {
            for (int y = 0; y < destinationBuffer.Height; y++)
            {
                int yMatrixIndex = y & 3;
                for (int x = 0; x < destinationBuffer.Width; x++)
                {
                    var sourceColor = sourceBuffer.GetPixel(x, y);

                    int threshold = BAYER4_12bpp[yMatrixIndex, x & 3] * 16;

                    int rRemainder = sourceColor.R & 0x0F;
                    int gRemainder = sourceColor.G & 0x0F;
                    int bRemainder = sourceColor.B & 0x0F;

                    int r4 = sourceColor.R >> 4;
                    int g4 = sourceColor.G >> 4;
                    int b4 = sourceColor.B >> 4;

                    if (rRemainder > threshold) r4++;
                    if (gRemainder > threshold) g4++;
                    if (bRemainder > threshold) b4++;

                    byte finalR = (byte)(Math.Min(r4, 15) << 4);
                    byte finalG = (byte)(Math.Min(g4, 15) << 4);
                    byte finalB = (byte)(Math.Min(b4, 15) << 4);

                    destinationBuffer.SetPixel(x, y, new Color(finalR, finalG, finalB));
                }
            }
        }

        private static void ConvertFloydSteinberg_Rgb444(IPixelBuffer sourceBuffer, BufferRgb444 destinationBuffer, bool serpentine)
        {
            int width = destinationBuffer.Width;
            int height = destinationBuffer.Height;

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
                        DitherPixelAndDiffuseError(x, y, 1);
                    }
                }
                else
                {
                    for (int x = width - 1; x >= 0; x--)
                    {
                        DitherPixelAndDiffuseError(x, y, -1);
                    }
                }

                (currentRowErrorR, nextRowErrorR) = (nextRowErrorR, currentRowErrorR);
                (currentRowErrorG, nextRowErrorG) = (nextRowErrorG, currentRowErrorG);
                (currentRowErrorB, nextRowErrorB) = (nextRowErrorB, currentRowErrorB);

                Array.Clear(nextRowErrorR, 0, nextRowErrorR.Length);
                Array.Clear(nextRowErrorG, 0, nextRowErrorG.Length);
                Array.Clear(nextRowErrorB, 0, nextRowErrorB.Length);
            }

            void DitherPixelAndDiffuseError(int x, int y, int direction)
            {
                int errorArrayIndex = x + 1;
                var sourceColor = sourceBuffer.GetPixel(x, y);

                // Add the accumulated fixed-point error from previous pixels
                int adjustedR = ClampByte(sourceColor.R + (currentRowErrorR[errorArrayIndex] >> 8));
                int adjustedG = ClampByte(sourceColor.G + (currentRowErrorG[errorArrayIndex] >> 8));
                int adjustedB = ClampByte(sourceColor.B + (currentRowErrorB[errorArrayIndex] >> 8));

                byte quantizedR = (byte)((adjustedR >> 4) << 4);
                byte quantizedG = (byte)((adjustedG >> 4) << 4);
                byte quantizedB = (byte)((adjustedB >> 4) << 4);

                destinationBuffer.SetPixel(x, y, new Color(quantizedR, quantizedG, quantizedB));

                int errorR = (adjustedR - quantizedR);
                int errorG = (adjustedG - quantizedG);
                int errorB = (adjustedB - quantizedB);

                if (direction > 0)
                {
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
                else
                {
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
}