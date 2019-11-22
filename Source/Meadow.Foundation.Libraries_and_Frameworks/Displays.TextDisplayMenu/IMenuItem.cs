using System;
using System.Collections;


namespace Meadow.Foundation.Displays.TextDisplayMenu
{
    public interface IMenuItem
    {
        MenuPage SubMenu { get; set; }

        string Text { get; set; }

        object Value { get; set; }
    }
}
