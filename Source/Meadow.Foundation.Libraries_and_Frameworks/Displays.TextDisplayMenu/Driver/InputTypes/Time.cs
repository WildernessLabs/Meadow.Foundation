namespace Meadow.Foundation.Displays.TextDisplayMenu.InputTypes
{
    /// <summary>
    /// Text display menu Time input item
    /// </summary>
    public class Time : TimeBase
    {
        /// <summary>
        /// Creates a new Time input object
        /// </summary>
        public Time() : base(TimeMode.HH_MM)
        {
        }
    }
}