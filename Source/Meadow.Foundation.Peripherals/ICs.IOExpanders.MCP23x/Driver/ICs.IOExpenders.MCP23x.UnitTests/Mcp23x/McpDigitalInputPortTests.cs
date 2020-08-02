using System;
using Meadow.Foundation.ICs.IOExpanders.Ports;
using Meadow.Foundation.ICs.IOExpanders.UnitTests.Helpers;
using Meadow.Hardware;
using Meadow.Utilities;
using Moq;
using Xunit;

namespace Meadow.Foundation.ICs.IOExpanders.UnitTests.Mcp23x
{
    public class McpDigitalInputPortTests
    {
        private static McpDigitalInputPort CreateInputPort(
            Mock<IMcp23x> mcpMock = null,
            IMcpGpioPorts ports = null,
            IPin pin = null,
            InterruptMode interruptMode = InterruptMode.None)
        {
            mcpMock = mcpMock ?? new Mock<IMcp23x>();
            ports = ports ?? new Mcp23xPorts(new McpGpioPort());
            pin = pin ?? ports[0].GP0;

            mcpMock.SetupGet(x => x.Ports).Returns(ports);

            return new McpDigitalInputPort(mcpMock.Object, pin, interruptMode);
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

            var digitalInputPort = CreateInputPort(mcpMock, ports, pin);

            Assert.Equal(pinState, digitalInputPort.State);
        }

        [Theory]
        [InlineData(InterruptMode.EdgeRising, 1)]
        [InlineData(InterruptMode.EdgeBoth, 1)]
        [InlineData(InterruptMode.EdgeFalling, 1)]
        [InlineData(InterruptMode.None, 0)]
        public void PortInputChangedEventIsSubscribedToWhenInterruptModeIsNotNone(
            InterruptMode interruptMode,
            int eventAddedCount)
        {
            var backingPort = new McpGpioPort();
            var portMock = new Mock<IMcpGpioPort>();
            portMock.SetupGet(m => m.GP0).Returns(backingPort.GP0);
            portMock.SetupGet(m => m.AllPins).Returns(backingPort.AllPins);
            portMock.SetupAdd(m => m.InputChanged += (sender, args) => { });
            var ports = new Mcp23xPorts(portMock.Object);

            var digitalInputPort = CreateInputPort(ports: ports, interruptMode: interruptMode);

            portMock.VerifyAdd(
                m => m.InputChanged += It.IsAny<EventHandler<IOExpanderPortInputChangedEventArgs>>(),
                Times.Exactly(eventAddedCount));
        }

        [Theory]
        [InlineData(InterruptMode.EdgeRising)]
        [InlineData(InterruptMode.EdgeBoth)]
        [InlineData(InterruptMode.EdgeFalling)]
        public void PortRemovesEventHandlerOnDispose(InterruptMode interruptMode)
        {
            var backingPort = new McpGpioPort();
            var portMock = new Mock<IMcpGpioPort>();
            portMock.SetupGet(m => m.AllPins).Returns(backingPort.AllPins);
            portMock.SetupAdd(m => m.InputChanged += (sender, args) => { });
            var ports = new Mcp23xPorts(portMock.Object);
            var pin1 = backingPort.GP1;

            var digitalInputPort1 = CreateInputPort(ports: ports, pin: pin1, interruptMode: interruptMode);

            portMock.VerifyAdd(
                m => m.InputChanged += It.IsAny<EventHandler<IOExpanderPortInputChangedEventArgs>>(),
                Times.Exactly(1));

            digitalInputPort1.Dispose();

            portMock.VerifyRemove(
                m => m.InputChanged -= It.IsAny<EventHandler<IOExpanderPortInputChangedEventArgs>>(),
                Times.Exactly(1));
        }


