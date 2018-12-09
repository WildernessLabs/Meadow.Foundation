using Meadow.Hardware;
using Meadow.Foundation.Communications;
using Meadow.Hardware.Communications;

namespace Meadow.Foundation.RTCs
{
    /// <summary>
    ///     Create a new DS3231 Real Time Clock object.
    /// </summary>
    public class DS3231 : DS323x
    {
        #region Constructors

        /// <summary>
        ///     Default constructor is private to prevent the developer from calling it.
        /// </summary>
        private DS3231()
        {
        }

        /// <summary>
        ///     Create a new MAG3110 object using the default parameters for the component.
        /// </summary>
        /// <param name="address">Address of the DS3231 (default = 0x68).</param>
        /// <param name="speed">Speed of the I2C bus (default = 100 KHz).</param>
        /// <param name="interruptPin">Digital pin connected to the alarm interrupt pin on the RTC.</param>
        // TODO: revisit; `DigitalPin.Empty`?
        public DS3231(byte address = 0x68, ushort speed = 100, IDigitalPin interruptPin = null /*Pins.GPIO_NONE*/)
        {
            _ds323x = new I2CBus(address, speed);

            // TODO: i changed this from GPIO_NONE
            // samples will need to pass null
            if (interruptPin != null)
            {
                InterruptPin = interruptPin;
            }
        }

        #endregion Constructors
    }
}