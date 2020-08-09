using System;
using System.Linq;
using Meadow.Foundation.ICs.IOExpanders.Ports;
using Meadow.Foundation.ICs.IOExpanders.UnitTests.Helpers;
using Moq;
using Xunit;

namespace Meadow.Foundation.ICs.IOExpanders.UnitTests.Mcp23x
{
    public partial class Mcp23xTests
    {
        private static void RegisterMapMockRegisterCloseOverride(Mock<IMcp23RegisterMap> mock)
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
                .Returns(0x02);
            mock.Setup(m => m.GetAddress(Mcp23PortRegister.IODirectionRegister, 1, It.IsAny<BankConfiguration>()))
                .Returns(0x12);
            mock.Setup(m => m.GetAddress(Mcp23PortRegister.IODirectionRegister, 2, It.IsAny<BankConfiguration>()))
                .Returns(0x22);
            mock.Setup(m => m.GetAddress(Mcp23PortRegister.IODirectionRegister, 3, It.IsAny<BankConfiguration>()))
                .Returns(0x32);
            mock.Setup(m =>
                    m.GetAddress(Mcp23PortRegister.InterruptControlRegister, 0, It.IsAny<BankConfiguration>()))
                .Returns(0x03);
            mock.Setup(m =>
                    m.GetAddress(Mcp23PortRegister.InterruptControlRegister, 1, It.IsAny<BankConfiguration>()))
                .Returns(0x13);
            mock.Setup(m =>
                    m.GetAddress(Mcp23PortRegister.InterruptControlRegister, 2, It.IsAny<BankConfiguration>()))
                .Returns(0x23);
            mock.Setup(m =>
                    m.GetAddress(Mcp23PortRegister.InterruptControlRegister, 3, It.IsAny<BankConfiguration>()))
                .Returns(0x33);
            mock.Setup(m => m.GetAddress(Mcp23PortRegister.InputPolarityRegister, 0, It.IsAny<BankConfiguration>()))
                .Returns(0x04);
            mock.Setup(m => m.GetAddress(Mcp23PortRegister.InputPolarityRegister, 1, It.IsAny<BankConfiguration>()))
                .Returns(0x14);
            mock.Setup(m => m.GetAddress(Mcp23PortRegister.InputPolarityRegister, 2, It.IsAny<BankConfiguration>()))
                .Returns(0x24);
            mock.Setup(m => m.GetAddress(Mcp23PortRegister.InputPolarityRegister, 3, It.IsAny<BankConfiguration>()))
                .Returns(0x34);
        }

        [Fact]
        public void ReadRegistersInSingleSequenceSucceeds()
        {
            var mcp23x = Mcp23xTestImplementation.Create(4, 1,
                mock =>
                {
                    // map address to result
                    mock.Setup(m => m.ReadRegisters(It.IsAny<byte>(), It.IsAny<ushort>())).Returns(
                        (byte address, ushort length) =>
                            Enumerable.Range(0, length).Select(i => (byte)(address + i)).ToArray());
                },
                registerMapMockOverride: RegisterMapMockRegisterCloseOverride);



            var result = mcp23x.ReadRegisters((Mcp23PortRegister.GPIORegister, 1),
                (Mcp23PortRegister.IODirectionRegister, 1), (Mcp23PortRegister.InterruptControlRegister, 1),
                (Mcp23PortRegister.InputPolarityRegister, 1));

            Assert.Collection(result,
                b => Assert.Equal(0x11, b),
                b => Assert.Equal(0x12, b),
                b => Assert.Equal(0x13, b),
                b => Assert.Equal(0x14, b));

            mcp23x.DeviceCommsMock.Verify(m => m.ReadRegisters(It.IsAny<byte>(), It.IsAny<ushort>()), Times.Once);
        }

        [Fact]
        public void ReadRegistersByPortInSingleSequenceSucceeds()
        {
            var mcp23x = Mcp23xTestImplementation.Create(4, 1,
                mock =>
                {
                    // map address to result
                    mock.Setup(m => m.ReadRegisters(It.IsAny<byte>(), It.IsAny<ushort>())).Returns(
                        (byte address, ushort length) =>
                            Enumerable.Range(0, length).Select(i => (byte)(address + i)).ToArray());
                },
                registerMapMockOverride: RegisterMapMockRegisterCloseOverride);



            var result = mcp23x.ReadRegisters(1, Mcp23PortRegister.GPIORegister, Mcp23PortRegister.IODirectionRegister,
                Mcp23PortRegister.InterruptControlRegister, Mcp23PortRegister.InputPolarityRegister);

            Assert.Collection(result,
                b => Assert.Equal(0x11, b),
                b => Assert.Equal(0x12, b),
                b => Assert.Equal(0x13, b),
                b => Assert.Equal(0x14, b));

            mcp23x.DeviceCommsMock.Verify(m => m.ReadRegisters(It.IsAny<byte>(), It.IsAny<ushort>()), Times.Once);
        }

