namespace Meadow.Foundation.Sensors.Distance;

internal enum Command
{
    ReadVersion = 0x00,
    ExitCommandMode = 0xfe,
    EnterCommandMode = 0xff,
}
