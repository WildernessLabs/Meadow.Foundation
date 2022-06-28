namespace Meadow.Foundation.ICs.IOExpanders
{
    public abstract partial class SerialWombatBase
    {
        public enum Pin : byte
        {
            Voltage = 66,
            Temperature = 70
        }
    }
}