        [Fact]
        public void ReadRegistersInGroupsOfSequencesSucceeds()
        {
            var mcp23x = Mcp23xTestImplementation.Create(4, 1,
                mock =>
                {
                    // map address to result
                    mock.Setup(m => m.ReadRegisters(It.IsAny<byte>(), It.IsAny<ushort>())).Returns(
                        (byte address, ushort length) =>
                            Enumerable.Range(0, length).Select(i => (byte)(address + i)).ToArray());
                },
                registerMapMockOverride: RegisterMapMockRegisterCloseOverride);

            var result = mcp23x.ReadRegisters((Mcp23PortRegister.GPIORegister, 1),
                (Mcp23PortRegister.IODirectionRegister, 1), (Mcp23PortRegister.InterruptControlRegister, 2),
                (Mcp23PortRegister.InputPolarityRegister, 2));

            Assert.Collection(result,
                b => Assert.Equal(0x11, b),
                b => Assert.Equal(0x12, b),
                b => Assert.Equal(0x23, b),
                b => Assert.Equal(0x24, b));

            mcp23x.DeviceCommsMock.Verify(m => m.ReadRegisters(It.IsAny<byte>(), It.IsAny<ushort>()), Times.Exactly(2));
        }


        [Fact]
        public void ReadRegistersByPortInGroupsOfSequencesSucceeds()
        {
            var mcp23x = Mcp23xTestImplementation.Create(4, 1,
                mock =>
                {
                    // map address to result
                    mock.Setup(m => m.ReadRegisters(It.IsAny<byte>(), It.IsAny<ushort>())).Returns(
                        (byte address, ushort length) =>
                            Enumerable.Range(0, length).Select(i => (byte)(address + i)).ToArray());
                },
                registerMapMockOverride: RegisterMapMockRegisterCloseOverride);

            var result = mcp23x.ReadRegisters(1, Mcp23PortRegister.GPIORegister, Mcp23PortRegister.IODirectionRegister, Mcp23PortRegister.IODirectionRegister, Mcp23PortRegister.InterruptControlRegister,Mcp23PortRegister.InputPolarityRegister);

            Assert.Collection(result,
                b => Assert.Equal(0x11, b),
                b => Assert.Equal(0x12, b),
                b => Assert.Equal(0x12, b),
                b => Assert.Equal(0x13, b),
                b => Assert.Equal(0x14, b));

            mcp23x.DeviceCommsMock.Verify(m => m.ReadRegisters(It.IsAny<byte>(), It.IsAny<ushort>()), Times.Exactly(2));
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        [InlineData(-1)]
        [InlineData(4)]
        public void ReadRegistersWithInvalidPortThrowsException(int port)
        {
            var mcp23x = Mcp23xTestImplementation.Create(4, 1,
                registerMapMockOverride: RegisterMapMockRegisterCloseOverride);
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                mcp23x.ReadRegisters((Mcp23PortRegister.GPIORegister, port)));
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        [InlineData(-1)]
        [InlineData(4)]
        public void ReadRegistersByPortWithInvalidPortThrowsException(int port)
        {
            var mcp23x = Mcp23xTestImplementation.Create(4, 1,
                registerMapMockOverride: RegisterMapMockRegisterCloseOverride);
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                mcp23x.ReadRegisters(port, Mcp23PortRegister.GPIORegister));
        }

        [Fact]
        public void ReadRegistersWithNoRegistersReturnsEmptyArray()
        {
            var mcp23x = Mcp23xTestImplementation.Create(4, 4);
            var result = mcp23x.ReadRegisters();

            Assert.Empty(result);
        }

        [Fact]
        public void ReadRegistersByPortWithNoRegistersReturnsEmptyArray()
        {
            var mcp23x = Mcp23xTestImplementation.Create(4, 4);
            var result = mcp23x.ReadRegisters(1);

            Assert.Empty(result);
        }
    }
}