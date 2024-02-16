using Meadow.Peripherals.Displays;
using System;

namespace Meadow.Foundation.Displays
{
    public partial class AsciiConsoleDisplay
    {
        internal class CharacterBuffer : IPixelBuffer
        {
            public int Width { get; }
            public int Height { get; }
            public ColorMode ColorMode => ColorMode.Format8bppGray;
            public int BitDepth => 6;

            public int ByteCount => throw new NotImplementedException();

            public byte[] Buffer => throw new NotImplementedException();

            private readonly char[,] _buffer;

            public CharacterBuffer(int width, int height)
            {
                Width = width;
                Height = height;

                _buffer = new char[width, height];
            }

            public void Clear()
            {
                Fill(Color.Black);
            }

            public void Fill(Color color)
            {
                for (var y = 0; y < Height; y++)
                {
                    for (var x = 0; x < Width; x++)
                    {
                        SetPixel(x, y, color);
                    }
                }
            }

            public void Fill(int originX, int originY, int width, int height, Color color)
            {
                for (var y = originY; y < height + originY; y++)
                {
                    for (var x = originX; x < width + originX; x++)
                    {
                        SetPixel(x, y, color);
                    }
                }
            }

            internal char GetPixelCharacter(int x, int y)
            {
                return _buffer[x, y];
            }

            public Color GetPixel(int x, int y)
            {
                throw new NotImplementedException();
            }

            public void InvertPixel(int x, int y)
            {
                throw new NotImplementedException();
            }

            public void SetPixel(int x, int y, Color color)
            {
                _buffer[x, y] = ColorToCharacter(color);
            }

            public void WriteBuffer(int originX, int originY, IPixelBuffer buffer)
            {
                throw new NotImplementedException();
            }

            private char ColorToCharacter(Color color)
            {
                var index = (int)((color.Color8bppGray / 255d) * (_colors.Length - 1));
                return _colors[index];
            }
        }
    }
}