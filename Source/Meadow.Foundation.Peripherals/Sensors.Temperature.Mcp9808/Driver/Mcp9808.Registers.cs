namespace Meadow.Foundation.Sensors.Temperature
{
    public partial class Mcp9808
    {
        internal class Registers
        {
            public const ushort CONFIG_SHUTDOWN = 0x0100;   // shutdown config
            public const ushort CONFIG_CRITLOCKED = 0x0080; // critical trip lock
            public const ushort CONFIG_WINLOCKED = 0x0040; // alarm window lock
            public const ushort CONFIG_INTCLR = 0x0020;     // interrupt clear
            public const ushort CONFIG_ALERTSTAT = 0x0010;  // alert output status
            public const ushort CONFIG_ALERTCTRL = 0x0008;  // alert output control
            public const ushort CONFIG_ALERTSEL = 0x0004;  // alert output select
            public const ushort CONFIG_ALERTPOL = 0x0002;   // alert output polarity
            public const ushort CONFIG_ALERTMODE = 0x0001;  // alert output mode

            public const byte REG_CONFIG = 0x01;        // config
            public const byte UPPER_TEMP = 0x02;     // upper alert boundary
            public const byte LOWER_TEMP = 0x03;     // lower alert boundery
            public const byte CRIT_TEMP = 0x04;     // critical temperature
            public const byte AMBIENT_TEMP = 0x05;   // ambient temperature
            public const byte MANUFACTURER_ID = 0x06; // manufacturer ID
            public const byte DEVICE_ID = 0x07;    // device ID
            public const byte RESOLUTION = 0x08;     // resolution
        }
    }
}