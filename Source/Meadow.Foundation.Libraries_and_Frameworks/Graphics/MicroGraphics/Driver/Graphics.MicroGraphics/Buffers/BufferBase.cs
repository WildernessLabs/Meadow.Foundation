using System;

namespace Meadow.Foundation.Graphics.Buffers
{
    public abstract class BufferBase : IDisplayBuffer
    {
        public int Width { get; protected set; }
        public int Height { get; protected set; }

        public byte[] Buffer { get; protected set; }

        public abstract int ByteCount { get; }

        public abstract GraphicsLibrary.ColorType ColorType { get; }

        public BufferBase()
        {
        }

        public BufferBase(int width, int height)
        {
            Width = width;
            Height = height;

            Buffer = new byte[ByteCount];
        }

        public BufferBase(int width, int height, byte[] buffer)
        {
            Width = width;
            Height = height;

            if(buffer.Length != ByteCount)
            {
                throw new ArgumentException("buffer length doesn't match width, height and bit depth of buffer");
            }

            Buffer = buffer;
        }

        public abstract void SetPixel(int x, int y, Color color);
        public abstract Color GetPixel(int x, int y);

        public void Clear()
        {
            Array.Clear(Buffer, 0, Buffer.Length);
        }
    }
}
