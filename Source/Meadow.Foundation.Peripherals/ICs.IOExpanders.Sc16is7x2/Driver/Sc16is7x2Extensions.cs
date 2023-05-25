
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
        /// 
        /// </summary>
        /// <param name="portName"></param>
        /// <param name="baudRate"></param>
        /// <param name="dataBits"></param>
        /// <param name="parity"></param>
        /// <param name="stopBits"></param>
        /// <returns></returns>
        public static ISerialPort CreateRs485SerialPort(
            this Sc16SerialPortName portName,
            int baudRate = 9600,
            int dataBits = 8,
            Parity parity = Parity.None,
            StopBits stopBits = StopBits.One)
        {
            return (portName.SerialController as Sc16is7x2).CreateRs485SerialPort(portName, baudRate, dataBits, parity, stopBits);
        }
    }
}