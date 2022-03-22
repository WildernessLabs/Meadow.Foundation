namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Bme680
    {
        public struct Register
        {
            public Register(byte address, byte length)
            {
                Address = address;
                Length = length;
            }

            public byte Address { get; }
            public byte Length { get; }
        }
    }
}