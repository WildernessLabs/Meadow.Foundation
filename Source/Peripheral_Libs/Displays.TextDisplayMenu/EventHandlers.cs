using System;

namespace Meadow.Foundation.Displays.TextDisplayMenu
{
    public class MenuSelectedEventArgs : EventArgs
    {
        private string _command;
        public MenuSelectedEventArgs(string command)
        {
            _command = command;
        }

        public string Command
        {
            get { return _command; }
        }
    }
    public delegate void MenuSelectedHandler(object sender, MenuSelectedEventArgs e);

    public class ValueChangedEventArgs : EventArgs
    {
        private string _id;
        private object _value;
        public ValueChangedEventArgs(string id, object value)
        {
            _id = id;
            _value = value;
        }

        public string ItemID
        {
            get { return _id; }
        }

        public object Value
        {
            get { return _value; }
        }
    }
    public delegate void ValueChangedHandler(object sender, ValueChangedEventArgs e);
}
