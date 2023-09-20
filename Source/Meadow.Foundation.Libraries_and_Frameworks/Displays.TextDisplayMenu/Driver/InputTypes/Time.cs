namespace Meadow.Foundation.Displays.UI.InputTypes
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

        /// <summary>
        /// Parse the value
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{timeParts[0]}:{timeParts[1]}";
        }
    }
}