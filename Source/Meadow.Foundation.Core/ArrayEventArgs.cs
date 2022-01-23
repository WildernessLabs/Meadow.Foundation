using System;

namespace Meadow.Foundation
{
    /// <summary>
    /// Custom event args that stores an int and an object
    /// </summary>
    public class ArrayEventArgs : EventArgs
    {
        /// <summary>
        /// Item index in array
        /// </summary>
        public int ItemIndex { get; set; }
        /// <summary>
        /// Item at index
        /// </summary>
        public object Item { get; set; } //Todo this should be updated to an int[] - used in DipSwitch.cs

        /// <summary>
        /// Create an ArrayEventArgs object
        /// </summary>
        /// <param name="itemIndex">Item index</param>
        /// <param name="item">Item/object</param>
        public ArrayEventArgs(int itemIndex, object item)
        {
            ItemIndex = itemIndex;
            Item = item;
        }        
    }

    /// <summary>
    /// Array event handler
    /// </summary>
    /// <param name="sender">sender</param>
    /// <param name="e">ArrayEventArgs</param>
    public delegate void ArrayEventHandler(object sender, ArrayEventArgs e);
}