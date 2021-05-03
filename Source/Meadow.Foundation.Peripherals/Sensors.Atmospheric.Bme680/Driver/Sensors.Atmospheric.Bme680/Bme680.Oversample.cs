using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Bme680
    {
        /// <summary>
        ///     Valid oversampling values.
        /// </summary>
        /// <remarks>
        ///     000 - Data output set to 0x8000
        ///     001 - Oversampling x1
        ///     010 - Oversampling x2
        ///     011 - Oversampling x4
        ///     100 - Oversampling x8
        ///     101, 110, 111 - Oversampling x16
        /// </remarks>
        public enum Oversample : byte
        {
            Skip = 0,
            OversampleX1,
            OversampleX2,
            OversampleX4,
            OversampleX8,
            OversampleX16
        }
    }
}