using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{

    public partial class Sc16is7x2
    {
        /// <summary>
        /// The SC16is7x2 SerialPortName
        /// </summary>
        public class Sc16SerialPortName : SerialPortName
        {
            internal Sc16SerialPortName(string friendlyName, string systemName, Sc16is7x2 controller)
            : base(friendlyName, systemName, controller)
            {
            }

        }
    }
}