        [Theory]
        [InlineData(InterruptMode.EdgeRising)]
        [InlineData(InterruptMode.EdgeBoth)]
        [InlineData(InterruptMode.EdgeFalling)]
        public void PortRemovesOnlyItsOwnEventHandlerOnDispose(InterruptMode interruptMode)
        {
            var backingPort = new McpGpioPort();
            var portMock = new Mock<IMcpGpioPort>();
            portMock.SetupGet(m => m.AllPins).Returns(backingPort.AllPins);
            portMock.SetupAdd(m => m.InputChanged += (sender, args) => { });
            var ports = new Mcp23xPorts(portMock.Object);
            var pin1 = backingPort.GP1;
            var pin2 = backingPort.GP2;

            var digitalInputPort1 = CreateInputPort(ports: ports, pin: pin1, interruptMode: interruptMode);
            var digitalInputPort2 = CreateInputPort(ports: ports, pin: pin2, interruptMode: interruptMode);

            portMock.VerifyAdd(
                m => m.InputChanged += It.IsAny<EventHandler<IOExpanderPortInputChangedEventArgs>>(),
                Times.Exactly(2));

            digitalInputPort1.Dispose();

            portMock.VerifyRemove(
                m => m.InputChanged -= It.IsAny<EventHandler<IOExpanderPortInputChangedEventArgs>>(),
                Times.Exactly(1));

            digitalInputPort2.Dispose();
            portMock.VerifyRemove(
                m => m.InputChanged -= It.IsAny<EventHandler<IOExpanderPortInputChangedEventArgs>>(),
                Times.Exactly(2));
        }


        [Fact]
        public void ConstructorThrowsIfPinIsFromDifferentController()
        {
            var ports = new Mcp23xPorts(new McpGpioPort(), new McpGpioPort());
            var pin = new McpGpioPort().GP1;

            Assert.Throws<ArgumentException>(() => CreateInputPort(ports: ports, pin: pin));
        }


        [Fact]
        public void InputChangedForDifferentPinDoesNotRaiseEvent()
        {
            var port = new McpGpioPort();
            var pin = port.GP0;
            var pinKey = (byte) ((byte) pin.Key + 1);
            var digitalInputPort = CreateInputPort(
                ports: new Mcp23xPorts(port),
                pin: pin,
                interruptMode: InterruptMode.EdgeRising);

            AssertHelpers.DoesNotRaise<DigitalInputPortEventArgs>(
                handler => digitalInputPort.Changed += handler,
                handler => digitalInputPort.Changed -= handler,
                () => port.InvokeInputChanged(
                    new IOExpanderPortInputChangedEventArgs(
                        BitHelpers.SetBit(pinKey, (byte) pin.Key, true),
                        0x00)));
        }


        [Fact]
        public void InputChangedForEdgeBothOnlyRaisesEventOnStatusChange()
        {
            var port = new McpGpioPort();
            var pin = port.GP0;
            var digitalInputPort = CreateInputPort(
                ports: new Mcp23xPorts(port),
                pin: pin,
                interruptMode: InterruptMode.EdgeBoth);

            var raisedTrue = Assert.Raises<DigitalInputPortEventArgs>(
                handler => digitalInputPort.Changed += handler,
                handler => digitalInputPort.Changed -= handler,
                () => port.InvokeInputChanged(
                    new IOExpanderPortInputChangedEventArgs(
                        BitHelpers.SetBit(0x00, (byte) pin.Key, true),
                        0xFF)));
            Assert.True(raisedTrue.Arguments.Value);

            AssertHelpers.DoesNotRaise<DigitalInputPortEventArgs>(
                handler => digitalInputPort.Changed += handler,
                handler => digitalInputPort.Changed -= handler,
                () => port.InvokeInputChanged(
                    new IOExpanderPortInputChangedEventArgs(
                        BitHelpers.SetBit(0x00, (byte) pin.Key, true),
                        0xFF)));

            var raisedFalse = Assert.Raises<DigitalInputPortEventArgs>(
                handler => digitalInputPort.Changed += handler,
                handler => digitalInputPort.Changed -= handler,
                () => port.InvokeInputChanged(
                    new IOExpanderPortInputChangedEventArgs(
                        BitHelpers.SetBit(0x00, (byte) pin.Key, true),
                        0x00)));
            Assert.False(raisedFalse.Arguments.Value);


            AssertHelpers.DoesNotRaise<DigitalInputPortEventArgs>(
                handler => digitalInputPort.Changed += handler,
                handler => digitalInputPort.Changed -= handler,
                () => port.InvokeInputChanged(
                    new IOExpanderPortInputChangedEventArgs(
                        BitHelpers.SetBit(0x00, (byte) pin.Key, true),
                        0x00)));
        }

