using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Hardware;

namespace Meadow.Foundation.mikroBUS
{
    /// <summary>
    /// Represents a mikroBUS Thermo 13 Click board
    /// </summary>
    public class CThermo13 : Bh1900Nux
    {
        /// <summary>
        /// Creates a Thermo13 driver
        /// </summary>
        /// <param name="i2cBus"></param>
        public CThermo13(II2cBus i2cBus)
            : base(i2cBus, Addresses.Default)
        {
        }
    }
}