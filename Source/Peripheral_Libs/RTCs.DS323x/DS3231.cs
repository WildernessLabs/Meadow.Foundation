using Meadow.Hardware;
using Meadow.Foundation.Communications;
using Meadow.Hardware.Communications;

namespace Meadow.Foundation.RTCs
{
    /// <summary>
    /// Create a new DS3231 Real Time Clock object.
    /// </summary>
    public class DS3231 : DS323x
    {
        #region Constructors

        /// <summary>
        /// Default constructor is private to prevent the developer from calling it.
        /// </summary>
        private DS3231() { }

        /// <summary>
        /// Create a new MAG3110 object using the default parameters for the component.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="interruptPin">Digital pin connected to the alarm interrupt pin on the RTC.</param>
        public DS3231(IIODevice device, IPin interruptPin) : 
            this (device.CreateDigitalInputPort(interruptPin)) { }

        /// <summary>
        /// Create a new MAG3110 object using the default parameters for the component.
        /// </summary>
        /// <param name="address">Address of the DS3231 (default = 0x68).</param>
        /// <param name="speed">Speed of the I2C bus (default = 100 KHz).</param>
        /// <param name="interruptPort">Digital port connected to the alarm interrupt pin on the RTC.</param>
        // TODO: revisit; `DigitalPin.Empty`?
        public DS3231(IDigitalInputPort interruptPort, byte address = 0x68, ushort speed = 100)
        {
            _ds323x = new I2cBus(address, speed);

            // TODO: i changed this from GPIO_NONE
            // samples will need to pass null
            if (interruptPort != null)
            {
                InterruptPin = interruptPort;
            }
        }

        #endregion Constructors
    }
}