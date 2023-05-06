namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Bme280
    {
        internal enum Register : byte
        {
            ChipID = 0xd0,
            Reset = 0xe0,
            Humidity = 0xf2,
            Status = 0xf3,
            Measurement = 0xf4,
            Configuration = 0xf5,
        }
    }
}