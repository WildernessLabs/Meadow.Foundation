namespace Meadow.Foundation.Displays.TextDisplayMenu.InputTypes
{
    /// <summary>
    /// Text display menu TimeDetailed input item
    /// </summary>
    public class TimeDetailed : TimeBase
    {
        /// <summary>
        /// Creates a new TimeDetailed input object
        /// </summary>
        public TimeDetailed() : base(TimeMode.HH_MM_SS)
        {
        }
    }
}