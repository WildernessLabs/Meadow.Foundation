using System;
using System.Collections.Generic;
using System.Text;

namespace Meadow.Foundation.Sensors.Hid
{
    public partial class BBQ10Keyboard
    {
        public enum KeyState
        {
            StateIdle = 0,
            StatePress,
            StateLongPress,
            StateRelease
        }

        public enum Addresses : byte
        {
            Default = Address0,
            Address0 = 0x1F,
        }
    }
}
