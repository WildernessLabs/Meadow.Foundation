namespace Meadow.Foundation.Displays.UI.InputTypes
{
    /// <summary>
    /// Text display menu bool input type
    /// </summary>
    public class Boolean : ListBase
    {
        /// <summary>
        /// Create a new Boolean input object
        /// </summary>
        public Boolean()
        {
            choices = new string[2];
            choices[0] = "True";
            choices[1] = "False";
        }

        /// <summary>
        /// Go to the next item in the list
        /// For Boolean this is a toggle
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
        /// For Boolean this is a toggle
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