        [Fact]
        public void InputChangedForEdgeBothRaisesEvent()
        {
            var port = new McpGpioPort();
            var pin = port.GP0;
            var digitalInputPort = CreateInputPort(
                ports: new Mcp23xPorts(port),
                pin: pin,
                interruptMode: InterruptMode.EdgeBoth);

            var raisedTrue = Assert.Raises<DigitalInputPortEventArgs>(
                handler => digitalInputPort.Changed += handler,
                handler => digitalInputPort.Changed -= handler,
                () => port.InvokeInputChanged(
                    new IOExpanderPortInputChangedEventArgs(
                        BitHelpers.SetBit(0x00, (byte) pin.Key, true),
                        0xFF)));
            Assert.True(raisedTrue.Arguments.Value);

            var raisedFalse = Assert.Raises<DigitalInputPortEventArgs>(
                handler => digitalInputPort.Changed += handler,
                handler => digitalInputPort.Changed -= handler,
                () => port.InvokeInputChanged(
                    new IOExpanderPortInputChangedEventArgs(
                        BitHelpers.SetBit(0x00, (byte) pin.Key, true),
                        0x00)));
            Assert.False(raisedFalse.Arguments.Value);
        }


        [Fact]
        public void InputChangedForEdgeFallingRaisesEvent()
        {
            var port = new McpGpioPort();
            var pin = port.GP0;
            var digitalInputPort = CreateInputPort(
                ports: new Mcp23xPorts(port),
                pin: pin,
                interruptMode: InterruptMode.EdgeFalling);

            AssertHelpers.DoesNotRaise<DigitalInputPortEventArgs>(
                handler => digitalInputPort.Changed += handler,
                handler => digitalInputPort.Changed -= handler,
                () => port.InvokeInputChanged(
                    new IOExpanderPortInputChangedEventArgs(
                        BitHelpers.SetBit(0x00, (byte) pin.Key, true),
                        0xFF)));

            var raisedTrue = Assert.Raises<DigitalInputPortEventArgs>(
                handler => digitalInputPort.Changed += handler,
                handler => digitalInputPort.Changed -= handler,
                () => port.InvokeInputChanged(
                    new IOExpanderPortInputChangedEventArgs(
                        BitHelpers.SetBit(0x00, (byte) pin.Key, true),
                        0x00)));
            Assert.False(raisedTrue.Arguments.Value);

            AssertHelpers.DoesNotRaise<DigitalInputPortEventArgs>(
                handler => digitalInputPort.Changed += handler,
                handler => digitalInputPort.Changed -= handler,
                () => port.InvokeInputChanged(
                    new IOExpanderPortInputChangedEventArgs(
                        BitHelpers.SetBit(0x00, (byte) pin.Key, true),
                        0xFF)));

            raisedTrue = Assert.Raises<DigitalInputPortEventArgs>(
                handler => digitalInputPort.Changed += handler,
                handler => digitalInputPort.Changed -= handler,
                () => port.InvokeInputChanged(
                    new IOExpanderPortInputChangedEventArgs(
                        BitHelpers.SetBit(0x00, (byte) pin.Key, true),
                        0x00)));
            Assert.False(raisedTrue.Arguments.Value);
        }

        [Fact]
        public void InputChangedForEdgeRisingRaisesEvent()
        {
            var port = new McpGpioPort();
            var pin = port.GP0;
            var digitalInputPort = CreateInputPort(
                ports: new Mcp23xPorts(port),
                pin: pin,
                interruptMode: InterruptMode.EdgeRising);

            var raisedTrue = Assert.Raises<DigitalInputPortEventArgs>(
                handler => digitalInputPort.Changed += handler,
                handler => digitalInputPort.Changed -= handler,
                () => port.InvokeInputChanged(
                    new IOExpanderPortInputChangedEventArgs(
                        BitHelpers.SetBit(0x00, (byte) pin.Key, true),
                        0xFF)));
            Assert.True(raisedTrue.Arguments.Value);

            AssertHelpers.DoesNotRaise<DigitalInputPortEventArgs>(
                handler => digitalInputPort.Changed += handler,
                handler => digitalInputPort.Changed -= handler,
                () => port.InvokeInputChanged(
                    new IOExpanderPortInputChangedEventArgs(
                        BitHelpers.SetBit(0x00, (byte) pin.Key, true),
                        0x00)));

            raisedTrue = Assert.Raises<DigitalInputPortEventArgs>(
                handler => digitalInputPort.Changed += handler,
                handler => digitalInputPort.Changed -= handler,
                () => port.InvokeInputChanged(
                    new IOExpanderPortInputChangedEventArgs(
                        BitHelpers.SetBit(0x00, (byte) pin.Key, true),
                        0xFF)));
            Assert.True(raisedTrue.Arguments.Value);
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
