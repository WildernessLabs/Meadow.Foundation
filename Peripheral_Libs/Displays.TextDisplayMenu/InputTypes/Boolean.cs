using System;

namespace Meadow.Foundation.Displays.TextDisplayMenu.InputTypes
{
    public class Boolean : ListBase
    {
        public Boolean()
        {
            _choices = new string[2];
            _choices[0] = "True";
            _choices[1] = "False";
        }
    }
}