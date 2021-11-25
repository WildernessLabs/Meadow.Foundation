using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using System;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Abstract hardware display class 
    /// </summary>
    [Obsolete]
    public abstract class DisplayBase : IGraphicsDisplay
    {
        public ColorType ColorMode => throw new NotImplementedException();

        public int Width => throw new NotImplementedException();

        public int Height => throw new NotImplementedException();

        public bool IgnoreOutOfBoundsPixels { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Clear(bool updateDisplay = false)
        {
            throw new NotImplementedException();
        }

        public void DrawBuffer(int x, int y, IDisplayBuffer displayBuffer)
        {
            throw new NotImplementedException();
        }

        public void DrawPixel(int x, int y, Color color)
        {
            throw new NotImplementedException();
        }

        public void DrawPixel(int x, int y, bool colored)
        {
            throw new NotImplementedException();
        }

        public void Fill(Color fillColor, bool updateDisplay = false)
        {
            throw new NotImplementedException();
        }

        public void Fill(int x, int y, int width, int height, Color fillColor)
        {
            throw new NotImplementedException();
        }

        public void InvertPixel(int x, int y)
        {
            throw new NotImplementedException();
        }

        public void Show()
        {
            throw new NotImplementedException();
        }

        public void Show(int left, int top, int right, int bottom)
        {
            throw new NotImplementedException();
        }
    }
}
