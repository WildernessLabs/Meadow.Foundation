namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Bh1900Nux
    {
        internal enum Register : byte
        {
            Temperature = 0x00,
            Configuration = 0x01,
            TLow = 0x02,
            THigh = 0x03,
            Reset = 0x04
        }
    }
}