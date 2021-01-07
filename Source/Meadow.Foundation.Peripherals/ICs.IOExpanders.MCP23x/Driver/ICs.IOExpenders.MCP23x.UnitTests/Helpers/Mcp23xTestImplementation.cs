using System;
using System.Collections.Generic;
using System.Linq;
using Meadow.Foundation.ICs.IOExpanders.Device;
using Meadow.Foundation.ICs.IOExpanders.Ports;
using Meadow.Hardware;
using Moq;

namespace Meadow.Foundation.ICs.IOExpanders.UnitTests.Helpers
{
    public class Mcp23xTestImplementation : IOExpanders.Mcp23x
    {
        public Mcp23xTestImplementation(IMcpDeviceComms mcpDeviceComms, IMcpGpioPorts ports,
            IMcp23RegisterMap registerMap, IList<IDigitalInputPort> interrupts) : base(mcpDeviceComms, ports,
            registerMap, interrupts)
        {
        }

        public new BankConfiguration BankConfiguration => base.BankConfiguration;
        public new object ConfigurationLock => base.ConfigurationLock;

        public new byte[] GppuState => base.GppuState;

        public new IList<IDigitalInputPort> Interrupts => base.Interrupts;

        public new bool InterruptSupported => base.InterruptSupported;

        public new byte[] IodirState => base.IodirState;

        public new bool MirroredInterrupts => base.MirroredInterrupts;

        public new byte[] OlatState => base.OlatState;

        public new byte IoconState => base.IoconState;

        public static Mcp23xTestImplementation Create(int portCount, int interruptCount,
            Action<Mock<IMcpDeviceComms>> deviceCommsMockOverride = null,
            Action<Mock<IMcpGpioPorts>, List<IMcpGpioPort>> portsMockOverride = null,
            Action<Mock<IMcp23RegisterMap>> registerMapMockOverride = null,
            Action<List<Mock<IDigitalInputPort>>> interruptsMockOverride = null)
        {
            var deviceCommsMock = new Mock<IMcpDeviceComms>();
            deviceCommsMockOverride?.Invoke(deviceCommsMock);

            var portsMock = new Mock<IMcpGpioPorts>();
            var portsImpl = new List<IMcpGpioPort>();
            for (var i = 0; i < portCount; i++) portsImpl.Add(new McpGpioPort($"GP_{i}_"));

            portsMock.Setup(m => m.GetEnumerator()).Returns(portsImpl.GetEnumerator());
            portsMock.SetupGet(m => m.Count).Returns(portsImpl.Count);
            portsMockOverride?.Invoke(portsMock, portsImpl);

            var registerMapMock = new Mock<IMcp23RegisterMap>();
            registerMapMockOverride?.Invoke(registerMapMock);

            var interrupts = new List<Mock<IDigitalInputPort>>();
            for (var i = 0; i < interruptCount; i++)
            {
                var interruptMock = new Mock<IDigitalInputPort>();
                interruptMock.SetupGet(m => m.InterruptMode).Returns(InterruptMode.EdgeFalling);
                interrupts.Add(interruptMock);
            }

            interruptsMockOverride?.Invoke(interrupts);

            var mcp23x = new Mcp23xTestImplementation(deviceCommsMock.Object, portsMock.Object, registerMapMock.Object,
                interrupts.Select(i => i.Object).ToList());

            mcp23x.DeviceCommsMock = deviceCommsMock;
            mcp23x.PortsMock = portsMock;
            mcp23x.RegisterMapMock = registerMapMock;
            mcp23x.InterruptsMock = interrupts;

            return mcp23x;
        }

        public Mock<IMcpDeviceComms> DeviceCommsMock { get; private set; }

        public Mock<IMcpGpioPorts> PortsMock { get; private set; }

        public Mock<IMcp23RegisterMap> RegisterMapMock { get; private set; }

        public List<Mock<IDigitalInputPort>> InterruptsMock { get; private set; }

        public new void Initialize()
        {
            base.Initialize();
        }

        public new bool IsValidPin(IPin pin)
        {
            return base.IsValidPin(pin);
        }

        public new void SetBankConfiguration(BankConfiguration bank)
        {
            base.SetBankConfiguration(bank);
        }

        public new byte[] ReadFromAllPorts(Mcp23PortRegister register)
        {
            return base.ReadFromAllPorts(register);
        }

        public new byte[][] ReadFromAllPorts(params Mcp23PortRegister[] registers)
        {
            return base.ReadFromAllPorts(registers);
        }

        public new byte ReadRegister(Mcp23PortRegister register, int port)
        {
            return base.ReadRegister(register, port);
        }

        public new byte[] ReadRegisters(params (Mcp23PortRegister register, int port)[] addresses)
        {
            return base.ReadRegisters(addresses);
        }

        public new byte[] ReadRegisters(int port, params Mcp23PortRegister[] registers)
        {
            return base.ReadRegisters(port, registers);
        }

        public new void WriteRegister(Mcp23PortRegister register, int port, byte value)
        {
            base.WriteRegister(register, port, value);
        }

        public new void WriteRegisters(params (Mcp23PortRegister register, int port, byte value)[] writeOps)
        {
            base.WriteRegisters(writeOps);
        }

        public new void WriteRegisters(int port, params (Mcp23PortRegister register, byte value)[] writeOps)
        {
            base.WriteRegisters(port, writeOps);
        }

        public new void WriteToAllPorts(Mcp23PortRegister register, byte value)
        {
            base.WriteToAllPorts(register, value);
        }

        public new void WriteToAllPorts(params (Mcp23PortRegister register, byte value)[] writeOps)
        {
            base.WriteToAllPorts(writeOps);
        }
    }
}