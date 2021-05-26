using System;
namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Bme280
    {
        public class Configuration
        {
            /// <summary>
            ///     Temperature over sampling configuration.
            /// </summary>
            public Oversample TemperatureOverSampling { get; set; }

            /// <summary>
            ///     Pressure over sampling configuration.
            /// </summary>
            public Oversample PressureOversampling { get; set; }

            /// <summary>
            ///     Humidity over sampling configuration.
            /// </summary>
            public Oversample HumidityOverSampling { get; set; }

            /// <summary>
            ///     Set the operating mode for the sensor.
            /// </summary>
            public Modes Mode { get; set; }

            /// <summary>
            ///     Set the standby period for the sensor.
            /// </summary>
            public StandbyDuration Standby { get; set; }

            /// <summary>
            ///     Determine the time constant for the IIR filter.
            /// </summary>
            /// <remarks>
            ///     See section 3.44 of the datasheet for more informaiton.
            /// </remarks>
            public FilterCoefficient Filter { get; set; }
        }
    }
}
