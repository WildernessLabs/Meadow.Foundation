namespace Meadow.Foundation.Displays.TextDisplayMenu.InputTypes
{
    /// <summary>
    /// Text display menu on/off input item
    /// </summary>
    public class OnOff : ListBase
    {
        /// <summary>
        /// Creates a new OnOff input object
        /// </summary>
        public OnOff()
        {
            choices = new string[2];
            choices[0] = "On";
            choices[1] = "Off";
        }
    }
}