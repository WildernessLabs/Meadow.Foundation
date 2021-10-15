namespace Meadow.Foundation.Graphics
{
    public partial class GraphicsLibrary
    {
        /// <summary>
        /// Enum for Display color mode, defines bit depth and RGB order
        /// </summary>
        public enum ColorType
        {
            Format1bpp, //single color 
            Format2bpp, //for 2 color ePaper or 4 color gray scale
            Format4bppGray, //for 16 color gray scale
            Format8bppGray,
            Format8bppRgb332, //Some TFT displays support this mode
            Format12bppRgb444, //TFT in 12 bit mode
            Format16bppRgb555, //not currently used
            Format16bppRgb565, //TFT in 16 bit mode
            Format18bppRgb666, //TFT in 18 bit mode
            Format24bppRgb888  //24 bit color
        }

        /// <summary>
        /// Display rotation 
        /// </summary>
        public enum RotationType
        {
            Default,
            _90Degrees,
            _180Degrees,
            _270Degrees
        }

        public enum ScaleFactor : int
        {
            X1 = 1,
            X2 = 2,
            X3 = 3,
            X4 = 4,
        }

        public enum TextAlignment
        {
            Left,
            Center,
            Right
        }

        /// <summary>
        /// Mode for copying 1 bit bitmaps
        /// </summary>
        public enum BitmapMode
        {
            And,
            Or,
            XOr,
            Copy
        };
    }
}
