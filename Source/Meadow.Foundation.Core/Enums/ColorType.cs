namespace Meadow.Foundation.Graphics
{
    /// <summary>
    /// Enum for Display color mode, defines bit depth and RGB order
    /// </summary>
    public enum ColorType
    {
        /// <summary>
        /// 1-bit color
        /// </summary>
        Format1bpp,
        /// <summary>
        /// 2-bit color
        /// </summary>
        Format2bpp,
        /// <summary>
        /// 4-bit grayscale
        /// </summary>
        Format4bppGray,
        /// <summary>
        /// 8-bit grayscale
        /// </summary>
        Format8bppGray,
        /// <summary>
        /// 8-bit color
        /// </summary>
        Format8bppRgb332,
        /// <summary>
        /// 12-bit color
        /// </summary>
        Format12bppRgb444,
        /// <summary>
        /// 15-bit color
        /// </summary>
        Format16bppRgb555,
        /// <summary>
        /// 16-bit color
        /// </summary>
        Format16bppRgb565, //TFT in 16 bit mode
        /// <summary>
        /// 18-bit color
        /// </summary>
        Format18bppRgb666,
        /// <summary>
        /// 24-bit color
        /// </summary>
        Format24bppRgb888,
        /// <summary>
        /// 32-bit color
        /// </summary>
        Format32bppRgba8888
    }
}