using System;
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Hmc5883
    {
        /// <summary>
        /// Measurement configuration.
        /// </summary>
        public enum MeasurementConfigurations : byte
        {
            Normal = 0b_0000_0000,
            PositiveBiasConfiguration = 0b_0000_0001,
            NegativeBias = 0b_0000_0010
        }

    }
}
