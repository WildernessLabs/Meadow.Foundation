using System;

using Meadow.Foundation.Sensors.Rotary;
using System.Threading;

namespace Meadow.Foundation.Displays.TextDisplayMenu.InputTypes
{
    public class Time : TimeBase
    {
        public Time() : base(TimeMode.HH_MM)
        {
        }
    }
}
