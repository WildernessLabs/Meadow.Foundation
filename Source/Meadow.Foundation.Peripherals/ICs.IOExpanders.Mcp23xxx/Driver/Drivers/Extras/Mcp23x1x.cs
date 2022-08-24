using Meadow.Hardware;
using System.Linq;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represent an MCP23x1x I2C port expander
    /// </summary>
    public abstract partial class Mcp23x1x : Mcp23xxx
    {
        /// <summary>
        /// MCP23x17 pin definitions
        /// </summary>
        public PinDefinitions Pins { get; } = new PinDefinitions();

        /// <summary>
        /// The number of IO pins avaliable on the device
        /// </summary>
        public override int NumberOfPins => 16;

        /// <summary>
        /// Is the pin valid for this device instance
        /// </summary>
        /// <param name="pin">The IPin to validate</param>
        /// <returns>True if pin is valid</returns>
        protected override bool IsValidPin(IPin pin) => Pins.AllPins.Contains(pin);

        /// <summary>
        /// Creates an Mcp23x1x object
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="address">The I2C address</param>
        /// <param name="interruptPort">The interrupt port</param>
        protected Mcp23x1x(II2cBus i2cBus, byte address = 32, IDigitalInputPort interruptPort = null) 
            : base(i2cBus, address, interruptPort)
        {
        }

        /// <summary>
        /// Creates an Mcp23x1x object
        /// </summary>
        /// <param name="spiBus">The SPI bus</param>
        /// <param name="chipSelectPort">The chip select port</param>
        /// <param name="interruptPort">The interrupt port</param>
        protected Mcp23x1x(ISpiBus spiBus, IDigitalOutputPort chipSelectPort, IDigitalInputPort interruptPort = null) : base(spiBus, chipSelectPort, interruptPort)
        {
        }

        /// <summary>
        /// Get pin reference by name
        /// </summary>
        /// <param name="pinName">The pin name as a string</param>
        /// <returns>IPin reference if found</returns>
        public override IPin GetPin(string pinName)
        {
            return Pins.AllPins.FirstOrDefault(p => p.Name == pinName || p.Key.ToString() == p.Name);
        }
    }
}