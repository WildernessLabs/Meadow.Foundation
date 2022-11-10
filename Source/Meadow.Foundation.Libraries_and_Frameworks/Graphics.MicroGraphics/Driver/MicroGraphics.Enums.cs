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
    }

    /// <summary>
    /// Horizontal text alignment when drawing
    /// </summary>
    public enum TextAlignment
    {
        /// <summary>
        /// Left aligned
        /// </summary>
        Left,
        /// <summary>
        /// Horizontally centered
        /// </summary>
        Center,
        /// <summary>
        /// Right aligned
        /// </summary>
        Right
    }
}