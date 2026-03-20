namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Tca9548a
    {
        /// <summary>
		/// Valid I2C addresses for the sensor
		/// </summary>
		public enum Addresses : byte
        {
            /// <summary>
            /// I2cBus address 0x70
            /// </summary>
            Address_0x70 = 0x70,
            /// <summary>
            /// I2cBus address 0x71
            /// </summary>
            Address_0x71 = 0x71,
            /// <summary>
            /// I2cBus address 0x72
            /// </summary>
            Address_0x72 = 0x72,
            /// <summary>
            /// I2cBus address 0x73
            /// </summary>
            Address_0x73 = 0x73,
            /// <summary>
            /// I2cBus address 0x74
            /// </summary>
            Address_0x74 = 0x74,
            /// <summary>
            /// I2cBus address 0x75
            /// </summary>
            Address_0x75 = 0x75,
            /// <summary>
            /// I2cBus address 0x76
            /// </summary>
            Address_0x76 = 0x76,
            /// <summary>
            /// I2cBus address 0x77
            /// </summary>
            Address_0x77 = 0x77,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x70
        }
    }
}