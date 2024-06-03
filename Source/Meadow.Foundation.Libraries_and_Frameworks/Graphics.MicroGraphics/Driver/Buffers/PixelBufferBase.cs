using Meadow.Peripherals.Displays;
using Meadow.Units;
using System;
using System.Linq;

namespace Meadow.Foundation.Graphics.Buffers
{
    /// <summary>
    /// Represents a pixel buffer
    /// </summary>
    public abstract class PixelBufferBase : IPixelBuffer, IDisposable
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
        /// Bit depth of display as an integer
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
        public virtual int ByteCount => (Width * Height * BitDepth) >> 3;

        /// <summary>
        /// The buffer that holds the pixel data
        /// The packing structure in buffer-specific
        /// </summary>
        public byte[] Buffer { get; protected set; } = default!;

        /// <summary>
        /// Did we create the buffer (true) or was it passed in (false)
        /// </summary>
        readonly bool createdBuffer = true;

        bool isDisposed = false;

        /// <summary>
        /// Create a new PixelBufferBase object
        /// </summary>
        public PixelBufferBase()
        {
            Buffer = new byte[0];
        }

        /// <summary>
        /// Create a new PixelBufferBase object
        /// </summary>
        /// <param name="width">Width in pixels</param>
        /// <param name="height">Height in pixels</param>
        public PixelBufferBase(int width, int height)
        {
            if (width < 1 || height < 1)
            {
                throw new ArgumentException("Width and height must be greater than 0.");
            }

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
            if (width < 1 || height < 1)
            {
                throw new ArgumentException("Width and height must be greater than 0.");
            }

            Width = width;
            Height = height;

            if (buffer.Length != ByteCount)
            {
                throw new ArgumentException($"Provided buffer length ({buffer.Length}) does not match this buffer's ByteCount ({ByteCount}).");
            }
            Buffer = buffer;

            createdBuffer = false;
        }

        /// <summary>
        /// Initialize the pixel buffer based on the current
        /// width, height and color depth
        /// </summary>
        /// <param name="replaceIfExists">If true, will recreates the buffer if it already exists</param>
        public void InitializeBuffer(bool replaceIfExists = false)
        {
            if (Buffer == null || Buffer.Length == 0 || replaceIfExists)
            {
                Buffer = new byte[ByteCount];
            }
        }

