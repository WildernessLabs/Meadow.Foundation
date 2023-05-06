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
    }
}