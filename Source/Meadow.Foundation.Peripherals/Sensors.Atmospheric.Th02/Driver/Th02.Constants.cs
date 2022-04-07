namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Th02
    {
        /// <summary>
        /// Start measurement bit in the configuration register.
        /// </summary>
        private const byte MeasureHumidity = 0x01;

        /// <summary>
        /// Measure temperature bit in the configuration register.
        /// </summary>
        private const byte MeasureTemperature = 0x11;
    }
}