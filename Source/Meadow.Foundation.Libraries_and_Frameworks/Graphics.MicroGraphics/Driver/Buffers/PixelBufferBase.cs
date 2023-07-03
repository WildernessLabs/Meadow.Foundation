using System;
using System.Linq;

namespace Meadow.Foundation.Graphics.Buffers
{
    /// <summary>
    /// Represents a pixel buffer
    /// </summary>
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
        public virtual ColorMode ColorMode { get; protected set; }

        /// <summary>
        /// Bitdepth of display as an integer
        /// </summary>
        public int BitDepth
        {
            get
            {
                return ColorMode switch
                {
                    ColorMode.Format1bpp => 1,
                    ColorMode.Format2bpp => 2,
                    ColorMode.Format4bppGray => 4,
                    ColorMode.Format4bppIndexed => 4,
                    ColorMode.Format8bppGray => 8,
                    ColorMode.Format8bppRgb332 => 8,
                    ColorMode.Format12bppRgb444 => 12,
                    ColorMode.Format16bppRgb555 => 15,
                    ColorMode.Format16bppRgb565 => 16,
                    ColorMode.Format18bppRgb666 => 18,
                    ColorMode.Format24bppRgb888 => 24,
                    ColorMode.Format24bppGrb888 => 24,
                    ColorMode.Format32bppRgba8888 => 32,
                    _ => throw new NotImplementedException($"Unknown or unsupported ColorMode: {ColorMode}"),
                };
            }
        }

        /// <summary>
        /// Number of bytes in buffer
        /// </summary>
        public int ByteCount => (Width * Height * BitDepth) >> 3;

        /// <summary>
        /// The buffer that holds the pixel data
        /// The packing structure in buffer-specific
        /// </summary>
        public byte[] Buffer { get; protected set; }


        /// <summary>
        /// Create a new PixelBufferBase object
        /// </summary>
        public PixelBufferBase() { }

        /// <summary>
        /// Create a new PixelBufferBase object
        /// </summary>
        /// <param name="width">Width in pixels</param>
        /// <param name="height">Height in pixels</param>
        public PixelBufferBase(int width, int height)
        {
            Width = width;
            Height = height;
            InitializeBuffer();
        }

        /// <summary>
        /// Create a new PixelBufferBase object
        /// </summary>
        /// <param name="width">Width in pixels</param>
        /// <param name="height">Height in pixels</param>
        /// <param name="buffer">The buffer to hold the pixel data</param>
        public PixelBufferBase(int width, int height, byte[] buffer)
        {
            Width = width;
            Height = height;
            if (buffer.Length != ByteCount)
            {
                throw new ArgumentException($"Provided buffer length ({buffer.Length}) does not match this buffer's ByteCount ({ByteCount}).");
            }
            Buffer = buffer;
        }

        /// <summary>
        /// Initialize the pixel buffer based on the current
        /// width, height and color depth
        /// </summary>
        /// <param name="replaceIfExists">If true, will recreates the buffer if it already exists</param>
        public void InitializeBuffer(bool replaceIfExists = false)
        {
            if (Buffer == null || replaceIfExists)
            {
                Buffer = new byte[ByteCount];
            }
        }

        /// <summary>
        /// Clear the array that stores the pixel buffer
        /// </summary>
        public virtual void Clear()
        {
            Array.Clear(Buffer, 0, Buffer.Length);
        }

        /// <summary>
        /// Fill the entire pixel buffer with a color
        /// </summary>
        /// <param name="color">Fill color</param>
        public abstract void Fill(Color color);

        /// <summary>
        /// Fill a region of the pixel buffer with a color
        /// </summary>
        /// <param name="originX">X pixel to start fill</param>
        /// <param name="originY">Y pixel to start fill</param>
        /// <param name="width">Width to fill</param>
        /// <param name="height">height to fill</param>
        /// <param name="color">Fill color</param>
        public abstract void Fill(int originX, int originY, int width, int height, Color color);

        /// <summary>
        /// Get pixel at location
        /// </summary>
        /// <param name="x">X pixel location</param>
        /// <param name="y">Y pixel location</param>
        /// <returns></returns>
        public abstract Color GetPixel(int x, int y);

        /// <summary>
        /// Set pixel at location to a color
        /// </summary>
        /// <param name="x">X pixel location</param>
        /// <param name="y">Y pixel location</param>
        /// <param name="color">Pixel color</param>
        public abstract void SetPixel(int x, int y, Color color);

