namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Defines available clock rates for I2C communication
    /// </summary>
    public enum I2CClockRate
    {
        /// <summary>
        /// Standard clock rate of 100kb/sec
        /// </summary>
        Standard = 100000,
        /// <summary>
        /// Fast clock rate of 400kb/sec
        /// </summary>
        Fast = 400000,
        /// <summary>
        /// Fast-Plus clock rate of 1000kb/sec
        /// </summary>
        FastPlus = 1000000,
        /// <summary>
        /// High Speed clock rate of 3.4Mb/sec
        /// </summary>
        HighSpeed = 3400000
    }

}