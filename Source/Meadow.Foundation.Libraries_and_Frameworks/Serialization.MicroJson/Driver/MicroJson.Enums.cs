namespace Meadow.Foundation.Serialization;

public static partial class MicroJson
{
    /// <summary>
    /// Enumeration of the popular formats of time and date with Json.
    /// </summary>
    public enum DateTimeFormat
    {
        /// <summary>
        /// ISO 8601 format for time and date representation in JSON.
        /// </summary>
        ISO8601,

        /// <summary>
        /// ASP.NET Ajax JSON format for time and date representation.
        /// </summary>
        Ajax
    }
}