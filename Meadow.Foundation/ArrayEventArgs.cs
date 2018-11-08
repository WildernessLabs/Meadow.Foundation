using System;

namespace Netduino.Foundation
{
    public class ArrayEventArgs : EventArgs
    {
        public int ItemIndex { get; set; }
        public object Item { get; set; }

        public ArrayEventArgs(int itemIndex, object item)
        {
            this.ItemIndex = ItemIndex;
            this.Item = item;
        }        
    }

    public delegate void ArrayEventHandler(object sender, ArrayEventArgs e);
}