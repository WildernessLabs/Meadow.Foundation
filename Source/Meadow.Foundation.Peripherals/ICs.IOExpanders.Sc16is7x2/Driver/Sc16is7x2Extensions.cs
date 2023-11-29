
using Meadow.Hardware;
using static Meadow.Foundation.ICs.IOExpanders.Sc16is7x2;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Convenience extension methods for the Sc16is7x2 class
    /// </summary>
    public static class Sc16is7x2Extensions
    {
        /// <summary>
        /// Creates an RS485 Serial Port
        /// </summary>
        /// <param name="portName">The Sc16SerialPortName name of the channel to create</param>
        /// <param name="baudRate">The baud rate used in communication</param>
        /// <param name="dataBits">The data bits used in communication</param>
        /// <param name="parity">The parity used in communication</param>
        /// <param name="stopBits">The stop bits used in communication</param>
        /// <param name="invertDE">Set to true to invert the logic (active high) driver enable output signal</param>
        public static ISerialPort CreateRs485SerialPort(
            this Sc16SerialPortName portName,
            int baudRate = 9600,
            int dataBits = 8,
            Parity parity = Parity.None,
            StopBits stopBits = StopBits.One,
            bool invertDE = false)
        {
            return (portName.SerialController as Sc16is7x2)!.CreateRs485SerialPort(portName, baudRate, dataBits, parity, stopBits, invertDE);
        }
    }
}