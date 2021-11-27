using System;

namespace Meadow.Foundation.Graphics
{
    /// <summary>
    ///     Abstract class for a Font.
    /// </summary>
    [Obsolete("FontBase is depricated, use IFont instead")]
    public abstract class FontBase : IFont
    {
        public byte[] this[char character] => throw new System.NotImplementedException();

        public int Width => throw new System.NotImplementedException();

        public int Height => throw new System.NotImplementedException();
    } 
}