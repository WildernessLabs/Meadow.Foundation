using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    internal readonly struct McpPinDetails
    {
        public readonly IPin Pin;
        public readonly Mcp23xxx.PortBank PortBank;
        public readonly byte Register;
        public readonly byte BitIndex;

        public McpPinDetails(IPin pin, Mcp23xxx.PortBank portBank, byte register, byte bitIndex)
        {
            Pin = pin;
            PortBank = portBank;
            Register = register;
            BitIndex = bitIndex;
        }
    }
}