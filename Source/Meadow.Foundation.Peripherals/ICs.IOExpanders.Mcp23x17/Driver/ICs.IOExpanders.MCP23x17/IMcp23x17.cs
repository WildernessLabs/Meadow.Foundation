using Meadow.Foundation.ICs.IOExpanders.Ports;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public interface IMcp23x17 : IMcp23x
    {
        /// <summary>
        /// The list of pins available on the Port A of the MCP23x17.
        /// </summary>
        IMcpGpioPort PortAPins { get; }

        /// <summary>
        /// The list of pins available on the Port B of the MCP23x17.
        /// </summary>
        IMcpGpioPort PortBPins { get; }

        /// <summary>
        /// Reads a byte value from all pins on all ports.
        /// little-endian; the least significant bit is the value of GP0.
        /// So a byte value of 0x60, or  0110 0000, means that pins GP5 and GP6 are high.
        /// If any pin on the port is currently configured
        /// as an output, this will change the configuration.
        /// </summary>
        /// <returns>A little-endian byte mask of the pin values for each port.</returns>
        (byte portA, byte portB) ReadAllPorts();

        /// <summary>
        /// Reads a byte value from all of the pins on the specified port.
        /// little-endian; the least significant bit is the value of GP0.
        /// So a byte value of 0x60, or  0110 0000, means that pins GP5 and GP6 are high.
        /// If any pin on the port is currently configured
        /// as an output, this will change the configuration.
        /// </summary>
        /// <param name="port"></param>
        /// <returns>A little-endian byte mask of the pin values.</returns>
        byte ReadPort(IMcpGpioPort port);

        /// <summary>
        /// Reads a byte value from all of the pins on the specified port.
        /// little-endian; the least significant bit is the value of GP0.
        /// So a byte value of 0x60, or  0110 0000, means that pins GP5 and GP6 are high.
        /// If any pin on the port is currently configured
        /// as an output, this will change the configuration.
        /// </summary>
        /// <param name="port"></param>
        /// <returns>A little-endian byte mask of the pin values.</returns>
        byte ReadPort(int port);

        /// <summary>
        /// Change the configuration of IOCON.Bank
        /// </summary>
        /// <param name="bank">The new setting for IOCON.Bank</param>
        /// <remarks>
        /// Different configurations can have different effects on performance.
        /// When creating this instance an educated guess is made to set the optimal configuration
        /// based on how many interrupt ports were provided.
        /// Refer to datasheet for more details.
        /// </remarks>
        void SetBankConfiguration(BankConfiguration bank);

        /// <summary>
        /// Outputs a byte value across all pins on all ports by writing directly
        /// to the output latch (OLAT) register.
        /// If any pin is currently configured
        /// as an input, this will change the configuration.
        /// </summary>
        /// <param name="portAMask"></param>
        /// <param name="portBMask"></param>
        void WriteAllPorts(byte portAMask, byte portBMask);

        /// <summary>
        /// Outputs a byte value across all pins of the specified port by writing directly
        /// to the output latch (OLAT) register.
        /// If any pin on the port is currently configured
        /// as an input, this will change the configuration.
        /// </summary>
        /// <param name="port"></param>
        /// <param name="mask"></param>
        void WritePort(IMcpGpioPort port, byte mask);

        /// <summary>
        /// Outputs a byte value across all pins of the specified port by writing directly
        /// to the output latch (OLAT) register.
        /// If any pin on the port is currently configured
        /// as an input, this will change the configuration.
        /// </summary>
        /// <param name="port"></param>
        /// <param name="mask"></param>
        void WritePort(int port, byte mask);
    }
}
