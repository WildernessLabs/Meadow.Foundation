using Meadow.Peripherals.Sensors.Rotary;
using System;
                
namespace Meadow.Foundation.Displays.TextDisplayMenu.InputTypes
{
    public class Age : NumericBase
    {
        public Age(): base(0, 100, 0)
        {
        }
    }
}