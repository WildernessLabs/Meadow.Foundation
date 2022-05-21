using Meadow.Foundation.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Meadow.Foundation.Graphics.Buffers
{
    /// <summary>
    /// An interface representing the concept of a
    /// buffer that holds a collection of pixels
    /// that can be rendered to a wide variety of devices
    /// at a variety of bit depths.
    /// 
    /// This interface has default implementations for many
    /// of its properties and methods.
    /// </summary>
    public interface IPixelBuffer
    {
        /// <summary>
        /// The width of the pixel buffer
        /// </summary>
        int Width { get; }

        /// <summary>
        /// The height of the pixel buffer
        /// </summary>
        int Height { get; }

        /// <summary>
        /// The ColorMode of the pixel buffer
        /// </summary>
        ColorType ColorMode { get; }

        /// <summary>
        /// The BitDepth of the chosen ColorMode.
        /// </summary>
        int BitDepth { get; }

        /// <summary>
        /// The number of bytes in this pixel buffer
        /// </summary>
        int ByteCount { get; }

        /// <summary>
        /// Determines whether this buffer should throw errors if
        /// you try to write pixels outside of the bounds
        /// </summary>
        bool IgnoreOutOfBounds { get; }

        /// <summary>
        /// The byte array that holds all pixel data
        /// </summary>
        byte[] Buffer { get; }




        /// <summary>
        /// Set the color of the pixel at the provided coordinates
        /// </summary>
        /// <param name="x">X coordinate of the pixel: 0,0 at top left</param>
        /// <param name="y">Y coordinate of the pixel: 0,0 at top left</param>
        /// <param name="color">The pixel color</param>
        void SetPixel(int x, int y, Color color);

        /// <summary>
        /// Get the color of a pixel - may be scaled based on buffer color depth
        /// </summary>
        /// <param name="x">X coordinate of the pixel: 0,0 at top left</param>
        /// <param name="y">Y coordinate of the pixel: 0,0 at top left</param>
        /// <param name="color">The pixel color</param>
        Color GetPixel(int x, int y);

        /// <summary>
        /// Invert the color of a pixel at the provided location
        /// </summary>
        /// <param name="x">The X coord to invert</param>
        /// <param name="y">The Y coord to invert</param>
        void InvertPixel(int x, int y);

        /// <summary>
        /// Writes another pixel buffer into this buffer.
        /// </summary>
        /// <param name="originX">The X origin to start writing</param>
        /// <param name="originY">The Y origin to start writing</param>
        /// <param name="buffer">The buffer to write into this buffer</param>
        /// <returns></returns>
        void WriteBuffer(int originX, int originY, IPixelBuffer buffer);

        /// <summary>
        /// Fills the buffer with the provided color
        /// </summary>
        /// <param name="color">The color to fill</param>
        void Fill(Color color);

        /// <summary>
        /// Fills part of the buffer with the provided color
        /// </summary>
        /// <param name="color">The color to fill</param>
        /// <param name="originX">The X coord to start filling</param>
        /// <param name="originY">The Y coord to start filling</param>
        /// <param name="width">The width to fill</param>
        /// <param name="height">The height to fill</param>
        void Fill(Color color, int originX, int originY, int width, int height);

        /// <summary>
        /// Clears the buffer (writes 0s to the byte array)
        /// </summary>
        void Clear();
    }
}
