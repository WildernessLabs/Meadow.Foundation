using Meadow.Foundation.ICs.IOExpanders.UnitTests.Helpers;
using Xunit;

namespace Meadow.Foundation.ICs.IOExpanders.UnitTests.Mcp23x
{
    public partial class Mcp23xTests
    {
        [Theory]
        // [InlineData(1, 1)] // Mirrored interrupts don't matter for a single port
        [InlineData(2, 1)]
        [InlineData(3, 1)]
        [InlineData(4, 1)]
        public void MirroredModeEnabledWhenOnlyOneInterrupt(int portCount, int interruptCount)
        {
            var mcp23x = Mcp23xTestImplementation.Create(portCount, interruptCount);
            Assert.True(mcp23x.MirroredInterrupts);
        }

        [Theory]
        [InlineData(2, 2)]
        [InlineData(3, 3)]
        [InlineData(4, 4)]
        public void MirroredModeDisableWhenInterruptsCountMatchesPortsCount(int portCount, int interruptCount)
        {
            var mcp23x = Mcp23xTestImplementation.Create(portCount, interruptCount);
            Assert.False(mcp23x.MirroredInterrupts);
        }
    }
}