        /// <summary>
        /// Clear the array that stores the pixel buffer
        /// </summary>
        public virtual void Clear()
        {
            if (Buffer != null)
            {
                Array.Clear(Buffer, 0, Buffer.Length);
            }
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
        public T Rotate<T>(RotationType rotation)
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
        /// Create a new buffer integer scaled up from the existing buffer
        /// </summary>
        /// <typeparam name="T">Buffer type</typeparam>
        /// <param name="scaleFactor">Integer scale ratio</param>
        /// <returns>The new buffer</returns>

        public T ScaleUp<T>(int scaleFactor)
            where T : PixelBufferBase, new()
        {
            T newBuffer = new()
            {
                Width = Width * scaleFactor,
                Height = Height * scaleFactor,
            };
            newBuffer.InitializeBuffer(true);

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
        /// Create a new pixel buffer and copy/convert pixel data from existing buffer
        /// </summary>
        /// <typeparam name="T">The buffer type to convert to</typeparam>
        /// <returns>A pixel buffer derived from PixelBufferBase</returns>
        public T Convert<T>()
            where T : PixelBufferBase, new()
        {
            if (GetType() == typeof(T))
            {
                return Clone<T>();
            }

            T newBuffer = new()
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
        /// Resize the buffer to new dimensions using the nearest neighbor algorithm
        /// </summary>
        /// <typeparam name="T">Buffer type</typeparam>
        /// <param name="newWidth">New width</param>
        /// <param name="newHeight">New height</param>
        /// <returns>The resized buffer</returns>
        public T Resize<T>(int newWidth, int newHeight)
            where T : PixelBufferBase, new()
        {
            T newBuffer = new()
            {
                Width = newWidth,
                Height = newHeight,
            };
            newBuffer.InitializeBuffer(true);

            float xRatio = (float)Width / newWidth;
            float yRatio = (float)Height / newHeight;

            for (int i = 0; i < newWidth; i++)
            {
                for (int j = 0; j < newHeight; j++)
                {
                    int srcX = (int)(i * xRatio);
                    int srcY = (int)(j * yRatio);
                    newBuffer.SetPixel(i, j, GetPixel(srcX, srcY));
                }
            }
            return newBuffer;
        }

        /// <summary>
        /// Resize the buffer to new dimensions using bilinear interpolation
        /// </summary>
        /// <typeparam name="T">Buffer type</typeparam>
        /// <param name="newWidth">New width</param>
        /// <param name="newHeight">New height</param>
        /// <returns>The resized buffer</returns>
        public T ResizeBilinear<T>(int newWidth, int newHeight)
            where T : PixelBufferBase, new()
        {
            T newBuffer = new()
            {
                Width = newWidth,
                Height = newHeight,
            };
            newBuffer.InitializeBuffer(true);

            float xRatio = (float)(Width - 1) / newWidth;
            float yRatio = (float)(Height - 1) / newHeight;

            for (int i = 0; i < newWidth; i++)
            {
                for (int j = 0; j < newHeight; j++)
                {
                    float gx = i * xRatio;
                    float gy = j * yRatio;
                    int gxi = (int)gx;
                    int gyi = (int)gy;

                    var c00 = GetPixel(gxi, gyi);
                    var c10 = GetPixel(gxi + 1, gyi);
                    var c01 = GetPixel(gxi, gyi + 1);
                    var c11 = GetPixel(gxi + 1, gyi + 1);

                    float w00 = (1 - (gx - gxi)) * (1 - (gy - gyi));
                    float w10 = (gx - gxi) * (1 - (gy - gyi));
                    float w01 = (1 - (gx - gxi)) * (gy - gyi);
                    float w11 = (gx - gxi) * (gy - gyi);

                    var r = (byte)(c00.R * w00 + c10.R * w10 + c01.R * w01 + c11.R * w11);
                    var g = (byte)(c00.G * w00 + c10.G * w10 + c01.G * w01 + c11.G * w11);
                    var b = (byte)(c00.B * w00 + c10.B * w10 + c01.B * w01 + c11.B * w11);

                    newBuffer.SetPixel(i, j, new Color(r, g, b));
                }
            }

            return newBuffer;
        }

        /// <summary>
        /// Rotate the buffer by an arbitrary angle
        /// </summary>
        /// <typeparam name="T">Buffer type</typeparam>
        /// <param name="angle">Rotation angle in degrees</param>
        /// <returns>The rotated buffer</returns>
        public T Rotate<T>(Angle angle)
            where T : PixelBufferBase, new()
        {
            // Convert angle to radians
            var radians = (float)angle.Radians;

            // Calculate sine and cosine of the angle
            var cos = MathF.Cos(radians);
            var sin = MathF.Sin(radians);

            // Calculate the new width and height of the bounding box
            int newWidth = (int)MathF.Ceiling(MathF.Abs(Width * cos) + MathF.Abs(Height * sin));
            int newHeight = (int)MathF.Ceiling(MathF.Abs(Width * sin) + MathF.Abs(Height * cos));

            // Create a new buffer
            T newBuffer = new()
            {
                Width = newWidth,
                Height = newHeight,
            };
            newBuffer.InitializeBuffer(true);
            newBuffer.Clear();

            // Center of the original and new buffer
            int x0 = Width / 2;
            int y0 = Height / 2;
            int x1 = newWidth / 2;
            int y1 = newHeight / 2;

            // Map each pixel from the original buffer to the new buffer
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    // Calculate the coordinates relative to the center
                    int dx = x - x0;
                    int dy = y - y0;

                    // Apply the rotation matrix
                    int newX = (int)(dx * cos - dy * sin + x1);
                    int newY = (int)(dx * sin + dy * cos + y1);

                    // Set the pixel in the new buffer if within bounds
                    if (newX >= 0 && newX < newWidth && newY >= 0 && newY < newHeight)
                    {
                        newBuffer.SetPixel(newX, newY, GetPixel(x, y));
                    }
                }
            }

            return newBuffer;
        }

        /// <summary>
        /// Calculate the uncorrected distance between two colors using bytes for red, green, blue
        /// </summary>
        /// <param name="color1"></param>
        /// <param name="color2"></param>
        /// <returns>The distance as a float</returns>
        public float GetColorDistance(Color color1, Color color2)
        {
            var rDeltaSquared = MathF.Pow(MathF.Abs(color1.R - color2.R), 2);
            var gDeltaSquared = MathF.Pow(MathF.Abs(color1.G - color2.G), 2);
            var bDeltaSquared = MathF.Pow(MathF.Abs(color1.B - color2.B), 2);

            return MathF.Sqrt(rDeltaSquared + gDeltaSquared + bDeltaSquared);
        }

        /// <summary>
        /// Dispose of the object
        /// </summary>
        /// <param name="disposing">Is disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing && createdBuffer)
                {
                    if (disposing)
                    {
                        Buffer = new byte[0];
                    }
                }
            }
            isDisposed = true;
        }

        ///<inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}