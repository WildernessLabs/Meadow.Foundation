using System;

namespace Meadow.Foundation.Graphics.Buffers
{
    public abstract class PixelBufferBase : IPixelBuffer
    {
        /// <summary>
        /// Width of buffer in pixels
        /// </summary>
        public int Width { get; protected set; }

        /// <summary>
        /// Height of buffer in pixels
        /// </summary>
        public int Height { get; protected set; }

        /// <summary>
        /// Color mode of the buffer
        /// </summary>
        public virtual ColorType ColorMode { get; protected set; }

        /// <summary>
        /// Bitdepth of display as an integer
        /// </summary>
        public int BitDepth
        {
            get
            {
                return ColorMode switch
                {
                    ColorType.Format1bpp => 1,
                    ColorType.Format2bpp => 2,
                    ColorType.Format4bppGray => 4,
                    ColorType.Format8bppGray => 8,
                    ColorType.Format8bppRgb332 => 8,
                    ColorType.Format12bppRgb444 => 12,
                    ColorType.Format16bppRgb555 => 15,
                    ColorType.Format16bppRgb565 => 16,
                    ColorType.Format18bppRgb666 => 18,
                    ColorType.Format24bppRgb888 => 24,
                    ColorType.Format32bppRgba8888 => 32,
                    _ => throw new NotImplementedException($"Unknown or unsupported ColorMode: {ColorMode}"),
                };
            }
        }

        /// <summary>
        /// Number of bytes in buffer
        /// </summary>
        public int ByteCount => (Width * Height * BitDepth) / 8;

        public bool IgnoreOutOfBounds { get; set; } = false;

        public byte[] Buffer { get; protected set; }


        public PixelBufferBase() { }

        public PixelBufferBase(int width, int height)
        {
            Width = width;
            Height = height;
            Buffer = new byte[ByteCount];
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
        
        public abstract void Fill(int originX, int originY, int width, int height, Color color);

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

        /// <summary>
        /// Create a new buffer from the existing buffer with a new rotation
        /// </summary>
        /// <typeparam name="T">Buffer type</typeparam>
        /// <param name="rotation">Rotation</param>
        /// <returns>The new buffer</returns>
        public T Rotate<T>(RotationType rotation) where T : PixelBufferBase, new()
        {
            PixelBufferBase newBuffer;

            switch (rotation)
            {
                case RotationType._90Degrees:
                    newBuffer = new T
                    {
                        Width = Height,
                        Height = Width
                    };
                    for (int i = 0; i < Width; i++)
                    {
                        for (int j = 0; j < Height; j++)
                        {
                            newBuffer.SetPixel(Height - j - 1, i, GetPixel(i, j));
                        }
                    }
                    break;
                case RotationType._270Degrees:
                    newBuffer = new T
                    {
                        Width = Height,
                        Height = Width
                    };
                    for (int i = 0; i < Width; i++)
                    {
                        for (int j = 0; j < Height; j++)
                        {
                            newBuffer.SetPixel(j, Width - i - 1, GetPixel(i, j));
                        }
                    }
                    break;
                case RotationType._180Degrees:
                    newBuffer = new T
                    {
                        Width = Width,
                        Height = Height
                    };
                    for (int i = 0; i < Width; i++)
                    {
                        for (int j = 0; j < Height; j++)
                        {
                            newBuffer.SetPixel(Width - i - 1, Height - j - 1, GetPixel(i, j));
                        }
                    }
                    break;
                case RotationType.Default:
                default:
                    newBuffer = new T
                    {
                        Width = Width,
                        Height = Height
                    };
                    Array.Copy(Buffer, newBuffer.Buffer, ByteCount);
                    break;
            }

            return (T)newBuffer;
        }
    }
}