namespace Graphics.MicroGraphics.Dither
{
    /// <summary>
    /// Specifies the dithering algorithm to use when rendering graphics
    /// </summary>
    public enum DitherMode
    {
        /// <summary>
        /// Uses a 4x4 ordered dithering matrix
        /// </summary>
        Ordered4x4,
        /// <summary>
        /// Uses the Floyd-Steinberg error diffusion dithering algorithm
        /// </summary>
        FloydSteinberg,
    }
}