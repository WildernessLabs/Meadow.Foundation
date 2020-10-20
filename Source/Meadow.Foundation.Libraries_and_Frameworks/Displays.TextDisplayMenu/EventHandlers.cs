using System;

namespace Meadow.Foundation.Displays.TextDisplayMenu
{
    public class MenuSelectedEventArgs : EventArgs
    {
        private string command;
        public MenuSelectedEventArgs(string command)
        {
            this.command = command;
        }

        public string Command
        {
            get { return command; }
        }
    }
    public delegate void MenuSelectedHandler(object sender, MenuSelectedEventArgs e);

    public class ValueChangedEventArgs : EventArgs
    {
        private string id;
        private object value;
        public ValueChangedEventArgs(string id, object value)
        {
            this.id = id;
            this.value = value;
        }

        public string ItemID
        {
            get { return id; }
        }

        public object Value
        {
            get { return value; }
        }
    }
    public delegate void ValueChangedHandler(object sender, ValueChangedEventArgs e);
}