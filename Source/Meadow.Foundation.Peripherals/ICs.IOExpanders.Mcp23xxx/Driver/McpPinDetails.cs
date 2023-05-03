namespace Meadow.Foundation.ICs.IOExpanders
{
    public readonly struct McpPinDetails
    {
        public readonly Mcp23xxx.PortBank PortBank;
        public readonly byte Register;
        public readonly byte BitIndex;

        public McpPinDetails(Mcp23xxx.PortBank portBank, byte register, byte bitIndex)
        {
            PortBank = portBank;
            Register = register;
            BitIndex = bitIndex;
        }
    }
}