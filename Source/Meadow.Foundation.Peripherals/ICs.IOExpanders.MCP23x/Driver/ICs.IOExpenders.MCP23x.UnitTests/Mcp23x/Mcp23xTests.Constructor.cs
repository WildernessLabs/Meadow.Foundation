using System;
using System.Collections.Generic;
using System.Linq;
using Meadow.Foundation.ICs.IOExpanders.Device;
using Meadow.Foundation.ICs.IOExpanders.Ports;
using Meadow.Foundation.ICs.IOExpanders.UnitTests.Helpers;
using Meadow.Hardware;
using Moq;
using Xunit;

namespace Meadow.Foundation.ICs.IOExpanders.UnitTests.Mcp23x
{
    public partial class Mcp23xTests
    {
        [Fact]
        public void ConstructorWithListOfNullInterruptThrowsException()
        {
            var deviceCommsMock = new Mock<IMcpDeviceComms>();
            var portsMock = new Mock<IMcpGpioPorts>();
            var registerMapMock = new Mock<IMcp23RegisterMap>();
            var interrupts = new List<IDigitalInputPort> {null};
            var portsImpl = (IEnumerable<IMcpGpioPort>) new List<IMcpGpioPort> {new McpGpioPort()};
            portsMock.Setup(m => m.GetEnumerator()).Returns(portsImpl.GetEnumerator());

            Assert.Throws<ArgumentNullException>(() =>
                new Mcp23xTestImplementation(deviceCommsMock.Object, portsMock.Object, registerMapMock.Object,
                    interrupts));
        }

        [Theory]
        [InlineData(InterruptMode.EdgeBoth)]
        [InlineData(InterruptMode.EdgeFalling)]
        [InlineData(InterruptMode.None)]
        public void ConstructorThrowsWhenInterruptsAreNotEdgeRising(InterruptMode mode)
        {
            var deviceCommsMock = new Mock<IMcpDeviceComms>();
            var portsMock = new Mock<IMcpGpioPorts>();
            var registerMapMock = new Mock<IMcp23RegisterMap>();
            var interruptMock = new Mock<IDigitalInputPort>();
            interruptMock.SetupGet(m => m.InterruptMode).Returns(mode);
            var interrupts = new List<IDigitalInputPort> { interruptMock.Object };
            var portsImpl = (IEnumerable<IMcpGpioPort>)new List<IMcpGpioPort> { new McpGpioPort() };
            portsMock.Setup(m => m.GetEnumerator()).Returns(portsImpl.GetEnumerator());

            Assert.Throws<ArgumentException>(() =>
                new Mcp23xTestImplementation(deviceCommsMock.Object, portsMock.Object, registerMapMock.Object,
                    interrupts));
        }

        [Fact]
        public void ConstructorValidSucceeds()
        {
            Mcp23xTestImplementation.Create(1, 1);
        }

        [Fact]
        public void ConstructorWithTooManyInterruptsThrowsException()
        {
            var deviceCommsMock = new Mock<IMcpDeviceComms>();
            var portsMock = new Mock<IMcpGpioPorts>();
            var registerMapMock = new Mock<IMcp23RegisterMap>();
            var interruptMock = new Mock<IDigitalInputPort>();
            interruptMock.SetupGet(m => m.InterruptMode).Returns(InterruptMode.EdgeRising);
            var interrupts = new List<IDigitalInputPort> { interruptMock.Object, interruptMock.Object, interruptMock.Object };
            var portsImpl = new List<IMcpGpioPort> { new McpGpioPort(), new McpGpioPort() };
            portsMock.Setup(m => m.GetEnumerator()).Returns(portsImpl.GetEnumerator());
            portsMock.SetupGet(m => m.Count).Returns(portsImpl.Count);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => new Mcp23xTestImplementation(deviceCommsMock.Object, portsMock.Object, registerMapMock.Object,
                    interrupts)
            );
        }

