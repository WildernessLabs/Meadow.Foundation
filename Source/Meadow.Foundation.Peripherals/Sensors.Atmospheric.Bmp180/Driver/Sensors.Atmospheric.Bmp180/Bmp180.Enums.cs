namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Bmp180
    {
		/// <summary>
		///     Valid addresses for the sensor.
		/// </summary>
		public enum Addresses : byte
		{
			Address0 = 0x77,
			Default = Address0
		}

		public enum DeviceMode
		{
			UltraLowPower = 0,
			Standard = 1,
			HighResolution = 2,
			UltraHighResolution = 3
		}
	}
}
