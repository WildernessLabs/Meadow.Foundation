using System;

namespace Meadow.Foundation.Graphics.Buffers
{
    /// <summary>
    /// Create matching Buffer type
    /// </summary>
    public static class BufferFactory
    {
        /// <summary>
        /// Create Compatible Buffer
        /// </summary>
        /// <param name="target">buffer to match</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns>new buffer</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static IPixelBuffer CreateCompatible(PixelBufferBase target, int width, int height)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (target.GetType() == typeof(Buffer1bpp))
                return new Buffer1bpp(width, height);
            else if (target.GetType() == typeof(Buffer1bppV))
                return new Buffer1bppV(width, height);
            else if (target.GetType() == typeof(BufferGray4))
                return new BufferGray4(width, height);
            else if (target.GetType() == typeof(BufferGray8))
                return new BufferGray8(width, height);
            else if (target.GetType() == typeof(BufferIndexed4))
                return new BufferIndexed4(width, height);
            else if (target.GetType() == typeof(BufferRgb332))
                return new BufferRgb332(width, height);
            else if (target.GetType() == typeof(BufferRgb444))
                return new BufferRgb444(width, height);
            else if (target.GetType() == typeof(BufferRgb565))
                return new BufferRgb565(width, height);
            else if (target.GetType() == typeof(BufferRgb888))
                return new BufferRgb888(width, height);
            else if (target.GetType() == typeof(BufferRgba8888))
                return new BufferRgba8888(width, height);
            else
                throw new ArgumentException($"Unknown Buffer type {target.GetType()}", nameof(target));
        }
    }
}

