using Newtonsoft.Json;

namespace Meadow.Foundation.Displays.TextDisplayMenu
{
    [JsonObject(MemberSerialization.OptIn)]
    public class MenuItem
    {
        [JsonProperty("sub")]
        public MenuItem[] SubItems { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("command")]
        public string Command { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("value")]
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