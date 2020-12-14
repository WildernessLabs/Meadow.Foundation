using System.Text.Json.Serialization;

namespace Meadow.Foundation.Displays.TextDisplayMenu
{
    public class MenuItem
    {
        [JsonPropertyName("sub")]
        public MenuItem[] SubItems { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("command")]
        public string Command { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("value")]
        public object Value { get; set; }

        public bool HasSubItems => SubItems != null && SubItems.Length > 1;

        public bool IsEditable => Value != null;

        public MenuItem(string text,
            string command = null,
            string id = null,
            string type = null,
            object value = null,
            MenuItem[] subItems = null)
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