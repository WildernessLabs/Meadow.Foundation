namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Mcp23xxx
    {
        internal interface IMcpDeviceComms
        {
            byte ReadRegister(byte address);
            void WriteRegister(byte address, byte value);
        }
    }
}