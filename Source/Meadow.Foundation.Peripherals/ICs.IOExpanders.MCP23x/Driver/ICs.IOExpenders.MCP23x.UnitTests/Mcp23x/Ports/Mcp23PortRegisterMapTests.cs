using System;
using System.Collections.Generic;
using System.Linq;
using Meadow.Foundation.ICs.IOExpanders.Ports;
using Xunit;

namespace Meadow.Foundation.ICs.IOExpanders.UnitTests.Mcp23x.Ports
{
    public class Mcp23PortRegisterMapTests
    {
        public static IEnumerable<object[]> AllRegisters =
            Enum.GetValues(typeof(Mcp23PortRegister)).Cast<Mcp23PortRegister>().Select(r => new object[] { r });

        public static IEnumerable<object[]> ValidRegisters =
            Enum.GetValues(typeof(Mcp23PortRegister))
                .Cast<Mcp23PortRegister>()
                .Where(r => r != Mcp23PortRegister.Invalid)
                .Select(r => new object[] { r });

        public static IEnumerable<(byte address, Mcp23PortRegister register)> DefaultMapping =>
            new[]
            {
                (address: (byte) 0x10, register: Mcp23PortRegister.IODirectionRegister),
                (address: (byte) 0x11, register: Mcp23PortRegister.InputPolarityRegister),
                (address: (byte) 0x12, register: Mcp23PortRegister.InterruptOnChangeRegister),
                (address: (byte) 0x13, register: Mcp23PortRegister.DefaultComparisonValueRegister),
                (address: (byte) 0x14, register: Mcp23PortRegister.InterruptControlRegister),
                (address: (byte) 0x15, register: Mcp23PortRegister.IOConfigurationRegister),
                (address: (byte) 0x16, register: Mcp23PortRegister.PullupResistorConfigurationRegister),
                (address: (byte) 0x17, register: Mcp23PortRegister.InterruptFlagRegister),
                (address: (byte) 0x18, register: Mcp23PortRegister.InterruptCaptureRegister),
                (address: (byte) 0x19, register: Mcp23PortRegister.GPIORegister),
                (address: (byte) 0x1A, register: Mcp23PortRegister.OutputLatchRegister)
            };


        [Theory]
        [MemberData(nameof(ValidRegisters))]
        public void MappingAValidRegisterMultipleTimesThrows(Mcp23PortRegister register)
        {
            var mapping = new[]
            {
                (address: (byte) 0x01, register),
                (address: (byte) 0x02, register)
            };

            Assert.Throws<ArgumentException>(() => new Mcp23PortRegisterMap(mapping));
        }

        [Theory]
        [InlineData(Mcp23PortRegister.Invalid)]
        [InlineData((Mcp23PortRegister) (-1))]
        [InlineData((Mcp23PortRegister) 13)]
        [InlineData((Mcp23PortRegister) int.MaxValue)]
        [InlineData((Mcp23PortRegister) int.MinValue)]
        public void GetAddressThrowsIfRegisterIsInvalid(Mcp23PortRegister register)
        {
            var registerMap = new Mcp23PortRegisterMap(new (byte address, Mcp23PortRegister register)[0]);

            Assert.Throws<ArgumentOutOfRangeException>(() => registerMap.GetAddress(register));
        }


        [Theory]
        [MemberData(nameof(ValidRegisters))]
        public void GetAddressReturnsCorrectByteAddress(Mcp23PortRegister register)
        {
            var registerMap = new Mcp23PortRegisterMap(DefaultMapping);

            Assert.Equal(
                DefaultMapping.Single(map => map.register == register).address,
                registerMap.GetAddress(register));
        }


        [Fact]
        public void ConstructorThrowsIfMappingIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new Mcp23PortRegisterMap(null));
        }


        [Fact]
        public void GetNextRegisterReturnsInvalidForUnknownAddresses()
        {
            var registerMap = new Mcp23PortRegisterMap(DefaultMapping);
            var invalidAddresses = Enumerable.Range(0, 256)
                .Where(i => i == byte.MaxValue || DefaultMapping.All(map => map.address != i + 1));

            foreach (var invalidAddress in invalidAddresses)
            {
                Assert.Equal(Mcp23PortRegister.Invalid, registerMap.GetNextRegister((byte) invalidAddress));
            }
        }

        [Fact]
        public void GetNextRegisterReturnsNextRegister()
        {
            var registerMap = new Mcp23PortRegisterMap(DefaultMapping);
            var addresses = Enumerable.Range(0, 256)
                .Where(i => i > 0 && DefaultMapping.Any(map => map.address == i));

            foreach (var address in addresses)
            {
                Assert.Equal(
                    DefaultMapping.Single(map => map.address == address).register,
                    registerMap.GetNextRegister((byte) (address - 1)));
            }
        }

        [Fact]
        public void GetRegisterAtAddressReturnsInvalidForUnknownAddresses()
        {
            var registerMap = new Mcp23PortRegisterMap(DefaultMapping);
            var invalidAddresses = Enumerable.Range(0, 256)
                .Where(i => DefaultMapping.All(map => map.address != i));

            foreach (var invalidAddress in invalidAddresses)
            {
                Assert.Equal(Mcp23PortRegister.Invalid, registerMap.GetRegisterAtAddress((byte) invalidAddress));
            }
        }

        [Fact]
        public void GetRegisterAtAddressReturnsRegister()
        {
            var registerMap = new Mcp23PortRegisterMap(DefaultMapping);

            foreach (var (address, register) in DefaultMapping)
            {
                Assert.Equal(register, registerMap.GetRegisterAtAddress(address));
            }
        }

        [Fact]
        public void MappingInvalidMultipleTimesDoesNotThrow()
        {
            var mapping = new[]
            {
                (address: (byte) 0x01, Mcp23PortRegister.Invalid),
                (address: (byte) 0x02, Mcp23PortRegister.Invalid)
            };

            var registerMap = new Mcp23PortRegisterMap(mapping);
        }
    }
}
