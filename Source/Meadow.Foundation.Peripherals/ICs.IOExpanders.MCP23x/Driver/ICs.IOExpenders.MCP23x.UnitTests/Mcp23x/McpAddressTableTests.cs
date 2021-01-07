using Xunit;

namespace Meadow.Foundation.ICs.IOExpanders.UnitTests.Mcp23x
{
    public class McpAddressTableTests
    {
        [Theory]
        [InlineData(false, false, false, (byte) 0x20)]
        [InlineData(true, false, false, (byte) 0x21)]
        [InlineData(false, true, false, (byte) 0x22)]
        [InlineData(true, true, false, (byte) 0x23)]
        [InlineData(false, false, true, (byte) 0x24)]
        [InlineData(true, false, true, (byte) 0x25)]
        [InlineData(false, true, true, (byte) 0x26)]
        [InlineData(true, true, true, (byte) 0x27)]
        public void GetAddressFromPinsReturnsCorrectResult(bool pinA0, bool pinA1, bool pinA2, byte result)
        {

            Assert.Equal(result, McpAddressTable.GetAddressFromPins(pinA0, pinA1, pinA2));
        }
    }
}
