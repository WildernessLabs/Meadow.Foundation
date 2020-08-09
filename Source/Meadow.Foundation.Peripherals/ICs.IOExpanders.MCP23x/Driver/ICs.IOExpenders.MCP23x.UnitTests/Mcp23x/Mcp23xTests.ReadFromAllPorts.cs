using System.Linq;
using Meadow.Foundation.ICs.IOExpanders.Ports;
using Meadow.Foundation.ICs.IOExpanders.UnitTests.Helpers;
using Moq;
using Xunit;

namespace Meadow.Foundation.ICs.IOExpanders.UnitTests.Mcp23x
{
    public partial class Mcp23xTests
    {
        private static void RegisterMapMockSparseOverride(Mock<IMcp23RegisterMap> mock)
        {
            mock.Setup(m => m.GetAddress(Mcp23PortRegister.GPIORegister, 0, It.IsAny<BankConfiguration>()))
                .Returns(0x01);
            mock.Setup(m => m.GetAddress(Mcp23PortRegister.GPIORegister, 1, It.IsAny<BankConfiguration>()))
                .Returns(0x11);
            mock.Setup(m => m.GetAddress(Mcp23PortRegister.GPIORegister, 2, It.IsAny<BankConfiguration>()))
                .Returns(0x21);
            mock.Setup(m => m.GetAddress(Mcp23PortRegister.GPIORegister, 3, It.IsAny<BankConfiguration>()))
                .Returns(0x31);
            mock.Setup(m => m.GetAddress(Mcp23PortRegister.IODirectionRegister, 0, It.IsAny<BankConfiguration>()))
                .Returns(0x03);
            mock.Setup(m => m.GetAddress(Mcp23PortRegister.IODirectionRegister, 1, It.IsAny<BankConfiguration>()))
                .Returns(0x13);
            mock.Setup(m => m.GetAddress(Mcp23PortRegister.IODirectionRegister, 2, It.IsAny<BankConfiguration>()))
                .Returns(0x23);
            mock.Setup(m => m.GetAddress(Mcp23PortRegister.IODirectionRegister, 3, It.IsAny<BankConfiguration>()))
                .Returns(0x33);
            mock.Setup(m =>
                    m.GetAddress(Mcp23PortRegister.InterruptControlRegister, 0, It.IsAny<BankConfiguration>()))
                .Returns(0x05);
            mock.Setup(m =>
                    m.GetAddress(Mcp23PortRegister.InterruptControlRegister, 1, It.IsAny<BankConfiguration>()))
                .Returns(0x15);
            mock.Setup(m =>
                    m.GetAddress(Mcp23PortRegister.InterruptControlRegister, 2, It.IsAny<BankConfiguration>()))
                .Returns(0x25);
            mock.Setup(m =>
                    m.GetAddress(Mcp23PortRegister.InterruptControlRegister, 3, It.IsAny<BankConfiguration>()))
                .Returns(0x35);
            mock.Setup(m => m.GetAddress(Mcp23PortRegister.InputPolarityRegister, 0, It.IsAny<BankConfiguration>()))
                .Returns(0x07);
            mock.Setup(m => m.GetAddress(Mcp23PortRegister.InputPolarityRegister, 1, It.IsAny<BankConfiguration>()))
                .Returns(0x17);
            mock.Setup(m => m.GetAddress(Mcp23PortRegister.InputPolarityRegister, 2, It.IsAny<BankConfiguration>()))
                .Returns(0x27);
            mock.Setup(m => m.GetAddress(Mcp23PortRegister.InputPolarityRegister, 3, It.IsAny<BankConfiguration>()))
                .Returns(0x37);
        }

        [Fact]
        public void ReadFromAllPortsInPairedModeSucceeds()
        {
            var mcp23x = Mcp23xTestImplementation.Create(4, 1,
                mock =>
                {
                    // map address to result
                    mock.Setup(m => m.ReadRegisters(It.IsAny<byte>(), It.IsAny<ushort>())).Returns(
                        (byte address, ushort length) =>
                            Enumerable.Range(0, length).Select(i => (byte) (address + i)).ToArray());
                },
                registerMapMockOverride: RegisterMapMockSparseOverride);

            Assert.Equal(BankConfiguration.Paired, mcp23x.BankConfiguration);

            var result = mcp23x.ReadFromAllPorts(Mcp23PortRegister.GPIORegister, Mcp23PortRegister.IODirectionRegister,
                Mcp23PortRegister.InterruptControlRegister, Mcp23PortRegister.InputPolarityRegister);

            Assert.Collection(result[0],
                b => Assert.Equal(0x01, b),
                b => Assert.Equal(0x03, b),
                b => Assert.Equal(0x05, b),
                b => Assert.Equal(0x07, b));

            Assert.Collection(result[1],
                b => Assert.Equal(0x11, b),
                b => Assert.Equal(0x13, b),
                b => Assert.Equal(0x15, b),
                b => Assert.Equal(0x17, b));

            Assert.Collection(result[2],
                b => Assert.Equal(0x21, b),
                b => Assert.Equal(0x23, b),
                b => Assert.Equal(0x25, b),
                b => Assert.Equal(0x27, b));

            Assert.Collection(result[3],
                b => Assert.Equal(0x31, b),
                b => Assert.Equal(0x33, b),
                b => Assert.Equal(0x35, b),
                b => Assert.Equal(0x37, b));
        }

        [Fact]
        public void ReadFromAllPortsInSegregatedModeSucceeds()
        {
            var mcp23x = Mcp23xTestImplementation.Create(4, 4,
                mock =>
                {
                    // map address to result
                    mock.Setup(m => m.ReadRegisters(It.IsAny<byte>(), It.IsAny<ushort>())).Returns(
                        (byte address, ushort length) =>
                            Enumerable.Range(0, length).Select(i => (byte) (address + i)).ToArray());
                },
                registerMapMockOverride: RegisterMapMockSparseOverride);

            Assert.Equal(BankConfiguration.Segregated, mcp23x.BankConfiguration);

            var result = mcp23x.ReadFromAllPorts(Mcp23PortRegister.GPIORegister, Mcp23PortRegister.IODirectionRegister,
                Mcp23PortRegister.InterruptControlRegister, Mcp23PortRegister.InputPolarityRegister);

            Assert.Collection(result[0],
                b => Assert.Equal(0x01, b),
                b => Assert.Equal(0x03, b),
                b => Assert.Equal(0x05, b),
                b => Assert.Equal(0x07, b));

            Assert.Collection(result[1],
                b => Assert.Equal(0x11, b),
                b => Assert.Equal(0x13, b),
                b => Assert.Equal(0x15, b),
                b => Assert.Equal(0x17, b));

            Assert.Collection(result[2],
                b => Assert.Equal(0x21, b),
                b => Assert.Equal(0x23, b),
                b => Assert.Equal(0x25, b),
                b => Assert.Equal(0x27, b));

            Assert.Collection(result[3],
                b => Assert.Equal(0x31, b),
                b => Assert.Equal(0x33, b),
                b => Assert.Equal(0x35, b),
                b => Assert.Equal(0x37, b));
        }

        [Fact]
        public void ReadFromAllPortsWithNoRegistersReturnsEmptyArrays()
        {
            var mcp23x = Mcp23xTestImplementation.Create(4, 4);
            var result = mcp23x.ReadFromAllPorts();

            Assert.Collection(result, Assert.Empty, Assert.Empty, Assert.Empty, Assert.Empty);
        }
    }
}