        [Fact]
        public void ConstructorWithListOfNullPortsThrowsException()
        {
            var deviceCommsMock = new Mock<IMcpDeviceComms>();
            var portsMock = new Mock<IMcpGpioPorts>();
            var registerMapMock = new Mock<IMcp23RegisterMap>();
            var interruptsMock = new Mock<IList<IDigitalInputPort>>();

            var portsImpl = (IList<IMcpGpioPort>) new List<IMcpGpioPort>
            {
                null
            };

            Assert.False(portsImpl.All(p => p != null));

            portsMock.Setup(m => m.GetEnumerator()).Returns(portsImpl.GetEnumerator());
            Assert.False(portsMock.Object.All(p => p != null));
            Assert.Throws<ArgumentNullException>(() =>
                new Mcp23xTestImplementation(deviceCommsMock.Object, portsMock.Object, registerMapMock.Object,
                    interruptsMock.Object));
        }

        [Fact]
        public void ConstructorWithNoPortsThrowsException()
        {
            var deviceCommsMock = new Mock<IMcpDeviceComms>();
            var portsMock = new Mock<IMcpGpioPorts>();
            var registerMapMock = new Mock<IMcp23RegisterMap>();
            var interruptsMock = new Mock<IList<IDigitalInputPort>>();

            var portsImpl = (IEnumerable<IMcpGpioPort>) new List<IMcpGpioPort>();
            portsMock.Setup(m => m.GetEnumerator()).Returns(portsImpl.GetEnumerator());

            Assert.Throws<ArgumentNullException>(() =>
                new Mcp23xTestImplementation(deviceCommsMock.Object, portsMock.Object, registerMapMock.Object,
                    interruptsMock.Object));
        }

        [Fact]
        public void ConstructorWithNullDeviceCommsThrowsException()
        {
            var deviceComms = (IMcpDeviceComms) null;
            var portsMock = new Mock<IMcpGpioPorts>();
            var registerMapMock = new Mock<IMcp23RegisterMap>();
            var interruptsMock = new Mock<IList<IDigitalInputPort>>();

            Assert.Throws<ArgumentNullException>(() =>
                new Mcp23xTestImplementation(deviceComms, portsMock.Object, registerMapMock.Object,
                    interruptsMock.Object));
        }
        //private static Mcp23xTestImplementation CreateMcp23x()

        [Fact]
        public void ConstructorWithNullInterruptThrowsException()
        {
            var deviceCommsMock = new Mock<IMcpDeviceComms>();
            var portsMock = new Mock<IMcpGpioPorts>();
            var registerMapMock = new Mock<IMcp23RegisterMap>();
            var interrupts = (IList<IDigitalInputPort>) null;

            Assert.Throws<ArgumentNullException>(() =>
                new Mcp23xTestImplementation(deviceCommsMock.Object, portsMock.Object, registerMapMock.Object,
                    interrupts));
        }

        [Fact]
        public void ConstructorWithNullPortsThrowsException()
        {
            var deviceCommsMock = new Mock<IMcpDeviceComms>();
            var ports = (IMcpGpioPorts) null;
            var registerMapMock = new Mock<IMcp23RegisterMap>();
            var interruptsMock = new Mock<IList<IDigitalInputPort>>();

            Assert.Throws<ArgumentNullException>(() =>
                new Mcp23xTestImplementation(deviceCommsMock.Object, ports, registerMapMock.Object,
                    interruptsMock.Object));
        }

        [Fact]
        public void ConstructorWithNullRegisterMapThrowsException()
        {
            var deviceCommsMock = new Mock<IMcpDeviceComms>();
            var portsMock = new Mock<IMcpGpioPorts>();
            var registerMap = (IMcp23RegisterMap) null;
            var interruptsMock = new Mock<IList<IDigitalInputPort>>();

            Assert.Throws<ArgumentNullException>(() =>
                new Mcp23xTestImplementation(deviceCommsMock.Object, portsMock.Object, registerMap,
                    interruptsMock.Object));
        }
    }
}