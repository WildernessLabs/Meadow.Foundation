using System;
using System.Linq;
using Meadow.Foundation.ICs.IOExpanders.Ports;
using Meadow.Foundation.ICs.IOExpanders.UnitTests.Helpers;
using Xunit;

namespace Meadow.Foundation.ICs.IOExpanders.UnitTests.Mcp23x.Ports
{
    public class McpGpioPortTests
    {
        [Fact]
        public void AllPinsAreCorrectlyNamed()
        {
            var prefix = Guid.NewGuid().ToString();
            var port = new McpGpioPort(prefix);

            Assert.Equal($"{prefix}0", port.GP0.Name);
            Assert.Equal($"{prefix}1", port.GP1.Name);
            Assert.Equal($"{prefix}2", port.GP2.Name);
            Assert.Equal($"{prefix}3", port.GP3.Name);
            Assert.Equal($"{prefix}4", port.GP4.Name);
            Assert.Equal($"{prefix}5", port.GP5.Name);
            Assert.Equal($"{prefix}6", port.GP6.Name);
            Assert.Equal($"{prefix}7", port.GP7.Name);
        }

        [Fact]
        public void AllPinsAreInPinList()
        {
            var port = new McpGpioPort();

            Assert.Contains(port.GP0, port.AllPins);
            Assert.Contains(port.GP1, port.AllPins);
            Assert.Contains(port.GP2, port.AllPins);
            Assert.Contains(port.GP3, port.AllPins);
            Assert.Contains(port.GP4, port.AllPins);
            Assert.Contains(port.GP5, port.AllPins);
            Assert.Contains(port.GP6, port.AllPins);
            Assert.Contains(port.GP7, port.AllPins);

            Assert.Equal(8, port.AllPins.Count);
        }

        [Fact]
        public void AllPinsHaveTheCorrectKey()
        {
            var port = new McpGpioPort();

            Assert.Equal((byte) 0x00, port.GP0.Key);
            Assert.Equal((byte) 0x01, port.GP1.Key);
            Assert.Equal((byte) 0x02, port.GP2.Key);
            Assert.Equal((byte) 0x03, port.GP3.Key);
            Assert.Equal((byte) 0x04, port.GP4.Key);
            Assert.Equal((byte) 0x05, port.GP5.Key);
            Assert.Equal((byte) 0x06, port.GP6.Key);
            Assert.Equal((byte) 0x07, port.GP7.Key);
        }

        [Fact]
        public void ConstructorThrowsIfNamePrefixIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new McpGpioPort(null));
        }

        [Fact]
        public void DefaultPinPrefix()
        {
            var defaultPrefix = "GP";
            var port = new McpGpioPort();

            Assert.Equal($"{defaultPrefix}0", port.GP0.Name);
            Assert.Equal($"{defaultPrefix}1", port.GP1.Name);
            Assert.Equal($"{defaultPrefix}2", port.GP2.Name);
            Assert.Equal($"{defaultPrefix}3", port.GP3.Name);
            Assert.Equal($"{defaultPrefix}4", port.GP4.Name);
            Assert.Equal($"{defaultPrefix}5", port.GP5.Name);
            Assert.Equal($"{defaultPrefix}6", port.GP6.Name);
            Assert.Equal($"{defaultPrefix}7", port.GP7.Name);
        }

        [Fact]
        public void InvokeInputChangedFiresValidEvent()
        {
            var port = new McpGpioPort();
            var eventMonitor = new EventMonitor<IOExpanderPortInputChangedEventArgs>();
            var eventArgs = new IOExpanderPortInputChangedEventArgs(0x10, 0x20);

            port.InputChanged += eventMonitor.GetHandler();
            eventMonitor.AddExpectation((sender, e) => Assert.Same(port, sender));
            eventMonitor.AddExpectation((sender, e) => Assert.NotNull(e));
            eventMonitor.AddExpectation((sender, e) => Assert.Same(eventArgs, e));

            port.InvokeInputChanged(eventArgs);

            Assert.Single(eventMonitor.Invocations);

            port.InvokeInputChanged(eventArgs);

            Assert.Equal(2, eventMonitor.Invocations.Count());
        }

        [Fact]
        public void InvokeInputChangedThrowsIfEventArgsIsNull()
        {
            var port = new McpGpioPort();

            Assert.Throws<ArgumentNullException>(() => port.InvokeInputChanged(null));
        }
    }
}
