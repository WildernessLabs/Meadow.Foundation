namespace Meadow.Foundation.Sensors.Atmospheric
{
    partial class Bme68x
    {
        /// <summary>
        /// The oversampling configuration for oversampling performed by the Bme68x
        /// </summary>
        public class Configuration
        {
            /// <summary>
            /// The temperature oversampling mode
            /// </summary>
            public Oversample TemperatureOversample { get; set; } = Oversample.OversampleX8;

            /// <summary>
            /// The pressure oversampling mode
            /// </summary>
            public Oversample PressureOversample { get; set; } = Oversample.OversampleX8;

            /// <summary>
            /// The humidity oversampling mode
            /// </summary>
            public Oversample HumidityOversample { get; set; } = Oversample.OversampleX8;
        }
    }
}