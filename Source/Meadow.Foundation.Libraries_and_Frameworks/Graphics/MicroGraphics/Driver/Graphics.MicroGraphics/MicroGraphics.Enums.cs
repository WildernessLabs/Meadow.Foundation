namespace Meadow.Foundation.Graphics
{
    public partial class GraphicsLibrary
    {
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
