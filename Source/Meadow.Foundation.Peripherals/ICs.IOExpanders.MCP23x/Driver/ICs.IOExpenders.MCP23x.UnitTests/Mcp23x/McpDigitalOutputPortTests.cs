using System;
using Meadow.Foundation.ICs.IOExpanders.Ports;
using Meadow.Hardware;
using Moq;
using Xunit;

namespace Meadow.Foundation.ICs.IOExpanders.UnitTests.Mcp23x
{
    public class McpDigitalOutputPortTests
    {
        private static McpDigitalOutputPort CreateOutputPort(
            Mock<IMcp23x> mcpMock = null,
            IMcpGpioPorts ports = null,
            IPin pin = null,
            bool initialState = false)
        {
            mcpMock = mcpMock ?? new Mock<IMcp23x>();
            ports = ports ?? new Mcp23xPorts(new McpGpioPort());
            pin = pin ?? ports[0].GP0;

            mcpMock.SetupGet(x => x.Ports).Returns(ports);

            return new McpDigitalOutputPort(mcpMock.Object, pin, initialState);
        }

        [Fact]
        public void ConstructorThrowsIfPinIsFromDifferentController()
        {
            var ports = new Mcp23xPorts(new McpGpioPort(), new McpGpioPort());
            var pin = new McpGpioPort().GP1;

            Assert.Throws<ArgumentException>(() => CreateOutputPort(ports: ports, pin: pin));
        }

        [Fact]
        public void ConstructorWithValidInputsSucceeds()
        {
            CreateOutputPort();
        }


        [Fact]
        public void ConstructorWritesInitialStateToPin()
        {
            var ports = new Mcp23xPorts(new McpGpioPort());
            var pin1 = ports[0].GP0;
            var pin2 = ports[0].GP0;
            var mcpMock = new Mock<IMcp23x>();
            mcpMock.Setup(m => m.WritePin(It.IsAny<IPin>(), It.IsAny<bool>()));

            var digitalOutputPort1 = CreateOutputPort(mcpMock, ports, pin1, true);
            mcpMock.Verify(m => m.WritePin(It.Is<IPin>(p => p == pin1), It.Is<bool>(b => b)), Times.Exactly(1));

            var digitalOutputPort2 = CreateOutputPort(mcpMock, ports, pin2, false);
            mcpMock.Verify(
                m => m.WritePin(It.Is<IPin>(p => p == pin2), It.Is<bool>(b => b == false)),
                Times.Exactly(1));
        }

        [Fact]
        public void DisposeResetsPin()
        {
            var ports = new Mcp23xPorts(new McpGpioPort());
            var pin = ports[0].GP0;
            var mcpMock = new Mock<IMcp23x>();
            mcpMock.Setup(m => m.ResetPin(It.IsAny<IPin>()));

            var digitalOutputPort = CreateOutputPort(mcpMock, ports, pin);

            digitalOutputPort.Dispose();

            mcpMock.Verify(m => m.ResetPin(It.Is<IPin>(p => p == pin)), Times.Exactly(1));
        }

        [Fact]
        public void DisposeResetsPinOnlyOnce()
        {
            var ports = new Mcp23xPorts(new McpGpioPort());
            var pin = ports[0].GP0;
            var mcpMock = new Mock<IMcp23x>();
            mcpMock.Setup(m => m.ResetPin(It.IsAny<IPin>()));

            var digitalOutputPort = CreateOutputPort(mcpMock, ports, pin);

            digitalOutputPort.Dispose();
            // second call should not reset pin
            digitalOutputPort.Dispose();

            mcpMock.Verify(m => m.ResetPin(It.Is<IPin>(p => p == pin)), Times.Exactly(1));
        }

        [Fact]
        public void SettingStateChangesInstanceStateValue()
        {
            var digitalOutputPort = CreateOutputPort(initialState: false);
            Assert.False(digitalOutputPort.State);

            digitalOutputPort.State = true;
            Assert.True(digitalOutputPort.State);

            digitalOutputPort.State = false;
            Assert.False(digitalOutputPort.State);
        }

        [Fact]
        public void SettingStateWritesValueToPin()
        {
            var ports = new Mcp23xPorts(new McpGpioPort());
            var pin = ports[0].GP0;
            var mcpMock = new Mock<IMcp23x>();
            mcpMock.Setup(m => m.WritePin(It.Is<IPin>(p => p == pin), It.IsAny<bool>()));

            var digitalOutputPort = CreateOutputPort(mcpMock, ports, pin, false);

            digitalOutputPort.State = true;
            mcpMock.Verify(m => m.WritePin(It.Is<IPin>(p => p == pin), It.Is<bool>(b => b)), Times.Exactly(1));

            digitalOutputPort.State = false;
            mcpMock.Verify(m => m.WritePin(It.Is<IPin>(p => p == pin), It.Is<bool>(b => b == false)), Times.Exactly(2));
        }
    }
}
