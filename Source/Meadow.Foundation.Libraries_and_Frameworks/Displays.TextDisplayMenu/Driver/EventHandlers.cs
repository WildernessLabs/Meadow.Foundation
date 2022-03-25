using System;

namespace Meadow.Foundation.Displays.TextDisplayMenu
{
    public class MenuSelectedEventArgs : EventArgs
    {
        public MenuSelectedEventArgs(string command)
        {
            Command = command;
        }

        public string Command { get; private set; }
    }
    public delegate void MenuSelectedHandler(object sender, MenuSelectedEventArgs e);

    public class ValueChangedEventArgs : EventArgs
    {
        public ValueChangedEventArgs(string id, object value)
        {
            ItemID = id;
            Value = value;
        }

        public string ItemID { get; private set; }

        public object Value { get; private set; }
    }
    public delegate void ValueChangedHandler(object sender, ValueChangedEventArgs e);
}