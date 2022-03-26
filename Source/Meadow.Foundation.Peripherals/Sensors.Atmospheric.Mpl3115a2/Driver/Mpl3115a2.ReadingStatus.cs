using System;
namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Mpl3115a2
    {
        /// <summary>
        /// Status register bits.
        /// </summary>
        private enum ReadingStatus : byte
        {
            NewTemperatureDataReady = 0x02,
            NewPressureDataAvailable = 0x04,
            NewTemperatureOrPressureDataReady = 0x08,
            TemperatureDataOverwrite = 0x20,
            PressureDataOverwrite = 0x40,
            PressureOrTemperatureOverwrite = 0x80
        }
    }
}
