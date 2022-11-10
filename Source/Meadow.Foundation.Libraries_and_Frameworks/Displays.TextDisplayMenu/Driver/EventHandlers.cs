using System;

namespace Meadow.Foundation.Displays.TextDisplayMenu
{
    /// <summary>
    /// Text display MenuSelectedEventArgs
    /// </summary>
    public class MenuSelectedEventArgs : EventArgs
    {
        /// <summary>
        /// Create a new MenuSelectedEventArgs object
        /// </summary>
        /// <param name="command">The command</param>
        public MenuSelectedEventArgs(string command)
        {
            Command = command;
        }
        /// <summary>
        /// Get the command string
        /// </summary>
        public string Command { get; private set; }
    }

    /// <summary>
    /// MenuSelectedHandler
    /// </summary>
    /// <param name="sender">the sender object</param>
    /// <param name="e">The MenuSelectedEventArgs</param>
    public delegate void MenuSelectedHandler(object sender, MenuSelectedEventArgs e);

    /// <summary>
    /// Text display ValueChangedEventArgs
    /// </summary>
    public class ValueChangedEventArgs : EventArgs
    {
        /// <summary>
        /// ValueChangedEventArgs
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="value">Value</param>
        public ValueChangedEventArgs(string id, object value)
        {
            ItemID = id;
            Value = value;
        }

        /// <summary>
        /// The item ID
        /// </summary>
        public string ItemID { get; private set; }

        /// <summary>
        /// The item value
        /// </summary>
        public object Value { get; private set; }
    }

    /// <summary>
    /// ValueChangedHandler
    /// </summary>
    /// <param name="sender">the sender object</param>
    /// <param name="e">The ValueChangedEventArgs</param>
    public delegate void ValueChangedHandler(object sender, ValueChangedEventArgs e);
}