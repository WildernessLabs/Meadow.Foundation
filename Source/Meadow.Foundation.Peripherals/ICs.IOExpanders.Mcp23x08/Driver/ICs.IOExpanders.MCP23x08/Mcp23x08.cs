using System.Collections.Generic;
using Meadow.Foundation.ICs.IOExpanders.Device;
using Meadow.Foundation.ICs.IOExpanders.Ports;
using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Provide an interface to connect to a MCP23x08 port expander.
    /// </summary>
    public class Mcp23x08 : Mcp23x, IMcp23x08
    {
        /// <inheritdoc />
        public IMcpGpioPort Pins => Ports[0];

        /// <inheritdoc />
        public byte ReadPort()
        {
            lock (ConfigurationLock)
            {
                if (IodirState[0] != 0xFF)
                {
                    IodirState[0] = 0xFF;
                    WriteRegister(Mcp23PortRegister.IODirectionRegister, 0, 0xFF);
                }

                return ReadRegister(Mcp23PortRegister.GPIORegister, 0);
            }
        }

        /// <inheritdoc />
        public void WritePort(byte mask)
        {
            lock (ConfigurationLock)
            {
                if (IodirState[0] != 0x00)
                {
                    IodirState[0] = 0x00;
                    WriteRegister(Mcp23PortRegister.IODirectionRegister, 0, 0x00);
                }

                WriteRegister(Mcp23PortRegister.OutputLatchRegister, 0, mask);
            }
        }

        #region constructors

        /// <summary>
        /// Instantiates an Mcp23008 on the specified I2C bus using the appropriate
        /// peripheral address based on the pin settings. Use this method if you
        /// don't want to calculate the address.
        /// </summary>
        /// <param name="i2cBus"></param>
        /// <param name="pinA0">Whether or not Address0 pin is pulled high.</param>
        /// <param name="pinA1">Whether or not Address1 pin is pulled high.</param>
        /// <param name="pinA2">Whether or not Address2 pin is pulled high.</param>
        public Mcp23x08(
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
        public Mcp23x08(
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
        public Mcp23x08(
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
        public Mcp23x08(
            II2cBus i2cBus,
            IDigitalInputPort interruptPort,
            byte address = McpAddressTable.DefaultDeviceAddress) :
            // use the internal constructor that takes an IMcpDeviceComms
            this(
                new I2cMcpDeviceComms(i2cBus, address),
                new[] { interruptPort })
        {
        }

        public Mcp23x08(
            ISpiBus spiBus,
            IDigitalOutputPort chipSelect) : this(new SpiMcpDeviceComms(spiBus, chipSelect), new IDigitalInputPort[0])
        {
        }

        public Mcp23x08(
            ISpiBus spiBus,
            IDigitalOutputPort chipSelect,
            IDigitalInputPort interruptPort) : this(new SpiMcpDeviceComms(spiBus, chipSelect), new[] { interruptPort })
        {
        }

        protected Mcp23x08(
            IMcpDeviceComms mcpDeviceComms,
            IList<IDigitalInputPort> interrupts) : base(
            mcpDeviceComms,
            new Mcp23x08Ports(), 
            Mcp23x08RegisterMap.Instance,
            interrupts)
        {
        }

        #endregion
    }
}
