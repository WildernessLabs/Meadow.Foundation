namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Mcp23xxx
    {
        /// <summary>
        /// Mcp23xxx registers class
        /// </summary>
        public static class Registers
        {
            /// <summary>
            /// IODIR Controls the direction of the data I/O
            /// </summary>
            /// <remarks>
            /// When a bit is set, the corresponding pin becomes an input. When
            /// a bit is clear, the corresponding pin becomes an output.
            /// </remarks>
            public const byte IODIR_IODirection = 0x00; //IODIR

            /// <summary>
            /// IPOL. The IPOL register allows the user to configure the
            /// polarity on the corresponding GPIO port bits.
            /// </summary>
            /// <remarks>
            /// If a bit is set, the corresponding GPIO register bit will
            /// reflect the inverted value on the pin.
            /// </remarks>
            public const byte IPOL_InputPolarity = 0x01; //IPOL

            /// <summary>
            /// GPINTEN. The GPINTEN register controls the interrupt-on-change
            /// feature for each pin.
            /// </summary>
            /// <remarks>
            /// If a bit is set, the corresponding pin is enabled for
            /// interrupt-on-change. The DEFVAL and INTCON registers must also
            /// be configured if any pins are enabled for interrupt-on-change.
            /// </remarks>
            public const byte GPINTEN_InterruptOnChange = 0x02; //GPINTEN

            /// <summary>
            /// DEFVAL. The default comparison value is configured in the DEFVAL
            /// register. If enabled (via GPINTEN and INTCON) to compare against
            /// the DEFVAL register, an opposite value on the associated pin
            /// will cause an interrupt to occur.
            /// </summary>
            public const byte DEFVAL_DefaultComparisonValue = 0x03; //DEFVAL

            /// <summary>
            /// INTCON. 
            /// </summary>
            /// <remarks>
            /// The INTCON register controls how the associated pin value is
            /// compared for the interrupt-on-change feature. If a bit is set,
            /// the corresponding I/O pin is compared against the associated bit
            /// in the DEFVAL register. If a bit value is clear, the
            /// corresponding I/O pin is compared against the previous value.
            /// </remarks>
            public const byte INTCON_InterruptControl = 0x04; //INTCON

            /// <summary>
            /// IOCON
            /// </summary>
            /// <remarks>
            /// The IOCON register contains several bits for configuring the device:
            /// * The SequentialOperation (SEQOP) controls the incrementing function
            /// of the address pointer.If the address pointer is disabled, the
            /// address pointer does not automatically increment after each byte
            /// is clocked during a serial transfer.This feature is useful when
            /// it is desired to continuously poll(read) or modify(write) a register.
            /// * The SlewRate (DISSLW) bit controls the slew rate function on the
            /// SDA pin. If enabled, the SDA slew rate will be controlled when
            /// driving from a high to a low.
            /// * The HardwareAddressEnable (HAEN) control bit enables/disables the
            /// hardware address pins (A1, A0) on the MCP23S08. This bit is not
            /// used on the MCP23008. The address pins are always enabled on the
            /// MCP23008.
            /// * The Open-Drain (ODR) control bit enables/disables the INT pin for
            /// open-drain configuration.
            /// * The InterruptPolarity (INTPOL) control bit sets the polarity of
            /// the INT pin. This bit is functional only when the ODR bit is
            /// cleared, configuring the INT pin as active push-pull.
            /// </remarks>
            public const byte IOCON_IOConfiguration = 0x05; //IOCON

            /// <summary>
            /// GPPU. 
            /// </summary>
            /// <remarks>
            /// The GPPU register controls the pull-up resistors for the port
            /// pins. If a bit is set and the corresponding pin is configured as
            /// an input, the corresponding port pin is internally pulled up
            /// with a 100kΩ resistor.
            /// </remarks>
            public const byte GPPU_PullupResistorConfiguration = 0x06; //GPPU

            /// <summary>
            /// INTF
            /// </summary>
            /// <remarks>
            /// The INTF register reflects the interrupt condition on the port
            /// pins of any pin that is enabled for interrupts via the GPINTEN
            /// register. A ‘set’ bit indicates that the associated pin caused
            /// the interrupt.
            /// This register is ‘read-only’. Writes to this register will be
            /// ignored.
            ///
            /// **NOTE**: INTF will always reflect the pin(s) that have an
            /// interrupt condition. For example, one pin causes an interrupt
            /// to occur and is captured in INTCAP and INF. If before clearing
            /// the interrupt another pin changes, which would normally cause an
            /// interrupt, it will be reflected in INTF, but not INTCAP.
            /// </remarks>
            public const byte INTF_InterruptFlag = 0x07; //INTF

            /// <summary>
            /// INTCAP
            /// </summary>
            /// <remarks>
            /// The INTCAP register captures the GPIO port value at the time the
            /// interrupt occurred. The register is ‘read- only’ and is updated
            /// only when an interrupt occurs. The register will remain unchanged
            /// until the interrupt is cleared via a read of INTCAP or GPIO.
            /// </remarks>
            public const byte INTCAP_InterruptCapture = 0x08; //INTCAP

            /// <summary>
            /// GPIO. The GPIO module contains the data port (GPIO), internal
            /// pull up resistors and the Output Latches (OLAT).
            /// </summary>
            /// <remarks>
            /// Reading the GPIO register reads the value on the port. Reading
            /// the OLAT register only reads the OLAT, not the actual value on
            /// the port.
            /// Writing to the GPIO register actually causes a write to the
            /// OLAT. Writing to the OLAT register forces the associated
            /// output drivers to drive to the level in OLAT. Pins configured
            /// as inputs turn off the associated output driver and put it in
            /// high-impedance.
            /// </remarks>
            public const byte GPIO = 0x09; //GPIO

            /// <summary>
            /// OLAT
            /// </summary>
            /// <remarks>
            /// The OLAT register provides access to the output latches. A read
            /// from this register results in a read of the OLAT and not the
            /// port itself. A write to this register modifies the output
            /// latches that modify the pins configured as outputs.
            /// </remarks>
            public const byte OutputLatch = 0x0A; //OLAT
        }
    }
}