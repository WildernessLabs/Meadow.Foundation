using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Bme680
    {
        public class SensorSettings
        {
            public Oversample TemperatureOversample { get; set; } = Oversample.OversampleX16;
            public Oversample PressureOversample { get; set; } = Oversample.OversampleX16;
            public Oversample HumidityOversample { get; set; } = Oversample.OversampleX16;
            public TemperatureUnit TemperatureUnit { get; set; } = TemperatureUnit.C;
            public PressureUnit PressureUnit { get; set; } = PressureUnit.Pa;

            /// <summary>
            /// Get the measurements in the standard metric system
            /// </summary>
            public static SensorSettings SI => new SensorSettings();

            /// <summary>
            /// Get the measurements in the United States Customary Units
            /// </summary>
            public static SensorSettings USCU => new SensorSettings() { PressureUnit = PressureUnit.Psia, TemperatureUnit = TemperatureUnit.F };
        }
    }
}
