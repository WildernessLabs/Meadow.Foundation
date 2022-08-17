namespace Meadow.Foundation.ICs.IOExpanders.Device
{
    /// <summary>
    /// The communication interface to the McpDevice
    /// </summary>
    public interface IMcpDeviceComms
    {
        byte ReadRegister(byte address);
        byte[] ReadRegisters(byte address, ushort length);
        void WriteRegister(byte address, byte value);
        void WriteRegisters(byte address, byte[] data);
    }
}
