namespace Meadow.Foundation.Displays.UI.InputTypes
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

        /// <summary>
        /// Go to the next item in the list
        /// For On/Off this is a toggle
        /// </summary>
        /// <returns>true</returns>
        public override bool Next()
        {
            if (selectedIndex == 0)
            {
                selectedIndex = 1;
            }
            else
            {
                selectedIndex = 0;
            }
            UpdateInputLine(OutputDisplay);
            return true;
        }

        /// <summary>
        /// Go to the previous item in the list
        /// For On/Off this is a toggle
        /// </summary>
        /// <returns>true</returns>
        public override bool Previous()
        {
            if (selectedIndex == 0)
            {
                selectedIndex = 1;
            }
            else
            {
                selectedIndex = 0;
            }
            UpdateInputLine(OutputDisplay);
            return true;
        }
    }
}