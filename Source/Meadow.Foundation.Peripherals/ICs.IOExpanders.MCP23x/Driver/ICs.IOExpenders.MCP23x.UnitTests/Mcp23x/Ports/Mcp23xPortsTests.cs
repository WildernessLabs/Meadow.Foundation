using System;
using System.Collections.Generic;
using System.Linq;
using Meadow.Foundation.ICs.IOExpanders.Ports;
using Xunit;

namespace Meadow.Foundation.ICs.IOExpanders.UnitTests.Mcp23x.Ports
{
    public class Mcp23xPortsTests
    {
        public static IEnumerable<object[]> MemberData =>
            new[]
            {
                new[] { new[] { new McpGpioPort("0") } },
                new[] { new[] { new McpGpioPort("0"), new McpGpioPort("1") } },
                new[] { new[] { new McpGpioPort("0"), new McpGpioPort("1"), new McpGpioPort("2") } },
                new[]
                {
                    new[] { new McpGpioPort("0"), new McpGpioPort("1"), new McpGpioPort("2"), new McpGpioPort("3") }
                }
            };

        [Theory]
        [MemberData(nameof(MemberData))]
        public void AllPortsInConstructorAreAccessible(IMcpGpioPort[] ports)
        {
            var portCollection = new Mcp23xPorts(ports);

            for (var i = 0; i < ports.Length; i++)
            {
                Assert.Equal(ports[i], portCollection[i]);
            }

            Assert.Equal(ports.Length, portCollection.Count);
        }

        [Theory]
        [MemberData(nameof(MemberData))]
        public void AllPinsContainsAllPinsFromAllPorts(IMcpGpioPort[] ports)
        {
            var portCollection = new Mcp23xPorts(ports);

            Assert.Equal(portCollection.AllPins, ports.SelectMany(p => p.AllPins));
        }

        [Theory]
        [MemberData(nameof(MemberData))]
        public void GetPortIndexReturnsCorrectIndex(IMcpGpioPort[] ports)
        {
            var portCollection = new Mcp23xPorts(ports);

            for (var i = 0; i < ports.Length; i++)
            {
                Assert.Equal(i, portCollection.GetPortIndex(ports[i]));
            }
        }

        [Theory]
        [MemberData(nameof(MemberData))]
        public void GetPortIndexOfPinReturnsCorrectIndex(IMcpGpioPort[] ports)
        {
            var portCollection = new Mcp23xPorts(ports);

            for (var i = 0; i < ports.Length; i++)
            {
                var port = ports[i];
                foreach (var pin in port.AllPins)
                {
                    Assert.Equal(i, portCollection.GetPortIndexOfPin(pin));
                }
            }
        }

        [Theory]
        [MemberData(nameof(MemberData))]
        public void GetPortOfPinReturnsCorrectIndex(IMcpGpioPort[] ports)
        {
            var portCollection = new Mcp23xPorts(ports);

            foreach (var port in ports)
            {
                foreach (var pin in port.AllPins)
                {
                    Assert.Equal(port, portCollection.GetPortOfPin(pin));
                }
            }
        }

        [Theory]
        [MemberData(nameof(MemberData))]
        public void GetPortIndexThrowsIfPortIsNull(IMcpGpioPort[] ports)
        {
            var portCollection = new Mcp23xPorts(ports);

            Assert.Throws<ArgumentNullException>(() => portCollection.GetPortIndex(null));
        }

        [Theory]
        [MemberData(nameof(MemberData))]
        public void GetPortIndexOfPinThrowsIfPinIsNull(IMcpGpioPort[] ports)
        {
            var portCollection = new Mcp23xPorts(ports);

            Assert.Throws<ArgumentNullException>(() => portCollection.GetPortIndexOfPin(null));
        }

        [Theory]
        [MemberData(nameof(MemberData))]
        public void GetPortOfPinThrowsIfPinIsNull(IMcpGpioPort[] ports)
        {
            var portCollection = new Mcp23xPorts(ports);

            Assert.Throws<ArgumentNullException>(() => portCollection.GetPortOfPin(null));
        }


        [Theory]
        [MemberData(nameof(MemberData))]
        public void GetPortIndexThrowsIfPortIsUnknown(IMcpGpioPort[] ports)
        {
            var portCollection = new Mcp23xPorts(ports);
            var unknownPort = new McpGpioPort("Unknown");

            Assert.Throws<ArgumentException>(() => portCollection.GetPortIndex(unknownPort));
        }

        [Theory]
        [MemberData(nameof(MemberData))]
        public void GetPortIndexOfPinThrowsIfPinIsUnknown(IMcpGpioPort[] ports)
        {
            var portCollection = new Mcp23xPorts(ports);
            var unknownPin = new McpGpioPort("Unknown").GP0;

            Assert.Throws<ArgumentException>(() => portCollection.GetPortIndexOfPin(unknownPin));
        }

        [Theory]
        [MemberData(nameof(MemberData))]
        public void GetPortOfPinThrowsIfPinIsUnknown(IMcpGpioPort[] ports)
        {
            var portCollection = new Mcp23xPorts(ports);
            var unknownPin = new McpGpioPort("Unknown").GP0;

            Assert.Throws<ArgumentException>(() => portCollection.GetPortOfPin(unknownPin));
        }

        [Theory]
        [MemberData(nameof(MemberData))]
        public void GetEnumeratorReturnsEnumerator(IMcpGpioPort[] ports)
        {
            var portCollection = new Mcp23xPorts(ports);

            Assert.NotNull(portCollection.GetEnumerator());
            Assert.True(portCollection.Any());
        }

        [Fact]
        public void ConstructorThrowsIfAnyPortsAreNull()
        {
            Assert.Throws<ArgumentNullException>(() => new Mcp23xPorts(new IMcpGpioPort[] { null }));
        }

        [Fact]
        public void ConstructorThrowsIfPortsIsEmpty()
        {
            var emptyPorts = new IMcpGpioPort[0];
            Assert.Throws<ArgumentException>(() => new Mcp23xPorts());
            Assert.Throws<ArgumentException>(() => new Mcp23xPorts(emptyPorts));
        }


        [Fact]
        public void ConstructorThrowsIfPortsIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new Mcp23xPorts(null));
        }
    }
}
