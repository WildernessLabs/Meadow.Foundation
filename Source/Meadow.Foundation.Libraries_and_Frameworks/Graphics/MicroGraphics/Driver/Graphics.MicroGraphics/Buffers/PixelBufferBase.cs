using Meadow.Foundation;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using System;

namespace Meadow.Foundation.Graphics.Buffers
{
    public abstract class PixelBufferBase : IPixelBuffer
    {
        public int Width { get; protected set; }

        public int Height { get; protected set; }

        public virtual ColorType ColorMode { get; protected set; }

        public int BitDepth
        {
            get
            {
                switch (ColorMode)
                {
                    case ColorType.Format1bpp: return 1;
                    case ColorType.Format2bpp: return 2;
                    case ColorType.Format4bppGray: return 4;
                    case ColorType.Format8bppGray: return 8;
                    case ColorType.Format8bppRgb332: return 8;
                    case ColorType.Format12bppRgb444: return 12;
                    case ColorType.Format16bppRgb555: return 15;
                    case ColorType.Format16bppRgb565: return 16;
                    case ColorType.Format24bppRgb888: return 24;
                    case ColorType.Format32bppRgba8888: return 32;
                    default:
                        throw new NotImplementedException($"Unknown or unsupported ColorMode: {ColorMode}");
                        break;
                }
            }
        }

        public int ByteCount
        {
            get
            {
                return (Width * Height * BitDepth) / 8;
            }
        }

        // TODO: the default here should be true but all deriving classes
        // need to implement protection for out of bounds writes
        public bool IgnoreOutOfBounds { get; protected set; } = false;

        public byte[] Buffer { get; protected set; }



        public PixelBufferBase() { }

        public PixelBufferBase(int width, int height)
        {
            Width = width;
            Height = height;
            Buffer = new byte[this.ByteCount];
        }

        public PixelBufferBase(int width, int height, byte[] buffer)
        {
            Width = width;
            Height = height;
            if(buffer.Length != ByteCount)
            {
                throw new ArgumentException($"Provided buffer length ({buffer.Length}) does not match this buffer's ByteCount ({ByteCount}).");
            }
            Buffer = buffer;
        }



        public void Clear()
        {
            Array.Clear(Buffer, 0, Buffer.Length);
        }

        public abstract void Fill(Color color);
        
        public abstract void Fill(Color color, int originX, int originY, int width, int height);

        public abstract Color GetPixel(int x, int y);

        public abstract void SetPixel(int x, int y, Color color);

        public abstract void InvertPixel(int x, int y);

        /// <summary>
        /// Default way to write a buffer into this buffer.
        /// 
        /// This is very slow and should be avoided if possible.
        /// It loops through every pixel. If the buffer bit depths match
        /// there should be faster ways to write using Array.Copy.
        /// </summary>
        /// <param name="originX">The X coord to start writing</param>
        /// <param name="originY">The Y coord to start writing</param>
        /// <param name="buffer">The buffer to copy into this buffer</param>
        public virtual void WriteBuffer(int originX, int originY, IPixelBuffer buffer)
        {
            WriteBufferSlow(originX, originY, buffer);
        }




        /// <summary>
        /// A slow buffer write operation that writes pixel-by-pixel using
        /// the Color struct to scale color based on bit depth.
        /// 
        /// This method can handle buffers with mismatched bit depths.
        /// </summary>
        /// <param name="originX">The X coord to start writing the buffer</param>
        /// <param name="originY">The Y coord to start writing the buffer</param>
        /// <param name="buffer">The buffer to write</param>
        protected void WriteBufferSlow(int originX, int originY, IPixelBuffer buffer)
        {
            Color color;

            // ensure that we don't write beyond the bounds and have a range exception
            // if IgnoreOutOfBounds is true
            var xRange = IgnoreOutOfBounds ? Math.Min(Width, originX + buffer.Width) : buffer.Width;
            var yRange = IgnoreOutOfBounds ? Math.Min(Height, originY + buffer.Height) : buffer.Width;

            for (var x = 0; x < xRange; x++)
            {
                for(var y = 0; y < yRange; y++)
                {
                    color = buffer.GetPixel(x, y);
                    SetPixel(originX + x, originY + y, color);
                }
            }
        }
    }
}
