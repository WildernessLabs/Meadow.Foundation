namespace Meadow.Foundation.ICs.IOExpanders.Ports
{
    /// <summary>
    /// Bank configuration changes how the registers are mapped.
    /// Only has an affect on MCP23x17 chips (and I guess any future chip that includes 2+ GPIO ports).
    /// </summary>
    /// <remarks>
    /// As changing this bit can break future calls read/writes to the device, it must be done in isolation.
    /// Please use the dedicated SetBank() method to change this value as it uses locks to prevent errors.
    /// </remarks>
    public enum BankConfiguration : byte
    {
        /// <summary>
        /// The A/B registers are paired. For example (for the MCP23x17), IODIRA is mapped to address 0x00 and IODIRB is mapped to
        /// the next address
        /// (address 0x01).
        /// </summary>
        /// <remarks>
        /// Paired is ideal when you expect to read all GPIO ports at once and will result in faster reads.
        /// The common use case is when you have a single interrupt pin for all GPIO ports.
        /// </remarks>
        Paired = 0x00,

        /// <summary>
        /// The registers associated with each port are segregated. (For the MCP23x17), registers associated with PORTA are mapped
        /// from address 0x00 - 0x0A and registers associated with PORTB are mapped from 0x10 - 0x1A
        /// </summary>
        /// <remarks>
        /// Segregated is ideal for reading inputs from single GPIO ports and will result in faster reads in this case.
        /// The common use case is when you have a different interrupt pin for each GPIO port.
        /// </remarks>
        Segregated = 0x01
    }
}
