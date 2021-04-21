using Meadow.Hardware;
using Meadow.Foundation.Sensors.Gnss;

namespace Meadow.Foundation.FeatherWings
{
    public class GPSWing : Mt3339
    {
        public GPSWing(ISerialMessagePort serialMessagePort)
            : base(serialMessagePort)
        {
        }
    }
}