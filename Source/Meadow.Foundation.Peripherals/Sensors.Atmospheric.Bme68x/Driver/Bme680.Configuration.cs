namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Bme680
    {
        public class Configuration
        {
            public Oversample TemperatureOversample { get; set; } = Oversample.OversampleX16;
            public Oversample PressureOversample { get; set; } = Oversample.OversampleX16;
            public Oversample HumidityOversample { get; set; } = Oversample.OversampleX16;
        }
    }
}