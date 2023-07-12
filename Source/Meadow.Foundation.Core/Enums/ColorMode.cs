using System;

namespace Meadow.Foundation.Graphics
{
    /// <summary>
    /// Enum for Display color mode, defines bit depth and RGB order
    /// </summary>
    [Flags]
    public enum ColorMode : int
    {
        /// <summary>
        /// 1-bit color
        /// </summary>
        Format1bpp = 1 << 0,
        /// <summary>
        /// 2-bit color
        /// </summary>
        Format2bpp = 1 << 1,
        /// <summary>
        /// 4-bit grayscale
        /// </summary>
        Format4bppGray = 1 << 2,
        /// <summary>
        /// 4-bit grayscale
        /// </summary>
        Format4bppIndexed = 1 << 3,
        /// <summary>
        /// 8-bit grayscale
        /// </summary>
        Format8bppGray = 1 << 4,
        /// <summary>
        /// 8-bit color
        /// </summary>
        Format8bppRgb332 = 1 << 5,
        /// <summary>
        /// 12-bit color
        /// </summary>
        Format12bppRgb444 = 1 << 6,
        /// <summary>
        /// 15-bit color
        /// </summary>
        Format16bppRgb555 = 1 << 7,
        /// <summary>
        /// 16-bit color
        /// </summary>
        Format16bppRgb565 = 1 << 8,
        /// <summary>
        /// 18-bit color
        /// </summary>
        Format18bppRgb666 = 1 << 9,
        /// <summary>
        /// 24-bit color
        /// </summary>
        Format24bppRgb888 = 1 << 10,
        /// <summary>
        /// 24-bit color
        /// </summary>
        Format24bppGrb888 = 1 << 11,
        /// <summary>
        /// 32-bit color
        /// </summary>
        Format32bppRgba8888 = 1 << 12,
    }
}