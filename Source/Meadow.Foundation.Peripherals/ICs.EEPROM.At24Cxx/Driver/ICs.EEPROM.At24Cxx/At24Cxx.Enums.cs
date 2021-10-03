namespace Meadow.Foundation.ICs.EEPROM
{
    public partial class At24Cxx
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Address0 = 0x50,
            Default = Address0
        }
    }
}
