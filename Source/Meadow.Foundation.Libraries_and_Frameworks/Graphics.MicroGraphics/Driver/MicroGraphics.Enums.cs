namespace Meadow.Foundation.Graphics
{
    /// <summary>
    /// Text scale factor when drawing text
    /// </summary>
    public enum ScaleFactor : int
    {
        /// <summary>
        /// 1X / normal scaling
        /// </summary>
        X1 = 1,
        /// <summary>
        /// 2X / double scaling
        /// </summary>
        X2 = 2,
        /// <summary>
        /// 3X / triple scaling
        /// </summary>
        X3 = 3,
        /// <summary>
        /// 4x / quadruple scaling
        /// </summary>
        X4 = 4,
        /// <summary>
        /// 5x / quintuple scaling
        /// </summary>
        X5 = 5,
        /// <summary>
        /// 6x / sextuple scaling
        /// </summary>
        X6 = 6,
        /// <summary>
        /// 7x / septuple scaling
        /// </summary>
        X7 = 7,
        /// <summary>
        /// 8x / octuple scaling
        /// </summary>
        X8 = 8,
    }

    /// <summary>
    /// Horizontal alignment for visual elements
    /// </summary>
    public enum HorizontalAlignment
    {
        /// <summary>
        /// Left aligned, drawing starts at X horizontally
        /// </summary>
        Left,
        /// <summary>
        /// Horizontally centered, X is horizontally centered 
        /// </summary>
        Center,
        /// <summary>
        /// Right aligned, drawing ends at X horizontally
        /// </summary>
        Right
    }

    /// <summary>
    /// Vertical alignment for visual elements
    /// </summary>
    public enum VerticalAlignment
    {
        /// <summary>
        /// Top aligned, drawing starts at Y vertically
        /// </summary>
        Top,
        /// <summary>
        /// Horizontally centered, Y is vertically centered 
        /// </summary>
        Center,
        /// <summary>
        /// Bottom aligned, drawing ends at Y vertically
        /// </summary>
        Bottom
    }
}