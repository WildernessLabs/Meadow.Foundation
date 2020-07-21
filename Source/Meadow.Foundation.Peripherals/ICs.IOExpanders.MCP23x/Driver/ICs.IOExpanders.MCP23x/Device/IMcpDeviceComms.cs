namespace Meadow.Foundation.ICs.IOExpanders.Device
{
    public interface IMcpDeviceComms
    {
        byte ReadRegister(byte address);
        byte[] ReadRegisters(byte address, ushort length);
        void WriteRegister(byte address, byte value);
        void WriteRegisters(byte address, byte[] data);
    }
}
