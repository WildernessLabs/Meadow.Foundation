namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Bmp085
    {
		/// <summary>
		///     Valid addresses for the sensor.
		/// </summary>
		public enum Addresses : byte
		{
			/// <summary>
			/// Bus address 0x77
			/// </summary>
			Address_0x77 = 0x77,
			/// <summary>
			/// Default bus address
			/// </summary>
			Default = Address_0x77
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
