namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Th02
    {
        /// <summary>
        ///     Start measurement bit in the configuration register.
        /// </summary>
        private const byte StartMeasurement = 0x01;

        /// <summary>
        ///     Measure temperature bit in the configuration register.
        /// </summary>
        private const byte MeasureTemperature = 0x10;

        /// <summary>
        ///     Heater control bit in the configuration register.
        /// </summary>
        private const byte HeaterOnBit = 0x02;

        /// <summary>
        ///     Mask used to turn the heater off.
        /// </summary>
        private const byte HeaterMask = 0xfd;

        /// <summary>
        ///     Minimum value that should be used for the polling frequency.
        /// </summary>
        public const ushort MinimumPollingPeriod = 200;
    }
}
