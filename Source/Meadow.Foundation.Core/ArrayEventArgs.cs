using System;

namespace Meadow.Foundation
{
    /// <summary>
    /// Custom event args that stores an int and an object
    /// </summary>
    public class ArrayEventArgs : EventArgs
    {
        public int ItemIndex { get; set; }
        public object Item { get; set; } //Todo this should be updated to an int[] - useed in DipSwitch.cs

        public ArrayEventArgs(int itemIndex, object item)
        {
            this.ItemIndex = ItemIndex;
            this.Item = item;
        }        
    }

    public delegate void ArrayEventHandler(object sender, ArrayEventArgs e);
}