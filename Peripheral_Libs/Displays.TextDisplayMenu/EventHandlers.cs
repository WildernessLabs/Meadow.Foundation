using System;

namespace Meadow.Foundation.Displays.TextDisplayMenu
{
    public class MenuSelectedEventArgs : EventArgs
    {
        private string _command;
        public MenuSelectedEventArgs(string command)
        {
            this._command = command;
        }

        public string Command
        {
            get { return this._command; }
        }
    }
    public delegate void MenuSelectedHandler(object sender, MenuSelectedEventArgs e);

    public class ValueChangedEventArgs : EventArgs
    {
        private string _id;
        private object _value;
        public ValueChangedEventArgs(string id, object value)
        {
            this._id = id;
            this._value = value;
        }

        public string ItemID
        {
            get { return this._id; }
        }

        public object Value
        {
            get { return this._value; }
        }
    }
    public delegate void ValueChangedHandler(object sender, ValueChangedEventArgs e);
}
