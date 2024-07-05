
namespace Meadow.Foundation.Displays.UI
{
    /// <summary>
    /// Represents a text display menu item
    /// </summary>
    public class MenuItem
    {
        /// <summary>
        /// Sub items in the menu
        /// </summary>
        public MenuItem[]? SubItems { get; set; }

        /// <summary>
        /// The text on the menu item
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The optional command when the item is selected
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// The menu item type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The menu item id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The menu item value
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// Does the item have sub items
        /// </summary>
        public bool HasSubItems => SubItems != null && SubItems.Length > 0;

        /// <summary>
        /// Is the menu item editable by the user
        /// </summary>
        public bool IsEditable => Value != null;

        /// <summary>
        /// Creates a new MenuItem object
        /// </summary>
        public MenuItem()
        {
            Text = string.Empty;
            Command = string.Empty;
            Id = string.Empty;
            Type = string.Empty;
            Value = null;
            SubItems = null;
        }

        /// <summary>
        /// Creates a new MenuItem object
        /// </summary>
        /// <param name="text">The item text</param>
        /// <param name="command">The item command</param>
        /// <param name="id">The item id</param>
        /// <param name="type">The item type</param>
        /// <param name="value">The item value</param>
        /// <param name="subItems">Item sub items</param>
        public MenuItem(string text,
            string? command = null,
            string? id = null,
            string? type = null,
            object? value = null,
            MenuItem[]? subItems = null)
        {
            Text = text;
            Command = command ?? string.Empty;
            Id = id ?? string.Empty;
            Type = type ?? string.Empty;
            Value = value ?? null;
            SubItems = subItems ?? null;
        }
    }
}