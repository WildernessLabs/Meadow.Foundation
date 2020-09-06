using System;
using System.Collections.Generic;
using Meadow.Foundation.ICs.IOExpanders.Device;
using Meadow.Foundation.ICs.IOExpanders.Ports;
using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Provide an interface to connect to a MCP23x17 port expander.
    /// </summary>
    public class Mcp23x17 : Mcp23x, IMcp23x17
    {
        /// <inheritdoc />
        public IMcpGpioPort PortAPins => Ports[0];

        /// <inheritdoc />
        public IMcpGpioPort PortBPins => Ports[1];

        /// <inheritdoc />
        public (byte portA, byte portB) ReadAllPorts()
        {
            lock (ConfigurationLock)
            {
                for (var i = 0; i < IodirState.Length; i++)
                {
                    if (IodirState[0] == 0xFF)
                    {
                        continue;
                    }

                    IodirState[i] = 0xFF;
                    WriteRegister(Mcp23PortRegister.IODirectionRegister, i, 0xFF);
                }

                var read = ReadFromAllPorts(Mcp23PortRegister.GPIORegister);
                return (portA: read[0], portB: read[1]);
            }
        }

        /// <inheritdoc />
        public byte ReadPort(IMcpGpioPort port)
        {
            // throws if port is not a member of Ports
            var portIndex = Ports.GetPortIndex(port);
            return ReadPort(portIndex);
        }

        /// <inheritdoc />
        public byte ReadPort(int port)
        {
            if (port < 0 || port >= Ports.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(port),
                    port,
                    $"Port index is not in the valid range for ports on this device [0, {Ports.Count - 1}]");
            }

            lock (ConfigurationLock)
            {
                if (IodirState[0] != 0xFF)
                {
                    IodirState[0] = 0xFF;
                    WriteRegister(Mcp23PortRegister.IODirectionRegister, port, 0xFF);
                }

                return ReadRegister(Mcp23PortRegister.GPIORegister, port);
            }
        }

        /// <inheritdoc />
        public new void SetBankConfiguration(BankConfiguration bank)
        {
            base.SetBankConfiguration(bank);
        }

        /// <inheritdoc />
        public void WriteAllPorts(byte portAMask, byte portBMask)
        {
            lock (ConfigurationLock)
            {
                for (var i = 0; i < IodirState.Length; i++)
                {
                    if (IodirState[0] == 0x00)
                    {
                        continue;
                    }

                    IodirState[i] = 0xFF;
                    WriteRegister(Mcp23PortRegister.IODirectionRegister, i, 0x00);
                }

                WriteRegisters(
                    (Mcp23PortRegister.OutputLatchRegister, 0, portAMask),
                    (Mcp23PortRegister.OutputLatchRegister, 1, portBMask));
            }
        }

        /// <inheritdoc />
        public void WritePort(IMcpGpioPort port, byte mask)
        {
            // throws if port is not a member of Ports
            var portIndex = Ports.GetPortIndex(port);
            WritePort(portIndex, mask);
        }

        /// <inheritdoc />
        public void WritePort(int port, byte mask)
        {
            if (port < 0 || port >= Ports.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(port),
                    port,
                    $"Port index is not in the valid range for ports on this device [0, {Ports.Count - 1}]");
            }

            lock (ConfigurationLock)
            {
                if (IodirState[0] != 0x00)
                {
                    IodirState[0] = 0x00;
                    WriteRegister(Mcp23PortRegister.IODirectionRegister, port, 0x00);
                }

                WriteRegister(Mcp23PortRegister.OutputLatchRegister, port, mask);
            }
        }

        #region Constructors

        /// <summary>
        /// Instantiates an Mcp23008 on the specified I2C bus using the appropriate
        /// peripheral address based on the pin settings. Use this method if you
        /// don't want to calculate the address.
        /// </summary>
        /// <param name="i2cBus"></param>
        /// <param name="pinA0">Whether or not Address0 pin is pulled high.</param>
        /// <param name="pinA1">Whether or not Address1 pin is pulled high.</param>
        /// <param name="pinA2">Whether or not Address2 pin is pulled high.</param>
        public Mcp23x17(
            II2cBus i2cBus,
            bool pinA0,
            bool pinA1,
            bool pinA2)
            : this(i2cBus, McpAddressTable.GetAddressFromPins(pinA0, pinA1, pinA2))
        {
            // nothing goes here
        }

        /// <summary>
        /// Instantiates an Mcp23008 on the specified I2C bus using the appropriate
        /// peripheral address based on the pin settings. Use this method if you
        /// don't want to calculate the address.
        /// </summary>
        /// <param name="i2cBus"></param>
        /// <param name="pinA0">Whether or not Address0 pin is pulled high.</param>
        /// <param name="pinA1">Whether or not Address1 pin is pulled high.</param>
        /// <param name="pinA2">Whether or not Address2 pin is pulled high.</param>
        /// <param name="interruptPort">
        /// Optional IDigitalInputPort used to support
        /// interrupts. The MCP will notify a single port for an interrupt on
        /// any input configured pin. The driver takes care of looking up which
        /// pin the interrupt occurred on, and will raise it on that port, if a port
        /// is used.
        /// </param>
        public Mcp23x17(
            II2cBus i2cBus,
            IDigitalInputPort interruptPort,
            bool pinA0,
            bool pinA1,
            bool pinA2)
            : this(i2cBus, interruptPort, McpAddressTable.GetAddressFromPins(pinA0, pinA1, pinA2))
        {
            // nothing goes here
        }

        /// <summary>
        /// Instantiates an Mcp23008 on the specified I2C bus, with the specified
        /// peripheral address.
        /// </summary>
        /// <param name="i2cBus"></param>
        /// <param name="address"></param>
        public Mcp23x17(
            II2cBus i2cBus,
            byte address = McpAddressTable.DefaultDeviceAddress) :
            // use the internal constructor that takes an IMcpDeviceComms
            this(
                new I2cMcpDeviceComms(i2cBus, address),
                new IDigitalInputPort[0])
        {
        }

        /// <summary>
        /// Instantiates an Mcp23008 on the specified I2C bus, with the specified
        /// peripheral address.
        /// </summary>
        /// <param name="i2cBus"></param>
        /// <param name="address"></param>
        public Mcp23x17(
            II2cBus i2cBus,
            IDigitalInputPort interruptPort,
            byte address = McpAddressTable.DefaultDeviceAddress) :
            // use the internal constructor that takes an IMcpDeviceComms
            this(
                new I2cMcpDeviceComms(i2cBus, address),
                new[] { interruptPort })
        {
        }

        public Mcp23x17(
            ISpiBus spiBus,
            IDigitalOutputPort chipSelect,
            bool pinA0,
            bool pinA1,
            bool pinA2) : this(
            new SpiMcpDeviceComms(spiBus, chipSelect, McpAddressTable.GetAddressFromPins(pinA0, pinA1, pinA2)),
            new IDigitalInputPort[0])
        {
        }

        public Mcp23x17(
            ISpiBus spiBus,
            IDigitalOutputPort chipSelect,
            IDigitalInputPort interruptPort,
            bool pinA0,
            bool pinA1,
            bool pinA2) : this(
            new SpiMcpDeviceComms(spiBus, chipSelect, McpAddressTable.GetAddressFromPins(pinA0, pinA1, pinA2)),
            new[] { interruptPort })
        {
        }

        public Mcp23x17(
            ISpiBus spiBus,
            IDigitalOutputPort chipSelect,
            byte address = McpAddressTable.DefaultDeviceAddress) : this(
            new SpiMcpDeviceComms(spiBus, chipSelect, address),
            new IDigitalInputPort[0])
        {
        }

        public Mcp23x17(
            ISpiBus spiBus,
            IDigitalOutputPort chipSelect,
            IDigitalInputPort interruptPort,
            byte address = McpAddressTable.DefaultDeviceAddress) : this(
            new SpiMcpDeviceComms(spiBus, chipSelect, address),
            new[] { interruptPort })
        {
        }

        protected Mcp23x17(
            IMcpDeviceComms mcpDeviceComms,
            IList<IDigitalInputPort> interrupts) : base(
            mcpDeviceComms,
            new Mcp23xPorts(new McpGpioPort("GPA"), new McpGpioPort("GPB")),
            Mcp23x17RegisterMap.Instance,
            interrupts)
        {
        }

        #endregion
    }
}
