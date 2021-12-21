namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Mcp23x08
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Address0 = 0x20,
            Default = Address0
        }
    }
}
