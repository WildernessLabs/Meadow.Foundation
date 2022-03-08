namespace Meadow.Foundation.Sensors.Atmospheric
{
	public partial class Bme680
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
			/// Bus address 0x76
			/// </summary>
			Address_0x76 = 0x76,
			/// <summary>
			/// Default bus address
			/// </summary>
			Default = Address_0x77
		}

		internal class RegisterAddresses
		{
			public static readonly Register Status = new Register(0x73, 1);
			public const byte Reset = 0xE0;
			public const byte ID = 0xD0;
			public const byte Config = 0x75;
			public static readonly Register ControlTemperatureAndPressure = new Register(0x74, 1);
			public static readonly Register ControlHumidity = new Register(0x72, 1);
			public static readonly Register ControlGas1 = new Register(0x71, 1);
			public static readonly Register ControlGas0 = new Register(0x70, 1);

			public static readonly Register GasWait = new Register(0x64, 10);
			public static readonly Register ResHeat = new Register(0x5A, 10);
			public static readonly Register IdacHeat = new Register(0x50, 10);

			public static readonly Register Gas = new Register(0x2A, 2);
			public static readonly Register Humidity = new Register(0x25, 2);
			public static readonly Register Temperature = new Register(0x22, 3);
			public static readonly Register Pressure = new Register(0x1F, 3);

			public static readonly Register AllSensors = new Register(0x1F, 10);

			public static readonly Register TemperatureCompensation1 = new Register(0xE9, 2);
			public static readonly Register TemperatureCompensation2 = new Register(0x8A, 3);

			public static readonly Register PressureCompensations = new Register(0x8E, 16);
			public static readonly Register HumidityCompensations = new Register(0xE1, 8);
			public static readonly Register CompensationData1 = new Register(0x89, 25);
			public static readonly Register CompensationData2 = new Register(0xE1, 16);

			public const byte EasStatus0 = 0x1D;
		}
	}
}
