namespace Meadow.Foundation.ICs.IOExpanders
{
    public abstract partial class SerialWombatBase
    {
        /// <summary>
        /// Serial Wombat values
        /// </summary>
        public enum SwPin : byte
        {
            /// <summary>
            /// Voltage
            /// </summary>
            Voltage = 66,
            /// <summary>
            /// Temperature
            /// </summary>
            Temperature = 70
        }
    }
}