namespace Meadow.Foundation.Displays.UI.InputTypes
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

        /// <summary>
        /// Parse the value
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{timeParts[0]}:{timeParts[1]}:{timeParts[2]}";
        }
    }
}