        /// <summary>
        /// Invert pixel color at location
        /// </summary>
        /// <param name="x">X pixel location</param>
        /// <param name="y">Y pixel location</param>
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
            for (var x = 0; x < buffer.Width; x++)
            {
                for (var y = 0; y < buffer.Height; y++)
                {
                    SetPixel(originX + x, originY + y, buffer.GetPixel(x, y));
                }
            }
        }

        /// <summary>
        /// Create a new buffer from the existing buffer with a new rotation
        /// </summary>
        /// <typeparam name="T">Buffer type</typeparam>
        /// <param name="rotation">Rotation</param>
        /// <returns>The new buffer</returns>
        public T RotateAndConvert<T>(RotationType rotation)
            where T : PixelBufferBase, new()
        {
            T newBuffer;
            int[] rowLookup;
            int[] colLookup;

            switch (rotation)
            {
                case RotationType._90Degrees:
                    newBuffer = new T { Width = Height, Height = Width };
                    rowLookup = Enumerable.Range(0, Height).Reverse().ToArray();
                    colLookup = Enumerable.Range(0, Width).ToArray();
                    break;
                case RotationType._270Degrees:
                    newBuffer = new T { Width = Height, Height = Width };
                    rowLookup = Enumerable.Range(0, Height).ToArray();
                    colLookup = Enumerable.Range(0, Width).Reverse().ToArray();
                    break;
                case RotationType._180Degrees:
                    newBuffer = new T { Width = Width, Height = Height };
                    rowLookup = Enumerable.Range(0, Width).Reverse().ToArray();
                    colLookup = Enumerable.Range(0, Height).Reverse().ToArray();
                    break;
                case RotationType.Default:
                default:
                    return Clone<T>();
            }

            newBuffer.InitializeBuffer();

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    newBuffer.SetPixel(rowLookup[j], colLookup[i], GetPixel(i, j));
                }
            }

            return newBuffer;
        }

        /// <summary>
        /// Create a new buffer scaled up from the existing buffer
        /// </summary>
        /// <typeparam name="T">Buffer type</typeparam>
        /// <param name="scaleFactor">Integer scale ratio</param>
        /// <returns>The new buffer</returns>

        public T ScaleUp<T>(int scaleFactor)
            where T : PixelBufferBase, new()
        {
            T newBuffer = new T
            {
                Width = Width * scaleFactor,
                Height = Height * scaleFactor,
            };
            newBuffer.InitializeBuffer(true);
            newBuffer.Clear();

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    newBuffer.Fill(i * scaleFactor, j * scaleFactor, scaleFactor, scaleFactor, GetPixel(i, j));
                }
            }
            return newBuffer;
        }

        /// <summary>
        /// Create a new pixel buffer and 
        /// copy/convert pixel data from existing buffer
        /// </summary>
        /// <typeparam name="T">The buffer type to convert to</typeparam>
        /// <returns>A pixel buffer derrived from PixelBufferBase</returns>
        public T ConvertPixelBuffer<T>()
            where T : PixelBufferBase, new()
        {
            if (GetType() == typeof(T))
            {
                return Clone<T>();
            }

            T newBuffer = new T()
            {
                Width = Width,
                Height = Height,
            };
            newBuffer.InitializeBuffer();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    newBuffer.SetPixel(x, y, GetPixel(x, y));
                }
            }

            return newBuffer;
        }

        /// <summary>
        /// Make a copy of the buffer object 
        /// Intentionally private
        /// </summary>
        /// <typeparam name="T">The buffer type</typeparam>
        /// <returns>A new pixel buffer object</returns>
        T Clone<T>() where T : PixelBufferBase, new()
        {
            var newBuffer = new T
            {
                Width = Width,
                Height = Height
            };
            Array.Copy(Buffer, newBuffer.Buffer, ByteCount);

            return newBuffer;
        }

        /// <summary>
        /// Calculate the uncorrected distance between two colors using bytes for red, green, blue
        /// </summary>
        /// <param name="color1"></param>
        /// <param name="color2"></param>
        /// <returns>The distance as a double</returns>
        public double GetColorDistance(Color color1, Color color2)
        {
            var rDeltaSquared = Math.Abs(color1.R - color2.R) ^ 2;
            var gDeltaSquared = Math.Abs(color1.G - color2.G) ^ 2;
            var bDeltaSquared = Math.Abs(color1.B - color2.B) ^ 2;

            return Math.Sqrt(rDeltaSquared + gDeltaSquared + bDeltaSquared);
        }
    }
}
