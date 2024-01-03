namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Mcp3xxx
    {
        /// <summary>
        /// The Mcp3xxx pin connection type
        /// </summary>
        public enum InputType
        {
            /// <summary>
            /// The value is measured as the voltage on a single pin
            /// </summary>
            SingleEnded = 0,
            /// <summary>
            /// The value is measured as the difference in voltage between two pins with the first pin positive
            /// </summary>
            Differential = 1,
            /// <summary>
            /// The value is measured as the difference in voltage between two pins with the second pin positive
            /// </summary>
            InvertedDifferential = 2
        }
    }
}