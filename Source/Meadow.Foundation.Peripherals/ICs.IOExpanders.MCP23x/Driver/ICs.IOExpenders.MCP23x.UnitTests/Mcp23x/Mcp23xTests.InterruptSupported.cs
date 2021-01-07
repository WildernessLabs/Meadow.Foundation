using Meadow.Foundation.ICs.IOExpanders.UnitTests.Helpers;
using Xunit;

namespace Meadow.Foundation.ICs.IOExpanders.UnitTests.Mcp23x
{
    public partial class Mcp23xTests
    {
        [Theory]
        [InlineData(1, 1)]
        [InlineData(2, 1)]
        [InlineData(3, 1)]
        [InlineData(4, 1)]
        [InlineData(2, 2)]
        [InlineData(3, 3)]
        [InlineData(4, 4)]
        public void InterruptSupportedWhenInterruptProvided(int portCount, int interruptCount)
        {
            var mcp23x = Mcp23xTestImplementation.Create(portCount, interruptCount);
            Assert.True(mcp23x.InterruptSupported);
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(2, 0)]
        [InlineData(3, 0)]
        [InlineData(4, 0)]
        public void InterruptNotSupportWhenNoInterruptProvided(int portCount, int interruptCount)
        {
            var mcp23x = Mcp23xTestImplementation.Create(portCount, interruptCount);
            Assert.False(mcp23x.InterruptSupported);
        }

    }
}