namespace Meadow.Foundation.Sensors.Environmental
{
    public partial class Pmsa003i
    {   // This is reversed because we reverse the whole message because it is big-endian and it makes converting values easier
        private static readonly byte[] Preamble = { 0x4d, 0x42 };
    }
}