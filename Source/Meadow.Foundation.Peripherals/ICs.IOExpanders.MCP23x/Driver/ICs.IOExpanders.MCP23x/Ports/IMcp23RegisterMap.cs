namespace Meadow.Foundation.ICs.IOExpanders.Ports
{
    public interface IMcp23RegisterMap
    {
        byte GetAddress(Mcp23PortRegister register, int port, BankConfiguration bank);

        Mcp23PortRegister GetRegisterAtAddress(byte address, int port, BankConfiguration bank);

        Mcp23PortRegister GetNextRegister(byte currentAddress, int port, BankConfiguration bank);
    }
}
