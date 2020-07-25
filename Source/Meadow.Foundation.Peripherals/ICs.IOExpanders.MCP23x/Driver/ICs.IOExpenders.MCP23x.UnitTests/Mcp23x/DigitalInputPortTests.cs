using System;
using Meadow.Foundation.ICs.IOExpanders.Ports;
using Meadow.Hardware;
using Moq;
using Xunit;

namespace Meadow.Foundation.ICs.IOExpanders.UnitTests.Mcp23x
{
    public class DigitalInputPortTests
    {
        private static McpDigitalInputPort CreateInputPort(
            Mock<IMcp23x> mcpMock = null,
            IMcpGpioPorts ports = null,
            int portIndex = 0,
            IPin pin = null,
            InterruptMode interruptMode = InterruptMode.None)
        {
            mcpMock = mcpMock ?? new Mock<IMcp23x>();
            ports = ports ?? new Mcp23xPorts(new McpGpioPort());
            pin = pin ?? ports[0].GP0;

            mcpMock.SetupGet(x => x.Ports).Returns(ports);

            return new McpDigitalInputPort(mcpMock.Object, pin, portIndex, interruptMode);
        }

        [Theory]
        [InlineData(ResistorMode.PullDown)]
        [InlineData(ResistorMode.PullUp)]
        public void SetResistorToNotDisabledWillThrow(ResistorMode resistorMode)
        {
            var digitalInputPort = CreateInputPort();

            Assert.Throws<NotImplementedException>(() => digitalInputPort.Resistor = resistorMode);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetStateReadsFromMcpPin(bool pinState)
        {
            var mcpMock = new Mock<IMcp23x>();
            var ports = new Mcp23xPorts(new McpGpioPort());
            var pin = ports[0].GP1;
            mcpMock.Setup(mcp => mcp.ReadPin(It.IsIn(pin))).Returns(pinState);

            var digitalInputPort = CreateInputPort(mcpMock, ports, pin: pin);

            Assert.Equal(pinState, digitalInputPort.State);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        [InlineData(2)]
        [InlineData(int.MaxValue)]
        public void ConstructorThrowsIfPortIsOutOfRange(int portIndex)
        {
            var ports = new Mcp23xPorts(new McpGpioPort(), new McpGpioPort());

            Assert.Throws<ArgumentOutOfRangeException>(() => CreateInputPort(ports: ports, portIndex: portIndex));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void ConstructorPassesIfPortIsInRange(int portIndex)
        {
            var ports = new Mcp23xPorts(new McpGpioPort(), new McpGpioPort());
            var pin = ports[portIndex].GP1;

            CreateInputPort(ports: ports, portIndex: portIndex, pin: pin);
        }

        [Fact]
        public void ConstructorThrowsIfPinIsFromDifferentController()
        {
            var ports = new Mcp23xPorts(new McpGpioPort(), new McpGpioPort());
            var portIndex = 0;
            var pin = new McpGpioPort().GP1;

            Assert.Throws<ArgumentException>(() => CreateInputPort(ports: ports, portIndex: portIndex, pin: pin));
        }

        [Fact]
        public void ConstructorThrowsIfPinIsFromDifferentPort()
        {
            var ports = new Mcp23xPorts(new McpGpioPort(), new McpGpioPort());
            var portIndex = 0;
            var pin = ports[1].GP1;

            Assert.Throws<ArgumentException>(() => CreateInputPort(ports: ports, portIndex: portIndex, pin: pin));
        }

        [Fact]
        public void ResistorIsAlwaysDisabled()
        {
            var digitalInputPort = CreateInputPort();

            // get
            Assert.Equal(ResistorMode.Disabled, digitalInputPort.Resistor);

            // set works too
            digitalInputPort.Resistor = ResistorMode.Disabled;
        }
    }
}
